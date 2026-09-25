using System.Security.Claims;
using MediCore.Api.Data;
using MediCore.Api.Models;
using MediCore.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Controllers
{
    [ApiController]
    [Route("api/documents")]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly MediCoreDbContext _db;
        private readonly IWebHostEnvironment _environment;
        private readonly IPatientAccessService _patientAccessService;

        public DocumentsController(
     MediCoreDbContext db,
     IWebHostEnvironment environment,
     IPatientAccessService patientAccessService)
        {
            _db = db;
            _environment = environment;
            _patientAccessService = patientAccessService;
        }

        // GET: /api/documents
        // Returns only the logged-in patient's normal documents.
        [Authorize(Roles = "Patient")]
        [HttpGet]
        public async Task<ActionResult<List<MedicalDocument>>> GetDocuments()
        {
            var currentUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Unauthorized();
            }

            var documents = await _db.MedicalDocuments
                .AsNoTracking()
                .Where(document =>
                    document.OwnerUserId == currentUserId &&
                    !document.IsDeleted
                )
                .OrderByDescending(document => document.UploadedAt)
                .ToListAsync();

            return Ok(documents);
        }

        // GET: /api/documents/patient/{patientUserId}
        // Returns a patient's active documents only when the logged-in doctor
        // has an active relationship with that patient.
        [Authorize(Roles = "Doctor")]
        [HttpGet("patient/{patientUserId}")]
        public async Task<ActionResult<List<MedicalDocument>>> GetPatientDocuments(
            string patientUserId)
        {
            var doctorUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(doctorUserId))
            {
                return Unauthorized();
            }

            var canAccess = await _patientAccessService
                .CanDoctorAccessPatientAsync(
                    doctorUserId,
                    patientUserId);

            if (!canAccess)
            {
                return Forbid();
            }

            var documents = await _db.MedicalDocuments
                .AsNoTracking()
                .Where(document =>
                    document.OwnerUserId == patientUserId &&
                    !document.IsDeleted)
                .OrderByDescending(document => document.UploadedAt)
                .ToListAsync();

            return Ok(documents);
        }

        // GET: /api/documents/trash
        // Returns only the logged-in patient's trashed documents.
        [Authorize(Roles = "Patient")]
        [HttpGet("trash")]
        public async Task<ActionResult<List<MedicalDocument>>> GetTrashDocuments()
        {
            var currentUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Unauthorized();
            }

            var documents = await _db.MedicalDocuments
                .AsNoTracking()
                .Where(document =>
                    document.OwnerUserId == currentUserId &&
                    document.IsDeleted
                )
                .OrderByDescending(document => document.DeletedAt)
                .ToListAsync();

            return Ok(documents);
        }

        // GET: /api/documents/1
        [Authorize(Roles = "Patient")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<MedicalDocument>> GetDocument(int id)
        {
            var currentUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Unauthorized();
            }

            var document = await _db.MedicalDocuments
                .AsNoTracking()
                .FirstOrDefaultAsync(document =>
                    document.Id == id &&
                    document.OwnerUserId == currentUserId &&
                    !document.IsDeleted
                );

            if (document is null)
            {
                return NotFound();
            }

            return Ok(document);
        }

        // GET: /api/documents/1/file
        // Returns the actual PDF/image only to its owner.
        [Authorize(Roles = "Patient")]
        [HttpGet("{id:int}/file")]
        public async Task<IActionResult> GetDocumentFile(int id)
        {
            var currentUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Unauthorized();
            }

            var document = await _db.MedicalDocuments
                .AsNoTracking()
                .FirstOrDefaultAsync(document =>
                    document.Id == id &&
                    document.OwnerUserId == currentUserId &&
                    !document.IsDeleted
                );

            if (document is null)
            {
                return NotFound("Document not found.");
            }

            var filePath = Path.Combine(
                _environment.ContentRootPath,
                "Uploads",
                document.StoredFileName
            );

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(
                    "The document file no longer exists on the server."
                );
            }

            Response.Headers["Content-Disposition"] =
                $"inline; filename=\"{document.OriginalFileName}\"";

            return PhysicalFile(
                filePath,
                document.ContentType,
                enableRangeProcessing: true
            );
        }

        // POST: /api/documents/upload
        [Authorize(Roles = "Patient")]
        [HttpPost("upload")]
        public async Task<ActionResult<MedicalDocument>> UploadDocument(
            IFormFile file,
            [FromForm] string title,
            [FromForm] string type)
        {
            var currentUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Unauthorized();
            }

            if (file is null || file.Length == 0)
            {
                return BadRequest("File is empty.");
            }

            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest("File type is not allowed.");
            }

            var uploadsPath = Path.Combine(
                _environment.ContentRootPath,
                "Uploads"
            );

            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            var storedFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsPath, storedFileName);

            await using var stream = System.IO.File.Create(filePath);
            await file.CopyToAsync(stream);

            var document = new MedicalDocument
            {
                OwnerUserId = currentUserId,
                Title = title,
                OriginalFileName = file.FileName,
                StoredFileName = storedFileName,
                ContentType = file.ContentType,
                SizeBytes = file.Length,
                Type = type,
                Status = "pending_review",
                Summary = "Uploaded document. AI summary is not generated yet.",
                UploadedAt = DateTime.UtcNow,
                IsDeleted = false,
                DeletedAt = null
            };

            _db.MedicalDocuments.Add(document);
            await _db.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetDocument),
                new { id = document.Id },
                document
            );
        }

        // DELETE: /api/documents/1
        // Soft delete: moves only the owner's document to Trash.
        [Authorize(Roles = "Patient")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> MoveToTrash(int id)
        {
            var currentUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Unauthorized();
            }

            var document = await _db.MedicalDocuments
                .FirstOrDefaultAsync(document =>
                    document.Id == id &&
                    document.OwnerUserId == currentUserId &&
                    !document.IsDeleted
                );

            if (document is null)
            {
                return NotFound(
                    "Document not found or already in Trash."
                );
            }

            document.IsDeleted = true;
            document.DeletedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return NoContent();
        }

        // POST: /api/documents/1/restore
        [Authorize(Roles = "Patient")]
        [HttpPost("{id:int}/restore")]
        public async Task<ActionResult<MedicalDocument>> RestoreDocument(int id)
        {
            var currentUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Unauthorized();
            }

            var document = await _db.MedicalDocuments
                .FirstOrDefaultAsync(document =>
                    document.Id == id &&
                    document.OwnerUserId == currentUserId &&
                    document.IsDeleted
                );

            if (document is null)
            {
                return NotFound("Deleted document not found.");
            }

            document.IsDeleted = false;
            document.DeletedAt = null;

            await _db.SaveChangesAsync();

            return Ok(document);
        }

        // DELETE: /api/documents/1/permanent
        // Removes only the owner's database row and stored file.
        [Authorize(Roles = "Patient")]
        [HttpDelete("{id:int}/permanent")]
        public async Task<IActionResult> PermanentlyDeleteDocument(int id)
        {
            var currentUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Unauthorized();
            }

            var document = await _db.MedicalDocuments
                .FirstOrDefaultAsync(document =>
                    document.Id == id &&
                    document.OwnerUserId == currentUserId &&
                    document.IsDeleted
                );

            if (document is null)
            {
                return NotFound("Deleted document not found.");
            }

            var filePath = Path.Combine(
                _environment.ContentRootPath,
                "Uploads",
                document.StoredFileName
            );

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _db.MedicalDocuments.Remove(document);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
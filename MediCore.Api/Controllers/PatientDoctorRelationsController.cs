using System.Security.Claims;
using MediCore.Api.Contracts;
using MediCore.Api.Data;
using MediCore.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenAI.Realtime;

namespace MediCore.Api.Controllers
{
    [ApiController]
    [Route("api/patient-doctor-relations")]
    public class PatientDoctorRelationsController : ControllerBase
    {
        private const string PendingStatus = "Pending";
        private const string ActiveStatus = "Active";
        private const string RejectedStatus = "Rejected";
        private const string EndedStatus = "Ended";

        private readonly MediCoreDbContext _db;

        public PatientDoctorRelationsController(MediCoreDbContext db)
        {
            _db = db;
        }

        [Authorize(Roles = "Patient")]
        [HttpPost("request")]
        public async Task<ActionResult<RelationshipStatusResponse>> RequestDoctorConnection(
            RequestDoctorConnectionRequest request
        )
        {
            var patientUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(patientUserId))
            {
                return Unauthorized();
            }

            var doctorUserId = request.DoctorUserId?.Trim();

            if (string.IsNullOrWhiteSpace(doctorUserId))
            {
                return BadRequest(new
                {
                    message = "DoctorUserId is required."
                });
            }

            if (patientUserId == doctorUserId)
            {
                return BadRequest(new
                {
                    message = "You cannot send a connection request to yourself."
                });
            }

            var patientProfileExists = await _db.PatientProfiles
                .AsNoTracking()
                .AnyAsync(profile => profile.UserId == patientUserId);

            if (!patientProfileExists)
            {
                return BadRequest(new
                {
                    message = "A patient profile was not found for this account."
                });
            }

            var doctor = await _db.DoctorProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(profile =>
                    profile.UserId == doctorUserId &&
                    profile.IsApproved
                );

            if (doctor is null)
            {
                return NotFound(new
                {
                    message = "Approved doctor was not found."
                });
            }

            var existingRelation = await _db.PatientDoctorRelations
                .SingleOrDefaultAsync(relation =>
                    relation.PatientUserId == patientUserId &&
                    relation.DoctorUserId == doctorUserId
                );

            if (existingRelation is not null)
            {
                if (existingRelation.Status == PendingStatus)
                {
                    return Conflict(new
                    {
                        message = "A connection request is already pending for this doctor."
                    });
                }

                if (existingRelation.Status == ActiveStatus)
                {
                    return Conflict(new
                    {
                        message = "You already have an active connection with this doctor."
                    });
                }

                existingRelation.Status = PendingStatus;
                existingRelation.RequestedAt = DateTime.UtcNow;
                existingRelation.AcceptedAt = null;
                existingRelation.EndedAt = null;

                await _db.SaveChangesAsync();

                return Ok(ToStatusResponse(existingRelation));
            }

            var relation = new PatientDoctorRelation
            {
                PatientUserId = patientUserId,
                DoctorUserId = doctorUserId,
                Status = PendingStatus,
                RequestedAt = DateTime.UtcNow
            };

            _db.PatientDoctorRelations.Add(relation);

            await _db.SaveChangesAsync();

            return StatusCode(
                StatusCodes.Status201Created,
                ToStatusResponse(relation)
            );
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("doctor/pending")]
        public async Task<ActionResult<List<DoctorPendingRequestResponse>>> GetPendingRequests()
        {
            var doctorUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(doctorUserId))
            {
                return Unauthorized();
            }

            var approvedDoctorExists = await _db.DoctorProfiles
                .AsNoTracking()
                .AnyAsync(profile =>
                    profile.UserId == doctorUserId &&
                    profile.IsApproved
                );

            if (!approvedDoctorExists)
            {
                return Forbid();
            }

            var pendingRequests = await _db.PatientDoctorRelations
                .AsNoTracking()
                .Where(relation =>
                    relation.DoctorUserId == doctorUserId &&
                    relation.Status == PendingStatus
                )
                .OrderBy(relation => relation.RequestedAt)
                .Select(relation => new DoctorPendingRequestResponse
                {
                    Id = relation.Id,
                    PatientUserId = relation.PatientUserId,
                    PatientDisplayName = relation.Patient.DisplayName,
                    RequestedAt = relation.RequestedAt,
                    Status = relation.Status
                })
                .ToListAsync();

            return Ok(pendingRequests);
        }

        [Authorize(Roles = "Doctor")]
        [HttpPost("{relationId:int}/accept")]
        public async Task<ActionResult<RelationshipStatusResponse>> AcceptConnectionRequest(
            int relationId
        )
        {
            var doctorUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(doctorUserId))
            {
                return Unauthorized();
            }

            var approvedDoctorExists = await _db.DoctorProfiles
                .AsNoTracking()
                .AnyAsync(profile =>
                    profile.UserId == doctorUserId &&
                    profile.IsApproved
                );

            if (!approvedDoctorExists)
            {
                return Forbid();
            }

            var relation = await _db.PatientDoctorRelations
                .SingleOrDefaultAsync(relation =>
                    relation.Id == relationId &&
                    relation.DoctorUserId == doctorUserId
                );

            if (relation is null)
            {
                return NotFound(new
                {
                    message = "Connection request was not found."
                });
            }

            if (relation.Status != PendingStatus)
            {
                return Conflict(new
                {
                    message = "Only pending requests can be accepted."
                });
            }

            relation.Status = ActiveStatus;
            relation.AcceptedAt = DateTime.UtcNow;
            relation.EndedAt = null;

            await _db.SaveChangesAsync();

            return Ok(ToStatusResponse(relation));
        }

        [Authorize(Roles = "Doctor")]
        [HttpPost("{relationId:int}/reject")]
        public async Task<ActionResult<RelationshipStatusResponse>> RejectConnectionRequest(
            int relationId
        )
        {
            var doctorUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(doctorUserId))
            {
                return Unauthorized();
            }

            var approvedDoctorExists = await _db.DoctorProfiles
                .AsNoTracking()
                .AnyAsync(profile =>
                    profile.UserId == doctorUserId &&
                    profile.IsApproved
                );

            if (!approvedDoctorExists)
            {
                return Forbid();
            }

            var relation = await _db.PatientDoctorRelations
                .SingleOrDefaultAsync(relation =>
                    relation.Id == relationId &&
                    relation.DoctorUserId == doctorUserId
                );

            if (relation is null)
            {
                return NotFound(new
                {
                    message = "Connection request was not found."
                });
            }

            if (relation.Status != PendingStatus)
            {
                return Conflict(new
                {
                    message = "Only pending requests can be rejected."
                });
            }

            relation.Status = RejectedStatus;
            relation.AcceptedAt = null;
            relation.EndedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(ToStatusResponse(relation));
        }

        //my-doctor endpoint
        [Authorize(Roles = "Patient")]
        [HttpGet("patient/my-doctors")]
        public async Task<ActionResult<List<MyDoctorResponse>>> GetMyDoctors()
        {
            var patientUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(patientUserId))
            {
                return Unauthorized();
            }

            var patientProfileExists = await _db.PatientProfiles
                .AsNoTracking()
                .AnyAsync(profile => profile.UserId == patientUserId);

            if (!patientProfileExists)
            {
                return BadRequest(new
                {
                    message = "A patient profile was not found for this account."
                });
            }

            var doctors = await (
                from relation in _db.PatientDoctorRelations.AsNoTracking()
                join doctorProfile in _db.DoctorProfiles.AsNoTracking()
                    on relation.DoctorUserId equals doctorProfile.UserId
                join doctorUser in _db.Users.AsNoTracking()
                    on relation.DoctorUserId equals doctorUser.Id
                where relation.PatientUserId == patientUserId
                      && relation.Status == ActiveStatus
                      && doctorProfile.IsApproved
                orderby doctorUser.DisplayName
                select new MyDoctorResponse
                {
                    RelationId = relation.Id,
                    DoctorUserId = doctorUser.Id,
                    DisplayName = doctorUser.DisplayName,
                    Specialty = doctorProfile.Specialty,
                    AcceptedAt = relation.AcceptedAt
                }
            ).ToListAsync();

            return Ok(doctors);
        }

        //my-patients endpoint
        [Authorize(Roles = "Doctor")]
        [HttpGet("doctor/my-patients")]
        public async Task<ActionResult<List<MyPatientResponse>>> GetMyPatients()
        {
            var doctorUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(doctorUserId))
            {
                return Unauthorized();
            }

            var approvedDoctorExists = await _db.DoctorProfiles
                .AsNoTracking()
                .AnyAsync(profile =>
                    profile.UserId == doctorUserId &&
                    profile.IsApproved
                );

            if (!approvedDoctorExists)
            {
                return Forbid();
            }

            var patients = await (
                from relation in _db.PatientDoctorRelations.AsNoTracking()
                join patientUser in _db.Users.AsNoTracking()
                    on relation.PatientUserId equals patientUser.Id
                where relation.DoctorUserId == doctorUserId
                      && relation.Status == ActiveStatus
                orderby patientUser.DisplayName
                select new MyPatientResponse
                {
                    RelationId = relation.Id,
                    PatientUserId = patientUser.Id,
                    DisplayName = patientUser.DisplayName,
                    AcceptedAt = relation.AcceptedAt
                }
            ).ToListAsync();

            return Ok(patients);
        }

        //end of patient-doc rel
        [Authorize(Roles = "Patient,Doctor")]
        [HttpPost("{relationId:int}/end")]
        public async Task<ActionResult<RelationshipStatusResponse>> EndConnection(
    int relationId
)
        {
            var currentUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Unauthorized();
            }

            var relation = await _db.PatientDoctorRelations
                .SingleOrDefaultAsync(relation =>
                    relation.Id == relationId &&
                    (
                        relation.PatientUserId == currentUserId ||
                        relation.DoctorUserId == currentUserId
                    )
                );

            if (relation is null)
            {
                return NotFound(new
                {
                    message = "Active connection was not found."
                });
            }

            if (relation.Status != ActiveStatus)
            {
                return Conflict(new
                {
                    message = "Only active connections can be ended."
                });
            }

            relation.Status = EndedStatus;
            relation.EndedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(ToStatusResponse(relation));
        }


        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        private static RelationshipStatusResponse ToStatusResponse(
            PatientDoctorRelation relation
        )
        {
            return new RelationshipStatusResponse
            {
                Id = relation.Id,
                Status = relation.Status,
                RequestedAt = relation.RequestedAt,
                AcceptedAt = relation.AcceptedAt,
                EndedAt = relation.EndedAt
            };
        }
    }
}
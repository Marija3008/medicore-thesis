using System.Security.Claims;
using MediCore.Api.Contracts;
using MediCore.Api.Models;
using MediCore.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCore.Api.Controllers
{
    [ApiController]
    [Route("api/medications")]
    [Authorize]
    public class MedicationsController : ControllerBase
    {
        private readonly IMedicationService _medicationService;
        private readonly IPatientAccessService _patientAccessService;

        public MedicationsController(
            IMedicationService medicationService,
            IPatientAccessService patientAccessService)
        {
            _medicationService = medicationService;
            _patientAccessService = patientAccessService;
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [Authorize(Roles = "Patient")]
        [HttpGet]
        public async Task<IActionResult> GetMyMedications()
        {
            var patientUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(patientUserId))
            {
                return Unauthorized();
            }

            var medications =
                await _medicationService.GetPatientMedicationsAsync(patientUserId);

            var response = medications
                .Select(ToResponse)
                .ToList();

            return Ok(response);
        }

        [Authorize(Roles = "Patient")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetMyMedication(int id)
        {
            var patientUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(patientUserId))
            {
                return Unauthorized();
            }

            var medication =
                await _medicationService.GetPatientMedicationAsync(
                    patientUserId,
                    id);

            if (medication is null)
            {
                return NotFound("Medication not found.");
            }

            return Ok(ToResponse(medication));
        }

        [Authorize(Roles = "Patient")]
        [HttpPost]
        public async Task<IActionResult> CreateMedication(
    CreateMedicationRequest request)
        {
            var patientUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(patientUserId))
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Dose))
            {
                return BadRequest("Medication name and dose are required.");
            }

            if (request.EndDate.HasValue &&
                request.EndDate.Value < request.StartDate)
            {
                return BadRequest(
                    "End date cannot be earlier than start date.");
            }

            var hasDuplicateScheduleTimes = request.Schedules
    .GroupBy(schedule => schedule.TimeOfDay)
    .Any(group => group.Count() > 1);

            if (hasDuplicateScheduleTimes)
            {
                return BadRequest(
                    "A medication cannot contain duplicate schedule times.");
            }

            var medication = new Medication
            {
                Name = request.Name.Trim(),
                Dose = request.Dose.Trim(),
                Unit = request.Unit?.Trim(),
                Instructions = request.Instructions?.Trim(),
                StartDate = request.StartDate,
                EndDate = request.EndDate,

                Schedules = request.Schedules
                    .Select(schedule => new MedicationSchedule
                    {
                        TimeOfDay = schedule.TimeOfDay,
                        ReminderEnabled = schedule.ReminderEnabled
                    })
                    .ToList()
            };

            var createdMedication =
                await _medicationService.CreateMedicationAsync(
                    patientUserId,
                    medication);

            return CreatedAtAction(
                nameof(GetMyMedication),
                new { id = createdMedication.Id },
                ToResponse(createdMedication));
        }

        [Authorize(Roles = "Patient")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateMedication(
    int id,
    UpdateMedicationRequest request)
        {
            var patientUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(patientUserId))
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Dose))
            {
                return BadRequest("Medication name and dose are required.");
            }

            if (request.EndDate.HasValue &&
                request.EndDate.Value < request.StartDate)
            {
                return BadRequest(
                    "End date cannot be earlier than start date.");
            }

            var hasDuplicateScheduleTimes = request.Schedules
                .GroupBy(schedule => schedule.TimeOfDay)
                .Any(group => group.Count() > 1);

            if (hasDuplicateScheduleTimes)
            {
                return BadRequest(
                    "A medication cannot contain duplicate schedule times.");
            }

            var updatedMedication = new Medication
            {
                Name = request.Name.Trim(),
                Dose = request.Dose.Trim(),
                Unit = request.Unit?.Trim(),
                Instructions = request.Instructions?.Trim(),
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsActive = request.IsActive,

                Schedules = request.Schedules
                    .Select(schedule => new MedicationSchedule
                    {
                        TimeOfDay = schedule.TimeOfDay,
                        ReminderEnabled = schedule.ReminderEnabled
                    })
                    .ToList()
            };

            var medication =
                await _medicationService.UpdateMedicationAsync(
                    patientUserId,
                    id,
                    updatedMedication);

            if (medication is null)
            {
                return NotFound("Medication not found.");
            }

            return Ok(ToResponse(medication));
        }

        [Authorize(Roles = "Patient")]
        [HttpPatch("{id:int}/deactivate")]
        public async Task<IActionResult> DeactivateMedication(int id)
        {
            var patientUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(patientUserId))
            {
                return Unauthorized();
            }

            var medication =
                await _medicationService.DeactivateMedicationAsync(
                    patientUserId,
                    id);

            if (medication is null)
            {
                return NotFound("Medication not found.");
            }

            return Ok(ToResponse(medication));
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("patient/{patientUserId}")]
        public async Task<IActionResult> GetPatientMedications(
    string patientUserId)
        {
            var doctorUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(doctorUserId))
            {
                return Unauthorized();
            }

            var canAccess =
                await _patientAccessService.CanDoctorAccessPatientAsync(
                    doctorUserId,
                    patientUserId);

            if (!canAccess)
            {
                return Forbid();
            }

            var medications =
                await _medicationService.GetPatientMedicationsAsync(
                    patientUserId);

            var response = medications
                .Select(ToResponse)
                .ToList();

            return Ok(response);
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("patient/{patientUserId}/{id:int}")]
        public async Task<IActionResult> GetPatientMedication(
    string patientUserId,
    int id)
        {
            var doctorUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(doctorUserId))
            {
                return Unauthorized();
            }

            var canAccess =
                await _patientAccessService.CanDoctorAccessPatientAsync(
                    doctorUserId,
                    patientUserId);

            if (!canAccess)
            {
                return Forbid();
            }

            var medication =
                await _medicationService.GetPatientMedicationAsync(
                    patientUserId,
                    id);

            if (medication is null)
            {
                return NotFound("Medication not found.");
            }

            return Ok(ToResponse(medication));
        }

        [HttpPatch("{id:int}/reactivate")]
        public async Task<ActionResult<MedicationResponse>> ReactivateMedication(
    int id)
        {
            var patientUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(patientUserId))
            {
                return Unauthorized();
            }

            var medication =
                await _medicationService.ReactivateMedicationAsync(
                    id,
                    patientUserId);

            if (medication == null)
            {
                return NotFound();
            }

            return Ok(ToResponse(medication));
        }

        private static MedicationResponse ToResponse(Medication medication)
        {
            return new MedicationResponse
            {
                Id = medication.Id,
                Name = medication.Name,
                Dose = medication.Dose,
                Unit = medication.Unit,
                Instructions = medication.Instructions,
                StartDate = medication.StartDate,
                EndDate = medication.EndDate,
                IsActive = medication.IsActive,
                CreatedAt = medication.CreatedAt,

                Schedules = medication.Schedules
                    .OrderBy(schedule => schedule.TimeOfDay)
                    .Select(schedule => new MedicationScheduleResponse
                    {
                        Id = schedule.Id,
                        TimeOfDay = schedule.TimeOfDay,
                        ReminderEnabled = schedule.ReminderEnabled
                    })
                    .ToList()
            };
        }
    }
}
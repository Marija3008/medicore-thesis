using MediCore.Api.Contracts;
using MediCore.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Controllers
{
    [ApiController]
    [Route("api/doctors")]
    public class DoctorsController : ControllerBase
    {
        private readonly MediCoreDbContext _db;

        public DoctorsController(MediCoreDbContext db)
        {
            _db = db;
        }

        [Authorize(Roles = "Patient")]
        [HttpGet]
        public async Task<ActionResult<List<DoctorDirectoryItemResponse>>> GetApprovedDoctors()
        {
            var doctors = await _db.DoctorProfiles
                .AsNoTracking()
                .Where(profile => profile.IsApproved)
                .OrderBy(profile => profile.User.DisplayName)
                .Select(profile => new DoctorDirectoryItemResponse
                {
                    UserId = profile.UserId,
                    DisplayName = profile.User.DisplayName,
                    Specialty = profile.Specialty
                })
                .ToListAsync();

            return Ok(doctors);
        }
    }
}
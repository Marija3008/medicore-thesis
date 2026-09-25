using System.Security.Claims;
using MediCore.Api.Data;
using MediCore.Api.Contracts;
using MediCore.Api.Models;
using MediCore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MediCore.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtTokenService _jwtTokenService;
        private readonly MediCoreDbContext _db;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            JwtTokenService jwtTokenService,
            MediCoreDbContext db)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _db = db; //this gives AuthController access to the profile tables
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(
            [FromBody] RegisterRequest request)
        {
            var displayName = request.DisplayName.Trim();
            var email = request.Email.Trim().ToLowerInvariant();

            var role = request.Role.Trim().ToLowerInvariant() switch
            {
                "patient" => "Patient",
                "doctor" => "Doctor",
                _ => string.Empty
            };

            if (string.IsNullOrWhiteSpace(displayName))
            {
                return BadRequest("Display name is required.");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest("Email is required.");
            }

            if (string.IsNullOrWhiteSpace(role))
            {
                return BadRequest(
                    "Role must be either Patient or Doctor."
                );
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                DisplayName = displayName,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(
                user,
                request.Password
            );

            if (!createResult.Succeeded)
            {
                return BadRequest(new
                {
                    errors = createResult.Errors
                        .Select(error => error.Description)
                });
            }

            var roleResult = await _userManager.AddToRoleAsync(user, role);

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                return BadRequest(new
                {
                    errors = roleResult.Errors
                        .Select(error => error.Description)
                });
            }

            if (role == "Patient")
            {
                var patientProfile = new PatientProfile
                {
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };

                _db.PatientProfiles.Add(patientProfile);
            }
            else
            {
                var doctorProfile = new DoctorProfile
                {
                    UserId = user.Id,
                    Specialty = string.IsNullOrWhiteSpace(request.Specialty)
                        ? null
                        : request.Specialty.Trim(),
                    IsApproved = false,
                    CreatedAt = DateTime.UtcNow
                };

                _db.DoctorProfiles.Add(doctorProfile);
            }

            await _db.SaveChangesAsync();

            var response = await _jwtTokenService
                .CreateAuthResponseAsync(user);

            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(
            [FromBody] LoginRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            var passwordIsCorrect = await _userManager.CheckPasswordAsync(
                user,
                request.Password
            );

            if (!passwordIsCorrect)
            {
                return Unauthorized("Invalid email or password.");
            }

            var response = await _jwtTokenService
                .CreateAuthResponseAsync(user);

            return Ok(response);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<AuthenticatedUserResponse>> Me()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return Unauthorized();
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new AuthenticatedUserResponse
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToList()
            });
        }
    }
}
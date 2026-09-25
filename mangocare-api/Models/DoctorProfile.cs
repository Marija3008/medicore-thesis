namespace MediCore.Api.Models
{
    public class DoctorProfile
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;

        public string? Specialty { get; set; }

        public bool IsApproved { get; set; } = false; //later feature: doctor can register, but should not automatically gain access to patients until approved

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
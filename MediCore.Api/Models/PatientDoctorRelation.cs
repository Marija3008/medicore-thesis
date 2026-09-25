namespace MediCore.Api.Models
{
    public class PatientDoctorRelation
    {
        public int Id { get; set; }

        public string PatientUserId { get; set; } = string.Empty;

        public ApplicationUser Patient { get; set; } = null!;

        public string DoctorUserId { get; set; } = string.Empty;

        public ApplicationUser Doctor { get; set; } = null!;

        public string Status { get; set; } = "Pending";

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public DateTime? AcceptedAt { get; set; }

        public DateTime? EndedAt { get; set; }
    }
}
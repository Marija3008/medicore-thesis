namespace MediCore.Api.Models
{
    public class Medication
    {
        public int Id { get; set; }

        public string PatientUserId { get; set; } = string.Empty;
        public ApplicationUser Patient { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public string Dose { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public string? Instructions { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<MedicationSchedule> Schedules { get; set; }
            = new List<MedicationSchedule>();
    }
}
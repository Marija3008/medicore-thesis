namespace MediCore.Api.Contracts
{
    public class CreateMedicationRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Dose { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public string? Instructions { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public List<CreateMedicationScheduleRequest> Schedules { get; set; }
            = new();
    }

    public class UpdateMedicationRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Dose { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public string? Instructions { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; }

        public List<CreateMedicationScheduleRequest> Schedules { get; set; }
            = new();
    }
    public class CreateMedicationScheduleRequest
    {
        public TimeSpan TimeOfDay { get; set; }
        public bool ReminderEnabled { get; set; } = true;
    }

    public class MedicationResponse
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Dose { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public string? Instructions { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<MedicationScheduleResponse> Schedules { get; set; }
            = new();
    }

    public class MedicationScheduleResponse
    {
        public int Id { get; set; }
        public TimeSpan TimeOfDay { get; set; }
        public bool ReminderEnabled { get; set; }
    }
}
namespace MediCore.Api.Models
{
    public class MedicationSchedule
    {
        public int Id { get; set; }

        public int MedicationId { get; set; }
        public Medication Medication { get; set; } = null!;

        public TimeSpan TimeOfDay { get; set; }

        public bool ReminderEnabled { get; set; } = true;
    }
}
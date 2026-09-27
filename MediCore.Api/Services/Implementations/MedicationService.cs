using MediCore.Api.Data;
using MediCore.Api.Models;
using MediCore.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Services.Implementations
{
    public class MedicationService : IMedicationService
    {
        private readonly MediCoreDbContext _db;

        public MedicationService(MediCoreDbContext db)
        {
            _db = db;
        }

        public async Task<List<Medication>> GetPatientMedicationsAsync(
            string patientUserId)
        {
            return await _db.Medications
                .AsNoTracking()
                .Include(medication => medication.Schedules)
                .Where(medication =>
                    medication.PatientUserId == patientUserId)
                .OrderByDescending(medication => medication.CreatedAt)
                .ToListAsync();
        }

        public async Task<Medication?> GetPatientMedicationAsync(
            string patientUserId,
            int medicationId)
        {
            return await _db.Medications
                .AsNoTracking()
                .Include(medication => medication.Schedules)
                .SingleOrDefaultAsync(medication =>
                    medication.Id == medicationId &&
                    medication.PatientUserId == patientUserId);
        }

        public async Task<Medication> CreateMedicationAsync(
    string patientUserId,
    Medication medication)
        {
            medication.PatientUserId = patientUserId;
            medication.CreatedAt = DateTime.UtcNow;
            medication.IsActive = true;

            _db.Medications.Add(medication);
            await _db.SaveChangesAsync();

            return medication;
        }

        public async Task<Medication?> UpdateMedicationAsync(
    string patientUserId,
    int medicationId,
    Medication updatedMedication)
        {
            var medication = await _db.Medications
                .Include(medication => medication.Schedules)
                .SingleOrDefaultAsync(medication =>
                    medication.Id == medicationId &&
                    medication.PatientUserId == patientUserId);

            if (medication is null)
            {
                return null;
            }

            medication.Name = updatedMedication.Name;
            medication.Dose = updatedMedication.Dose;
            medication.Unit = updatedMedication.Unit;
            medication.Instructions = updatedMedication.Instructions;
            medication.StartDate = updatedMedication.StartDate;
            medication.EndDate = updatedMedication.EndDate;
            medication.IsActive = updatedMedication.IsActive;

            _db.MedicationSchedules.RemoveRange(medication.Schedules);

            medication.Schedules = updatedMedication.Schedules
                .Select(schedule => new MedicationSchedule
                {
                    TimeOfDay = schedule.TimeOfDay,
                    ReminderEnabled = schedule.ReminderEnabled
                })
                .ToList();

            await _db.SaveChangesAsync();

            return medication;
        }

        public async Task<Medication?> DeactivateMedicationAsync(
    string patientUserId,
    int medicationId)
        {
            var medication = await _db.Medications
                .Include(medication => medication.Schedules)
                .SingleOrDefaultAsync(medication =>
                    medication.Id == medicationId &&
                    medication.PatientUserId == patientUserId);

            if (medication is null)
            {
                return null;
            }

            medication.IsActive = false;

            foreach (var schedule in medication.Schedules)
            {
                schedule.ReminderEnabled = false;
            }

            await _db.SaveChangesAsync();

            return medication;
        }

        public async Task<Medication?> ReactivateMedicationAsync(
    int medicationId,
    string patientUserId)
        {
            var medication = await _db.Medications
                .Include(m => m.Schedules)
                .FirstOrDefaultAsync(m =>
                    m.Id == medicationId &&
                    m.PatientUserId == patientUserId);

            if (medication == null)
            {
                return null;
            }

            medication.IsActive = true;

            await _db.SaveChangesAsync();

            return medication;
        }
    }
}
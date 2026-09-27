using MediCore.Api.Models;

namespace MediCore.Api.Services.Interfaces
{
    public interface IMedicationService
    {
        Task<List<Medication>> GetPatientMedicationsAsync(
            string patientUserId);

        Task<Medication?> GetPatientMedicationAsync(
            string patientUserId,
            int medicationId);

        Task<Medication> CreateMedicationAsync(
            string patientUserId,
            Medication medication);

        Task<Medication?> UpdateMedicationAsync(
    string patientUserId,
    int medicationId,
    Medication updatedMedication);

        Task<Medication?> DeactivateMedicationAsync(
    string patientUserId,
    int medicationId);
    }

}
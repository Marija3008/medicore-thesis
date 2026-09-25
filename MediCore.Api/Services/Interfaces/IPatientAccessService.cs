namespace MediCore.Api.Services.Interfaces
{
    public interface IPatientAccessService
    {
        Task<bool> CanDoctorAccessPatientAsync(
           string doctorUserId,
           string patientUserId);
    }
}

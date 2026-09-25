using MediCore.Api.Data;
using MediCore.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediCore.Api.Services.Implementations
{
    public class PatientAccessService : IPatientAccessService
    {
        private readonly MediCoreDbContext _db;

        public PatientAccessService(MediCoreDbContext db)
        {
            _db = db;
        }

        public async Task<bool> CanDoctorAccessPatientAsync(
            string doctorUserId,
            string patientUserId)
        {
            var isApprovedDoctor = await _db.DoctorProfiles
                .AsNoTracking()
                .AnyAsync(profile =>
                    profile.UserId == doctorUserId &&
                    profile.IsApproved);

            if (!isApprovedDoctor)
            {
                return false;
            }

            return await _db.PatientDoctorRelations
                .AsNoTracking()
                .AnyAsync(relation =>
                    relation.DoctorUserId == doctorUserId &&
                    relation.PatientUserId == patientUserId &&
                    relation.Status == "Active");
        }
    }
}
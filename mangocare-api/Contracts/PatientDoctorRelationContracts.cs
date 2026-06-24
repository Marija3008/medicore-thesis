namespace mangocare_api.Contracts
{
    public class DoctorDirectoryItemResponse
    {
        public string UserId { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? Specialty { get; set; }
    }

    public class RequestDoctorConnectionRequest
    {
        public string DoctorUserId { get; set; } = string.Empty;
    }

    public class RelationshipStatusResponse
    {
        public int Id { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime RequestedAt { get; set; }

        public DateTime? AcceptedAt { get; set; }

        public DateTime? EndedAt { get; set; }
    }

    public class DoctorPendingRequestResponse
    {
        public int Id { get; set; }

        public string PatientUserId { get; set; } = string.Empty;

        public string PatientDisplayName { get; set; } = string.Empty;

        public DateTime RequestedAt { get; set; }

        public string Status { get; set; } = string.Empty;
    }

    public class MyDoctorResponse
    {
        public int RelationId { get; set; }

        public string DoctorUserId { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? Specialty { get; set; }

        public DateTime? AcceptedAt { get; set; }
    }

    public class MyPatientResponse
    {
        public int RelationId { get; set; }

        public string PatientUserId { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public DateTime? AcceptedAt { get; set; }
    }
}
namespace MediCore.Api.Models
{
    public class MedicalDocument
    {
        public int Id { get; set; }
        public string OwnerUserId { get; set; } = string.Empty;

        public ApplicationUser OwnerUser { get; set; } = null!;

        public string Title { get; set; } = string.Empty;

        public string OriginalFileName { get; set; } = string.Empty;

        public string StoredFileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public long SizeBytes { get; set; }

        public string Type { get; set; } = "other";

        public string Status { get; set; } = "pending_review";

        public string Summary { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

    }
}

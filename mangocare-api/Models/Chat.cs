namespace MediCore.Api.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public string OwnerUserId { get; set; } = string.Empty;

        public ApplicationUser OwnerUser { get; set; } = null!;

        public string Title { get; set; } = "New conversation";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public List<ChatMessage> Messages { get; set; } = new();
    }
}
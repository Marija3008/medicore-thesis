namespace mangocare_api.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        public int ChatId { get; set; }

        public Chat Chat { get; set; } = null!;

        public string Role { get; set; } = string.Empty;
        // "user" or "assistant"

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
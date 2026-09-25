namespace MediCore.Api.Contracts
{
    public class CreateChatRequest
    {
        public string? Title { get; set; }
    }

    public class SendChatMessageRequest
    {
        public string Content { get; set; } = string.Empty;
    }

    public class ChatListItemResponse
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public int MessageCount { get; set; }
    }

    public class ChatMessageResponse
    {
        public int Id { get; set; }

        public int ChatId { get; set; }

        public string Role { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }

    public class SendChatMessageResponse
    {
        public ChatMessageResponse UserMessage { get; set; } = new();

        public ChatMessageResponse AssistantMessage { get; set; } = new();
    }
}
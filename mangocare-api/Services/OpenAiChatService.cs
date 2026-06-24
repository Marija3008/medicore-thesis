using OpenAI.Chat;
using DatabaseChatMessage = mangocare_api.Models.ChatMessage;

namespace mangocare_api.Services
{
    public class OpenAiChatService
    {
        private readonly ChatClient _chatClient;

        public OpenAiChatService(IConfiguration configuration)
        {
            var apiKey = configuration["OpenAI:ApiKey"];
            var model = configuration["OpenAI:Model"] ?? "gpt-5.1";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "OpenAI API key is missing. Check User Secrets."
                );
            }

            _chatClient = new ChatClient(
                model: model,
                apiKey: apiKey
            );
        }

        public async Task<string> GenerateReplyAsync(
            IEnumerable<DatabaseChatMessage> messages)
        {
            var history = messages
                .OrderBy(message => message.CreatedAt)
                .Select(message =>
                {
                    var label = message.Role == "assistant"
                        ? "Assistant"
                        : "Patient";

                    return $"{label}: {message.Content}";
                });

            var prompt = $"""
You are MangoCare, a supportive health-information assistant.

Rules:
- Explain health topics in plain, calm, simple language.
- Give general educational information only.
- Do not diagnose conditions.
- Do not prescribe medication or treatment.
- Do not claim certainty about a patient's health.
- Encourage the user to contact a healthcare professional for medical decisions.
- For urgent or emergency symptoms, recommend urgent local medical care.
- Keep answers reasonably concise.

Conversation history:
{string.Join(Environment.NewLine, history)}

Reply to the most recent patient message.
""";

            ChatCompletion completion =
                await _chatClient.CompleteChatAsync(prompt);

            var reply = string.Concat(
                completion.Content.Select(part => part.Text)
            ).Trim();

            if (string.IsNullOrWhiteSpace(reply))
            {
                throw new InvalidOperationException(
                    "OpenAI returned an empty response."
                );
            }

            return reply;
        }
    }
}
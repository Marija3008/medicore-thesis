using System.Security.Claims;
using mangocare_api.Contracts;
using mangocare_api.Data;
using mangocare_api.Models;
using mangocare_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace mangocare_api.Controllers
{
    [ApiController]
    [Route("api/chats")]
    [Authorize]
    public class ChatsController : ControllerBase
    {
        private readonly MangoCareDbContext _db;
        private readonly OpenAiChatService _openAiChatService;
        private readonly ILogger<ChatsController> _logger;

        public ChatsController(
            MangoCareDbContext db,
            OpenAiChatService openAiChatService,
            ILogger<ChatsController> logger)
        {
            _db = db;
            _openAiChatService = openAiChatService;
            _logger = logger;
        }

        // GET: /api/chats
        // Returns only chats owned by the logged-in user.
        [HttpGet]
        public async Task<ActionResult<List<ChatListItemResponse>>> GetChats()
        {
            var currentUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Unauthorized();
            }

            var chats = await _db.Chats
                .AsNoTracking()
                .Where(chat => chat.OwnerUserId == currentUserId)
                .OrderByDescending(chat => chat.UpdatedAt)
                .Select(chat => new ChatListItemResponse
                {
                    Id = chat.Id,
                    Title = chat.Title,
                    CreatedAt = chat.CreatedAt,
                    UpdatedAt = chat.UpdatedAt,
                    MessageCount = chat.Messages.Count
                })
                .ToListAsync();

            return Ok(chats);
        }

        // POST: /api/chats
        // Creates a chat owned by the logged-in user.
        [HttpPost]
        public async Task<ActionResult<ChatListItemResponse>> CreateChat(
            [FromBody] CreateChatRequest request)
        {
            var currentUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Unauthorized();
            }

            var title = string.IsNullOrWhiteSpace(request.Title)
                ? "New conversation"
                : request.Title.Trim();

            var chat = new Chat
            {
                OwnerUserId = currentUserId,
                Title = title,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Chats.Add(chat);
            await _db.SaveChangesAsync();

            var response = new ChatListItemResponse
            {
                Id = chat.Id,
                Title = chat.Title,
                CreatedAt = chat.CreatedAt,
                UpdatedAt = chat.UpdatedAt,
                MessageCount = 0
            };

            return CreatedAtAction(
                nameof(GetChatMessages),
                new { id = chat.Id },
                response
            );
        }

        // GET: /api/chats/1/messages
        // Returns messages only when the chat belongs to the logged-in user.
        [HttpGet("{id:int}/messages")]
        public async Task<ActionResult<List<ChatMessageResponse>>> GetChatMessages(
            int id)
        {
            var currentUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Unauthorized();
            }

            var chatExists = await _db.Chats
                .AsNoTracking()
                .AnyAsync(chat =>
                    chat.Id == id &&
                    chat.OwnerUserId == currentUserId
                );

            if (!chatExists)
            {
                return NotFound("Chat not found.");
            }

            var messages = await _db.ChatMessages
                .AsNoTracking()
                .Where(message => message.ChatId == id)
                .OrderBy(message => message.CreatedAt)
                .Select(message => new ChatMessageResponse
                {
                    Id = message.Id,
                    ChatId = message.ChatId,
                    Role = message.Role,
                    Content = message.Content,
                    CreatedAt = message.CreatedAt
                })
                .ToListAsync();

            return Ok(messages);
        }

        // POST: /api/chats/1/messages
        // Sends a message only inside a chat owned by the logged-in user.
        [HttpPost("{id:int}/messages")]
        public async Task<ActionResult<SendChatMessageResponse>> SendMessage(
            int id,
            [FromBody] SendChatMessageRequest request)
        {
            var currentUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Unauthorized();
            }

            var content = request.Content?.Trim();

            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("Message content cannot be empty.");
            }

            var chat = await _db.Chats
                .FirstOrDefaultAsync(chat =>
                    chat.Id == id &&
                    chat.OwnerUserId == currentUserId
                );

            if (chat is null)
            {
                return NotFound("Chat not found.");
            }

            var userMessage = new ChatMessage
            {
                ChatId = chat.Id,
                Role = "user",
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            chat.UpdatedAt = DateTime.UtcNow;

            _db.ChatMessages.Add(userMessage);
            await _db.SaveChangesAsync();

            var conversationHistory = await _db.ChatMessages
                .Where(message => message.ChatId == chat.Id)
                .OrderBy(message => message.CreatedAt)
                .ToListAsync();

            string assistantText;

            try
            {
                assistantText = await _openAiChatService
                    .GenerateReplyAsync(conversationHistory);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "OpenAI reply failed for chat {ChatId}",
                    chat.Id
                );

                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    new
                    {
                        message = "OpenAI reply could not be generated."
                    }
                );
            }

            var assistantMessage = new ChatMessage
            {
                ChatId = chat.Id,
                Role = "assistant",
                Content = assistantText,
                CreatedAt = DateTime.UtcNow
            };

            chat.UpdatedAt = DateTime.UtcNow;

            _db.ChatMessages.Add(assistantMessage);
            await _db.SaveChangesAsync();

            return Ok(new SendChatMessageResponse
            {
                UserMessage = MapMessage(userMessage),
                AssistantMessage = MapMessage(assistantMessage)
            });
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        private static ChatMessageResponse MapMessage(ChatMessage message)
        {
            return new ChatMessageResponse
            {
                Id = message.Id,
                ChatId = message.ChatId,
                Role = message.Role,
                Content = message.Content,
                CreatedAt = message.CreatedAt
            };
        }
    }
}
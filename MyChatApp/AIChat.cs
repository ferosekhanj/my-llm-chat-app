using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Client;
using OllamaSharp.Models;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Diagnostics;
#pragma warning disable SKEXP0001

namespace MyChatApp
{
    public class AIChat
    {
        public static int RunningCount = 1;

        public readonly ILogger<AIChat> _logger;

        public string ActiveModel { get; set; }
        public string ActiveChatKey { get; private set; } = $"New Chat {RunningCount++}";
        public ChatHistory ActiveChat { get; private set; } = new ChatHistory();

        private BindingList<ChatDetails> _chatHistories = new();
        public BindingList<ChatDetails> ChatHistories 
        { 
            get => _chatHistories; 
        }

        private BindingList<string> Models { get; } = new();

        private AIChatProviders _aiChatProviders;

        public AIChat(AIChatProviders aIChatProviders)
        {
            _aiChatProviders = aIChatProviders; 

            // Add enterprise components
            _aiChatProviders.GetServices().AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Critical));

            // Fetch the logger
            _logger = _aiChatProviders.GetServices().BuildServiceProvider().GetRequiredService<ILogger<AIChat>>();

            _chatHistories.Add(new ChatDetails { Name = ActiveChatKey, ChatHistory = ActiveChat });

        }

        public void CreateNewChat()
        {
            var activeChatKey = ActiveChatKey;

            // Create a new chat history
            _chatHistories.Insert(0,new ChatDetails { Name = $"New Chat {RunningCount++}", ChatHistory = new ChatHistory() });

            // Log the creation of a new chat
            _logger.LogInformation("New chat created with ID");
        }

        public void SelectChat(ChatDetails details)
        {
            if (details == null || details.Name == ActiveChatKey)
            {
                return;
            }

            // Create a new chat history
            ActiveChatKey = details.Name;
            ActiveChat = details.ChatHistory;

            // Notify subscribers that the chat history has been updated
            OnActiveChatChanged(ActiveChat);
        }

        public async IAsyncEnumerable<String> GetResponseAsync(string userMessage, bool enableStreaming = true, string fileAttachment = null, string modelId = "siemens", bool useTools = false)
        {
            OnStatusChanged("Working...");
            // Log the user message
            _logger.LogInformation("{UserMessage}", userMessage);

            // Add the file attachment
            if(fileAttachment != null)
            {
                var fileContent = await File.ReadAllTextAsync(fileAttachment);
                ActiveChat.Add(new ChatMessageContent()
                {
                    Role = AuthorRole.User,
                    Items = [
                        new TextContent(userMessage),
                        GetFileAttachmentContent(fileAttachment)
                       ]
                });
            }
            else
            {
                // Add the user message to the active chat history
                ActiveChat.AddUserMessage(userMessage);
            }
            var (_kernel, _chatCompletionService, _promptExecutionSettings) = _aiChatProviders.GetKernelAndSettings(modelId, useTools);
            foreach (var plugins in _kernel.Plugins)
            {
                _logger.LogInformation("Plugin: {Name}", plugins.Name);
            }

            var fullResponse = "";
            if (!enableStreaming)
            {
                // Get the response from the AI model
                var response = await _chatCompletionService.GetChatMessageContentsAsync(
                    ActiveChat,
                    _promptExecutionSettings,
                    _kernel);

                foreach (var chunk in response)
                {
                    fullResponse += chunk;
                    yield return chunk.Content;
                }
            }
            else
            {
                // Get the response from the AI model
                var response = _chatCompletionService.GetStreamingChatMessageContentsAsync(
                    ActiveChat,
                    _promptExecutionSettings,
                    _kernel);

                await foreach (var chunk in response)
                {
                    fullResponse += chunk;
                    yield return chunk.Content;
                }
            }
            yield return "\n";
            OnStatusChanged("Done.");

            // Add the AI response to the active chat history
            ActiveChat.AddAssistantMessage(fullResponse);

        }

        private KernelContent GetFileAttachmentContent(string fileAttachment)
        {
            if (string.IsNullOrEmpty(fileAttachment) || !File.Exists(fileAttachment))
            {
                return null;
            }

            switch(Path.GetExtension(fileAttachment).ToLower())
            {
                case ".txt":
                    var fileContent = File.ReadAllText(fileAttachment);
                    // Create a TextContent object
                    return new TextContent(fileContent);
                // Add more cases for other file types if needed
                case ".jpg":
                case ".jpeg":
                    var fileBytes1 = File.ReadAllBytes(fileAttachment);
                    // Create an ImageContent object
                    return new ImageContent(fileBytes1, "image/jpeg");
                case ".png":
                    var fileBytes2 = File.ReadAllBytes(fileAttachment);
                    // Create an ImageContent object
                    return new ImageContent(fileBytes2, "image/png");
                default:
                    throw new NotSupportedException($"File type not supported: {fileAttachment}");
            }

            // Read the file content
        }

        public async Task<String> UpdateTitleForChat(ChatDetails chatDetails)
        {
            var chat = new ChatHistory();
            foreach(var message in chatDetails.ChatHistory)
            {
                // Add the messages to the new chat history
                chat.Add(message);
            }

            if (chat.Count < 2)
            {
                return "No messages";
            }

            // Log the user message
            _logger.LogInformation("Get Title");

            // Add the user message to the active chat history
            chat.AddUserMessage("You are an AI assistant that summarizes conversations into short, clear, and engaging titles (3–6 words). Based on this chat content, generate one concise title that best represents the main topic or purpose of the discussion. Avoid generic terms like 'Chat' or 'Conversation'. Make it descriptive and relevant.");

            if (chatDetails.IsTitleGenerated)
            {
                return "ALready done";
            }

            var (_kernel, _chatCompletionService, _promptExecutionSettings) = _aiChatProviders.GetKernelAndSettings(ActiveModel, false);

            // Get the response from the AI model
            var response = await _chatCompletionService.GetChatMessageContentAsync(
                chat,
                _promptExecutionSettings,
                _kernel);
            chat.RemoveAt(chat.Count - 1); // Remove the last user message

            var str = response.Content.Trim();
            // Replace everything except letters, numbers, and spaces with space
            string cleaned = Regex.Replace(str, @"[^a-zA-Z0-9\s]", " ");

            // Optionally, normalize multiple spaces into one
            cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

            // Add the AI response to the active chat history
            if (!chatDetails.IsTitleGenerated)
            {
                chatDetails.Name = cleaned;
                chatDetails.IsTitleGenerated = true;
                // Notify subscribers that the chat history has been updated
                OnChatTitleChanged();
            }

            return response.Content;
        }

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public void SaveChatHistories()
        {
            foreach(var  chat in _chatHistories)
            {
                if (chat.IsModified && chat.ChatHistory.Count>0)
                {
                    // Save the chat history to a file or database
                    // For example, you can serialize the chat history to JSON and save it to a file
                    var filePath = Path.Combine("ChatHistories", $"{chat.Name}.json");
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    File.WriteAllText(filePath, JsonSerializer.Serialize(chat.ChatHistory, JsonOptions));
                    chat.IsModified = false; // Reset the modified flag after saving
                }
            }
        }

        public void LoadChatHistories()
        {
            if(!Directory.Exists("ChatHistories"))
            {
                return;
            }
            var chatHistoryFiles = Directory.GetFiles("ChatHistories", "*.json");
            foreach (var file in chatHistoryFiles)
            {
                try
                {
                    var chatHistory = JsonSerializer.Deserialize<ChatHistory>(File.ReadAllText(file), JsonOptions);
                    if (chatHistory != null)
                    {
                        _chatHistories.Add(new ChatDetails { Name = Path.GetFileNameWithoutExtension(file), ChatHistory = chatHistory, IsModified = false, IsTitleGenerated = true });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load chat history from file: {File}", file);
                }
            }

        }

        public event EventHandler<ChatHistory> ActiveChatChanged;
        protected virtual void OnActiveChatChanged(ChatHistory chatHistory)
        {
            ActiveChatChanged?.Invoke(this, chatHistory);
        }

        public event EventHandler ChatTitleChanged;
        protected virtual void OnChatTitleChanged()
        {
            ChatTitleChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<string> StatusChanged;
        protected virtual void OnStatusChanged(string status)
        {
            StatusChanged?.Invoke(this, status);
        }

        internal async Task CreateTitlesAsync()
        {
            foreach(var chatHistory in _chatHistories)
            {
                if(chatHistory != null && !chatHistory.IsTitleGenerated)
                {
                    var title = await UpdateTitleForChat(chatHistory);
                }
            }
        }
    }
}

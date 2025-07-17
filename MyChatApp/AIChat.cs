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
#pragma warning disable SKEXP0001

namespace MyChatApp
{
    public class AIChat
    {
        public readonly ILogger<AIChat> _logger;

        public string ActiveChatKey { get; private set; } = Guid.NewGuid().ToString();
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

            // Set the active chat to the new chat
            Task.Run(() => GetTitleForChat(activeChatKey));


            // Create a new chat history
            _chatHistories.Add(new ChatDetails { Name = Guid.NewGuid().ToString(), ChatHistory = new ChatHistory() });

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

        public async Task<String> GetTitleForChat(string key)
        {
            OnStatusChanged("Creating title...");

            // Check if the chat history exists
            var chatDetails = _chatHistories.FirstOrDefault(x => x.Name == key) ?? throw new KeyNotFoundException($"Chat history with key {key} does not exist.");

            var chat = chatDetails.ChatHistory;

            // Log the user message
            _logger.LogInformation("Get Title");

            // Add the user message to the active chat history
            chat.AddUserMessage("Create a short title for this chat so that it can be shown in the list.");

            var (_kernel, _chatCompletionService, _promptExecutionSettings) = _aiChatProviders.GetKernelAndSettings("siemens", false);

            // Get the response from the AI model
            var response = await _chatCompletionService.GetChatMessageContentAsync(
                chat,
                _promptExecutionSettings,
                _kernel);
            chat.RemoveAt(chat.Count - 1); // Remove the last user message

            // Add the AI response to the active chat history
            chatDetails.Name = response.Content;

            OnStatusChanged("Updaing title...");

            // Notify subscribers that the chat history has been updated
            OnChatTitleChanged();

            return response.Content;
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

    }
}

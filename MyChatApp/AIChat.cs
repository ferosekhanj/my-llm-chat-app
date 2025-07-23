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
        public ChatDetails ActiveChat { get; private set; } = new ();

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

            // Get logger from the central AppLogger
            _logger = AppLogger.GetLogger<AIChat>();

            _chatHistories.Add(ActiveChat);
            
            _logger.LogInformation("AIChat initialized with {ProviderCount} providers available", _aiChatProviders.AvailableProviders.Count);
        }

        public void CreateNewChat()
        {
            // Create a new chat history
            var newChat = new ChatDetails { Name = $"New Chat {RunningCount++}" };
            _chatHistories.Insert(0, newChat);

            // Log the creation of a new chat
            _logger.LogInformation("New chat created with name '{ChatName}', total chats: {ChatCount}", newChat.Name, _chatHistories.Count);
        }

        public void SelectChat(ChatDetails details)
        {
            if (details == null || ActiveChat.Name == details.Name)
            {
                _logger.LogDebug("Chat selection skipped - details null: {IsNull}, same as active: {IsSame}", 
                    details == null, details?.Name == ActiveChat.Name);
                return;
            }

            var previousChatName = ActiveChat.Name;
            // Create a new chat history
            ActiveChat = details;

            _logger.LogInformation("Selected chat changed from '{PreviousChat}' to '{NewChat}'", previousChatName, ActiveChat.Name);

            // Notify subscribers that the chat history has been updated
            OnActiveChatChanged(ActiveChat.ChatHistory);
        }

        public async IAsyncEnumerable<String> GetResponseAsync(string userMessage, bool enableStreaming = true, string fileAttachment = null, string modelId = "siemens", bool useTools = false, IList<string> selectedTools = null)
        {
            OnStatusChanged("Working...");
            _logger.LogInformation("Starting chat response - Model: {ModelId}, Streaming: {Streaming}, Tools: {UseTools}, Attachment: {HasAttachment}", 
                modelId, enableStreaming, useTools, !string.IsNullOrEmpty(fileAttachment));
                
            // Log the user message
            _logger.LogInformation("User message received: {UserMessage}", userMessage);

            // Add the file attachment
            if(fileAttachment != null)
            {
                _logger.LogInformation("Processing file attachment: {FilePath}", fileAttachment);
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
            
            var (_kernel, _chatCompletionService, _promptExecutionSettings) = _aiChatProviders.GetKernelAndSettings(modelId, useTools, selectedTools);
            
            _logger.LogInformation("Using kernel with {PluginCount} plugins loaded", _kernel.Plugins.Count);
            foreach (var plugins in _kernel.Plugins)
            {
                _logger.LogDebug("Plugin loaded: {PluginName} with {FunctionCount} functions", plugins.Name, plugins.Count());
            }

            ChatHistorySummarizationReducer reducer = new ChatHistorySummarizationReducer(_chatCompletionService, 4);
            var reducedMessages = await reducer.ReduceAsync(ActiveChat.ShortChatHistory);

            if (reducedMessages is not null)
            {
                _logger.LogDebug("Chat history reduced from {OriginalCount} to {ReducedCount} messages", 
                    ActiveChat.ShortChatHistory.Count, reducedMessages.Count());
                ActiveChat.SetReducedHistory(new ChatHistory(reducedMessages));
            }

            var fullResponse = "";
            if (!enableStreaming)
            {
                _logger.LogDebug("Using non-streaming response mode");

                // Get the response from the AI model
                var response = await _chatCompletionService.GetChatMessageContentsAsync(
                    ActiveChat.ShortChatHistory,
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
                _logger.LogDebug("Using streaming response mode");
                
                // Get the response from the AI model
                var response = _chatCompletionService.GetStreamingChatMessageContentsAsync(
                    ActiveChat.ShortChatHistory,
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

            _logger.LogInformation("AI response completed - Length: {ResponseLength} characters", fullResponse.Length);

            // Add the AI response to the active chat history
            ActiveChat.AddAssistantMessage(fullResponse);

        }

        private KernelContent GetFileAttachmentContent(string fileAttachment)
        {
            if (string.IsNullOrEmpty(fileAttachment) || !File.Exists(fileAttachment))
            {
                _logger.LogWarning("File attachment invalid or not found: {FilePath}", fileAttachment);
                return null;
            }

            var extension = Path.GetExtension(fileAttachment).ToLower();
            _logger.LogDebug("Processing file attachment with extension: {Extension}", extension);

            switch(extension)
            {
                case ".txt":
                    var fileContent = File.ReadAllText(fileAttachment);
                    _logger.LogDebug("Text file loaded - Size: {Size} characters", fileContent.Length);
                    // Create a TextContent object
                    return new TextContent(fileContent);
                // Add more cases for other file types if needed
                case ".jpg":
                case ".jpeg":
                    var fileBytes1 = File.ReadAllBytes(fileAttachment);
                    _logger.LogDebug("JPEG image loaded - Size: {Size} bytes", fileBytes1.Length);
                    // Create an ImageContent object
                    return new ImageContent(fileBytes1, "image/jpeg");
                case ".png":
                    var fileBytes2 = File.ReadAllBytes(fileAttachment);
                    _logger.LogDebug("PNG image loaded - Size: {Size} bytes", fileBytes2.Length);
                    // Create an ImageContent object
                    return new ImageContent(fileBytes2, "image/png");
                default:
                    _logger.LogError("Unsupported file type: {Extension} for file: {FilePath}", extension, fileAttachment);
                    throw new NotSupportedException($"File type not supported: {fileAttachment}");
            }

            // Read the file content
        }

        public async Task<String> UpdateTitleForChat(ChatDetails chatDetails)
        {
            _logger.LogDebug("Starting title generation for chat: {ChatName}", chatDetails.Name);
            
            var chat = new ChatHistory();
            foreach(var message in chatDetails.ChatHistory)
            {
                // Add the messages to the new chat history
                chat.Add(message);
            }

            if (chat.Count < 2)
            {
                _logger.LogDebug("Insufficient messages for title generation - Count: {MessageCount}", chat.Count);
                return "No messages";
            }

            // Log the user message
            _logger.LogInformation("Generating title for chat with {MessageCount} messages", chat.Count);

            // Add the user message to the active chat history
            chat.AddUserMessage("You are an AI assistant that summarizes conversations into short, clear, and engaging titles (3–6 words). Based on this chat content, generate one concise title that best represents the main topic or purpose of the discussion. Avoid generic terms like 'Chat' or 'Conversation'. Make it descriptive and relevant.");

            if (chatDetails.IsTitleGenerated)
            {
                _logger.LogDebug("Title already generated for chat: {ChatName}", chatDetails.Name);
                return "ALready done";
            }

            var (_kernel, _chatCompletionService, _promptExecutionSettings) = _aiChatProviders.GetKernelAndSettings(ActiveModel);

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

            _logger.LogInformation("Generated title for chat: '{OldName}' -> '{NewTitle}'", chatDetails.Name, cleaned);

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
            _logger.LogInformation("Starting to save chat histories - Total chats: {ChatCount}", _chatHistories.Count);
            
            var savedCount = 0;
            foreach(var  chat in _chatHistories)
            {
                if (chat.IsModified && chat.ChatHistory.Count>0)
                {
                    try
                    {
                        // Save the chat history to a file or database
                        // For example, you can serialize the chat history to JSON and save it to a file
                        var filePath = Path.Combine("ChatHistories", $"{chat.Name}.json");
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                        File.WriteAllText(filePath, JsonSerializer.Serialize(chat, JsonOptions));
                        chat.IsModified = false; // Reset the modified flag after saving
                        savedCount++;
                        
                        _logger.LogDebug("Saved chat history: {ChatName} to {FilePath}", chat.Name, filePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to save chat history: {ChatName}", chat.Name);
                    }
                }
            }
            
            _logger.LogInformation("Chat histories save completed - Saved: {SavedCount}/{TotalCount}", savedCount, _chatHistories.Count);
        }

        public void LoadChatHistories()
        {
            if(!Directory.Exists("ChatHistories"))
            {
                _logger.LogInformation("ChatHistories directory does not exist, skipping load");
                return;
            }
            
            OnStatusChanged("Loading chat histories...");
            _logger.LogInformation("Starting to load chat histories from ChatHistories directory");
            
            var chatHistoryFiles = Directory.GetFiles("ChatHistories", "*.json");
            _logger.LogDebug("Found {FileCount} chat history files to load", chatHistoryFiles.Length);
            
            var loadedCount = 0;
            foreach (var file in chatHistoryFiles)
            {
                try
                {
                    var chatDetails = JsonSerializer.Deserialize<ChatDetails>(File.ReadAllText(file), JsonOptions);
                    if (chatDetails != null)
                    {
                        _chatHistories.Add(chatDetails);
                        chatDetails.IsModified = false;
                        chatDetails.IsTitleGenerated = true;
                        loadedCount++;
                        
                        _logger.LogDebug("Loaded chat history: {ChatName} from {FileName} with {MessageCount} messages", 
                            chatDetails.Name, Path.GetFileName(file), chatDetails.ChatHistory.Count);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to deserialize chat history from file: {FileName} - result was null", Path.GetFileName(file));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load chat history from file: {File}", file);
                }
            }
            
            _logger.LogInformation("Chat histories load completed - Loaded: {LoadedCount}/{FileCount}", loadedCount, chatHistoryFiles.Length);
            OnStatusChanged("Ready.");
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
            var pendingTitles = _chatHistories.Where(ch => ch != null && !ch.IsTitleGenerated).ToList();
            
            if (pendingTitles.Count == 0)
            {
                _logger.LogDebug("No pending title generation tasks found");
                return;
            }
            
            _logger.LogInformation("Starting bulk title generation for {PendingCount} chats", pendingTitles.Count);
            
            var generatedCount = 0;
            foreach(var chatHistory in pendingTitles)
            {
                try
                {
                    var title = await UpdateTitleForChat(chatHistory);
                    generatedCount++;
                    _logger.LogDebug("Generated title {Count}/{Total} for chat: {ChatName}", 
                        generatedCount, pendingTitles.Count, chatHistory.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to generate title for chat: {ChatName}", chatHistory.Name);
                }
            }
            
            _logger.LogInformation("Bulk title generation completed - Generated: {GeneratedCount}/{PendingCount}", 
                generatedCount, pendingTitles.Count);
        }
    }
}

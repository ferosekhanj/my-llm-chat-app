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

        public readonly Kernel _kernel;

        public string ActiveChatKey { get; private set; } = Guid.NewGuid().ToString();
        public ChatHistory ActiveChat { get; private set; } = new ChatHistory();

        private BindingList<ChatDetails> _chatHistories = new();
        public BindingList<ChatDetails> ChatHistories 
        { 
            get => _chatHistories; 
        }

        private BindingList<string> Models { get; } = new();

        private IChatCompletionService _chatCompletionService;
        private ToolRepository _toolRepository;
        private PromptExecutionSettings _promptExecutionSettings;
        private MyChatAppSettings _appSettings;

        public AIChat(ToolRepository toolRepository)
        {
            LoadConfigurations();
            // Create a kernel with OpenAI chat completion

            // Create a kernel with OpenAI chat completion
            //var builder = Kernel.CreateBuilder().AddOpenAIChatCompletion("gpt-4o", _appSettings.OpenAIApiKey);
            //_promptExecutionSettings = new OpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true }) };

            // OpenAI compatible
            //var builder = Kernel.CreateBuilder().AddOpenAIChatCompletion("qwen3-30b-a3b", new Uri("https://api.siemens.com/llm/v1"), _appSettings.SiemensApiKey);
            //_promptExecutionSettings = new OpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true }) };

            // Grok OpenAI compatible
            //var builder = Kernel.CreateBuilder().AddOpenAIChatCompletion("grok-3-mini-fast", new Uri("https://api.x.ai/v1"), _appSettings.GrokApiKey);
            //_promptExecutionSettings = new OpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };

            // ollama
#pragma warning disable SKEXP0070
            var builder = Kernel.CreateBuilder().AddOllamaChatCompletion("gemma3:latest", new Uri("http://localhost:11434"));
            _promptExecutionSettings = new OllamaPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };

            // Add enterprise components
            builder.Services
                .AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Critical));

            // Build the kernel
            _kernel = builder.Build();
            _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();


            // Fetch the logger
            _logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<AIChat>>();

            _chatHistories.Add(new ChatDetails { Name = ActiveChatKey, ChatHistory = ActiveChat });

            _toolRepository = toolRepository;

            _toolRepository.InitializeMcpClients().ContinueWith(async task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    _logger.LogInformation("MCP clients initialized successfully.");
                    var mcpClients = _toolRepository.GetAvailableServers();
                    foreach(var client in mcpClients)
                    {
                        _logger.LogInformation("MCP Client: {Name}", client.ServerInfo.Name);

                        var tools = await client.ListToolsAsync();

                        // Register the MCP clients with the kernel
                        _kernel.Plugins.AddFromFunctions(client.ServerInfo.Name.Replace("-","_"), tools.Select(aiFunction => aiFunction.AsKernelFunction()));
                        _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

                    }
                    OnStatusChanged("Ready.");
                }
                else
                {
                    _logger.LogError("Failed to initialize MCP clients: {Error}", task.Exception?.Message);
                }
            });
        }

        private void LoadConfigurations()
        {
            // Load configuration from User Secrets
            var config = new ConfigurationBuilder()
                .AddUserSecrets<Program>() // based on UserSecretsId in .csproj
                .Build();

            // Bind to strongly typed class
            _appSettings = new MyChatAppSettings();
            config.GetSection("MyChatAppSettings").Bind(_appSettings);
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

        public async IAsyncEnumerable<String> GetResponseAsync(string userMessage, bool enableStreaming = true)
        {
            OnStatusChanged("Working...");
            // Log the user message
            _logger.LogInformation("{UserMessage}", userMessage);

            // Add the user message to the active chat history
            ActiveChat.AddUserMessage(userMessage);

            var promptExecutionSettings = new OpenAIPromptExecutionSettings()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

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
                    promptExecutionSettings,
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
                    promptExecutionSettings,
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

            // Get the response from the AI model
            var response = await _chatCompletionService.GetChatMessageContentAsync(
                chat,
                null,
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

    public class ChatDetails 
    {
        public string Name { get; set;}
        public ChatHistory ChatHistory { get; set; }
        public override string ToString() => Name;
    }
}

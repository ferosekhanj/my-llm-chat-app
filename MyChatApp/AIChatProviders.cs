using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Connectors.OpenAI;
#pragma warning disable SKEXP0001

namespace MyChatApp
{
    public class AIChatProviders
    {
        IKernelBuilder builder;
        private MyChatAppSettings _appSettings;
        private readonly ILogger<AIChatProviders> _logger;

        Dictionary<string, (IKernelBuilder builder, PromptExecutionSettings promptSettings, Kernel kernel)> _kernels = new();

        public IList<string> AvailableProviders => _kernels.Keys.ToList();

        private ToolRepository _toolRepository;

        public AIChatProviders( MyChatAppSettings appSettings, ToolRepository toolRepository)
        {
            _toolRepository = toolRepository;
            _appSettings = appSettings;

            // Get logger from the central AppLogger
            _logger = AppLogger.GetLogger<AIChatProviders>();

            _logger.LogInformation("AIChatProviders initializing with {ProviderCount} providers", 
                _appSettings.LLMProviders.Count);

            LoadProviders();
        }
        private void LoadProviders()
        {
            _logger.LogInformation("Loading LLM providers...");
            
            var loadedCount = 0;
            foreach (var llmProvider in _appSettings.LLMProviders)
            {
                try
                {
                    _logger.LogDebug("Loading provider: {ProviderName} ({ProviderType}) - Model: {Model}", 
                        llmProvider.Name, llmProvider.Type, llmProvider.Model);

                    if (llmProvider.Type == "OpenAI")
                    {
                        // Create a kernel with OpenAI chat completion
                        builder = Kernel.CreateBuilder().AddOpenAIChatCompletion(llmProvider.Model, new Uri(llmProvider.BaseUrl), llmProvider.ApiKey);
                        PromptExecutionSettings _promptExecutionSettings = new OpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true }) };
                        _kernels.Add(llmProvider.Name.ToLowerInvariant(), (builder, _promptExecutionSettings, null!));
                        loadedCount++;
                        
                        _logger.LogInformation("Successfully loaded OpenAI provider: {ProviderName}", llmProvider.Name);
                    }
                    else if (llmProvider.Type == "Ollama")
                    {
                        // ollama
#pragma warning disable SKEXP0070
                        builder = Kernel.CreateBuilder().AddOllamaChatCompletion(llmProvider.Model, new Uri(llmProvider.BaseUrl));
                        PromptExecutionSettings _promptExecutionSettings = new OllamaPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true }) };
                        _kernels.Add(llmProvider.Name.ToLowerInvariant(), (builder, _promptExecutionSettings, null!));
                        loadedCount++;
                        
                        _logger.LogInformation("Successfully loaded Ollama provider: {ProviderName}", llmProvider.Name);
                    }
                    else
                    {
                        _logger.LogWarning("Unknown provider type: {ProviderType} for provider: {ProviderName}", 
                            llmProvider.Type, llmProvider.Name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load provider: {ProviderName} ({ProviderType})", 
                        llmProvider.Name, llmProvider.Type);
                }
            }
            
            _logger.LogInformation("Provider loading completed - Successfully loaded: {LoadedCount}/{TotalCount}", 
                loadedCount, _appSettings.LLMProviders.Count);
            
            // Inject AppLogger's Serilog logger into the builder's services
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddProvider(new Serilog.Extensions.Logging.SerilogLoggerProvider(AppLogger.GetSerilogLogger(), dispose: false));
            });
        }

        public (Kernel, IChatCompletionService, PromptExecutionSettings?) GetKernelAndSettings(string providerName, bool useTools= false,IList<string>? selectedTools=null)
        {
            _logger.LogDebug("Getting kernel and settings for provider: {ProviderName}, UseTools: {UseTools}, SelectedTools: {ToolCount}", 
                providerName, useTools, selectedTools?.Count ?? 0);

            if (_kernels.TryGetValue(providerName, out var kernelInfo))
            {
                if( kernelInfo.kernel == null )
                {
                    _logger.LogInformation("Building kernel for provider: {ProviderName}", providerName);
                    kernelInfo.kernel = kernelInfo.builder.Build();
                    kernelInfo.kernel.AutoFunctionInvocationFilters.Add(AutoFunctionInvocationFilter.GetInstance(_logger));
                    _kernels[providerName] = (kernelInfo.builder, kernelInfo.promptSettings, kernelInfo.kernel);
                }
                var _kernel = kernelInfo.kernel;
                var _promptExecutionSettings = kernelInfo.promptSettings;
                var _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
                ClearTools(kernelInfo.kernel);

                if (useTools)
                {
                    _logger.LogDebug("Adding tools to kernel for provider: {ProviderName}", providerName);
                    AddTools(kernelInfo.kernel, selectedTools);
                }
                else
                {
                    _logger.LogDebug("Tools disabled for provider: {ProviderName}", providerName);
                    _promptExecutionSettings = null;
                }

                _logger.LogDebug("Successfully prepared kernel for provider: {ProviderName} with {PluginCount} plugins", 
                    providerName, _kernel.Plugins.Count);

                return (_kernel,_chatCompletionService,_promptExecutionSettings);
            }
            
            _logger.LogError("Provider '{ProviderName}' not found. Available providers: {AvailableProviders}", 
                providerName, string.Join(", ", _kernels.Keys));
            throw new ArgumentException($"Provider '{providerName}' not found.");
        }

        public void AddTools(Kernel _kernel, IList<string>? selectedTools)
        {
            var allTools = _toolRepository.GetAvailableTools();
            
            _logger.LogDebug("Adding tools to kernel. Available tools: {TotalTools}, Selected tools: {SelectedCount}", 
                allTools.Count, selectedTools?.Count ?? 0);
            
            if (selectedTools == null || selectedTools.Count == 0)
            {
                _logger.LogWarning("No tools selected for addition to kernel");
                return;
            }
            
            // Filter tools based on selected tools list
            var filteredTools = allTools.Where(tool => selectedTools.Contains(tool.Name)).ToList();

            _logger.LogInformation("Adding {FilteredCount} selected tools to kernel from {TotalTools} available", 
                filteredTools.Count, allTools.Count);

            if (filteredTools.Count != selectedTools.Count)
            {
                var missingTools = selectedTools.Except(filteredTools.Select(t => t.Name)).ToList();
                _logger.LogWarning("Some selected tools were not found: {MissingTools}", string.Join(", ", missingTools));
            }

            // Register only the selected MCP tools with the kernel
            if (filteredTools.Any())
            {
                _kernel.Plugins.AddFromFunctions("mcp_tools", filteredTools.Select(aifunction => aifunction.AsKernelFunction()));
                _logger.LogDebug("Successfully added tools to kernel: {ToolNames}", 
                    string.Join(", ", filteredTools.Select(t => t.Name)));
            }
        }

        public void ClearTools(Kernel _kernel)
        {
            var pluginCount = _kernel.Plugins.Count;
            _kernel.Plugins.Clear();
            _logger.LogDebug("Cleared {PluginCount} plugins from kernel", pluginCount);
        }

        public IServiceCollection GetServices() => builder.Services;

        public event EventHandler<string> StatusChanged;
        protected virtual void OnStatusChanged(string status)
        {
            StatusChanged?.Invoke(this, status);
        }
    }
}

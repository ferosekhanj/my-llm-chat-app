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

        Dictionary<string, (IKernelBuilder builder, PromptExecutionSettings promptSettings, Kernel kernel)> _kernels = new();

        public IList<string> AvailableProviders => _kernels.Keys.ToList();

        private ToolRepository _toolRepository;

        public AIChatProviders( MyChatAppSettings appSettings, ToolRepository toolRepository)
        {
            _toolRepository = toolRepository;
            _appSettings = appSettings;

            LoadProviders();

        }
        private void LoadProviders()
        {
            foreach (var llmProvider in _appSettings.LLMProviders)
            {
                if (llmProvider.Type == "OpenAI")
                {
                    // Create a kernel with OpenAI chat completion
                    builder = Kernel.CreateBuilder().AddOpenAIChatCompletion(llmProvider.Model, new Uri(llmProvider.BaseUrl), llmProvider.ApiKey);
                    PromptExecutionSettings _promptExecutionSettings = new OpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true }) };
                    _kernels.Add(llmProvider.Name.ToLowerInvariant(), (builder, _promptExecutionSettings, null));
                }
                else if (llmProvider.Type == "Ollama")
                {
                    // ollama
#pragma warning disable SKEXP0070
                    builder = Kernel.CreateBuilder().AddOllamaChatCompletion(llmProvider.Model, new Uri(llmProvider.BaseUrl));
                    PromptExecutionSettings _promptExecutionSettings = new OllamaPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true }) };
                    _kernels.Add(llmProvider.Name.ToLowerInvariant(), (builder, _promptExecutionSettings, null));
                }
            }
            // Add enterprise components
            builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Critical));
        }

        public (Kernel, IChatCompletionService, PromptExecutionSettings) GetKernelAndSettings(string providerName, bool useTools= false,IList<string> selectedTools=null)
        {
            if (_kernels.TryGetValue(providerName, out var kernelInfo))
            {
                if( kernelInfo.kernel == null )
                {
                    kernelInfo.kernel = kernelInfo.builder.Build();
                    _kernels[providerName] = (kernelInfo.builder, kernelInfo.promptSettings, kernelInfo.kernel);
                }
                var _kernel = kernelInfo.kernel;
                var _promptExecutionSettings = kernelInfo.promptSettings;
                var _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
                ClearTools(kernelInfo.kernel);

                if (useTools)
                {

                    AddTools(kernelInfo.kernel, selectedTools);
                }
                else
                {
                    _promptExecutionSettings = null;
                }

                return (_kernel,_chatCompletionService,_promptExecutionSettings);
            }
            throw new ArgumentException($"Provider '{providerName}' not found.");
        }

        public void AddTools(Kernel _kernel, IList<string> selectedTools)
        {
            var allTools = _toolRepository.GetAvailableTools();
            
            // Filter tools based on selected tools list
            var filteredTools = allTools.Where(tool => selectedTools.Contains(tool.Name)).ToList();

            // Register only the selected MCP tools with the kernel
            if (filteredTools.Any())
            {
                _kernel.Plugins.AddFromFunctions("mcp_tools", filteredTools.Select(aifunction => aifunction.AsKernelFunction()));
            }
        }

        public void ClearTools(Kernel _kernel)
        {
            // Clear the MCP tools from the kernel
            _kernel.Plugins.Clear();
        }

        public IServiceCollection GetServices() => builder.Services;

        public event EventHandler<string> StatusChanged;
        protected virtual void OnStatusChanged(string status)
        {
            StatusChanged?.Invoke(this, status);
        }
    }

}

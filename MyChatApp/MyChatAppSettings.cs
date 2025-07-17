using System.Collections.Generic;
namespace MyChatApp
{
    public class MyChatAppSettings
    {
        public string McpConfigFilePath { get; set; } = "E:\\ws\\chatgpt\\mcp.json";
        public List<LLMProvider> LLMProviders { get; set; } = new List<LLMProvider>();
    }

    public class LLMProvider
    {
        public string Name { get; set; }
        public string Type { get; set; } // "OpenAI" or "Ollama"
        public string Model { get; set; }
        public string BaseUrl { get; set; }
        public string ApiKey { get; set; } // optional for local models like Ollama
    }

}
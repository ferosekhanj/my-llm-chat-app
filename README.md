# MyChatApp

A powerful Windows Forms application that enables chat interactions with multiple Large Language Model (LLM) providers and integrates with Model Context Protocol (MCP) servers, similar to Claude Desktop's functionality.

## Features

- **Multi-Provider Support**: Connect to various LLM providers including:
  - OpenAI and OpenAI-compatible APIs
  - Ollama (local models)
  - X.ai (Grok models)
  - Any OpenAI api compatible model

- **MCP Server Integration**: Leverage Model Context Protocol servers to extend AI capabilities with external tools and data sources. Only Stdio tranport is supported.

- **Function Calling**: Full support for tool/function calling with automatic tool discovery and execution

- **Configurable Settings**: Easy JSON-based configuration for providers and MCP servers (Claude desktop mcp configuration works)

- **Modern UI**: Clean Windows Forms interface built on .NET 9.0

## Version
- 1.0.0 - Initial release with core features
- 1.1.0 - Added support for multiple LLM providers, MCP server integration, and function calling
- 1.2.0 - Chat history management
- 1.3.0 - Added function invocation filter and logging

## Prerequisites

- Windows OS
- .NET 9.0 Runtime
- Visual Studio 2022 or later (for development)

## Installation

1. Clone the repository:
   ```powershell
   git clone <repository-url>
   cd MyChatApp
   ```

2. Build the solution:
   ```powershell
   dotnet build
   ```

3. Configure your settings (see Configuration section below)

4. Run the application:
   ```powershell
   dotnet run --project MyChatApp
   ```

## Configuration

### LLM Providers

Configure your LLM providers in `appSettings.json`:

```json
{
  "McpConfigFilePath": "E:\\ws\\claude-desktop\\mcp.json",
  "LLMProviders": [
    {
      "Name": "OpenAI-GPT-4",
      "Type": "OpenAI",
      "Model": "gpt-4",
      "BaseUrl": "https://api.openai.com/v1",
      "ApiKey": "your-api-key-here"
    },
    {
      "Name": "Local-Ollama",
      "Type": "Ollama",
      "Model": "llama2:latest",
      "BaseUrl": "http://localhost:11434",
      "ApiKey": ""
    }
  ]
}
```

### MCP Servers

Configure MCP servers in your `mcp.json` file:

```json
{
  "McpServers": {
    "filesystem": {
      "Command": "npx",
      "Args": ["-y", "@modelcontextprotocol/server-filesystem", "/path/to/directory"],
      "Env": {}
    },
    "git": {
      "Command": "uvx",
      "Args": ["mcp-server-git", "--repository", "/path/to/repo"],
      "Env": {}
    }
  }
}
```

## Project Structure

```
MyChatApp/
├── Program.cs              # Application entry point
├── ChatForm.cs             # Main UI form
├── AIChatProviders.cs      # LLM provider management
├── ToolRepository.cs       # MCP server and tool management
├── MyChatAppSettings.cs    # Configuration models
├── appSettings.json        # LLM provider configuration
└── mcp.json               # MCP server configuration
```

## Key Components

### AIChatProviders
Manages multiple LLM providers using Microsoft Semantic Kernel framework. Supports:
- Dynamic provider loading
- Function calling with auto-discovery
- Provider-specific prompt execution settings

### ToolRepository
Handles MCP server lifecycle and tool management:
- Loads MCP server configurations
- Establishes stdio transport connections
- Discovers and exposes available tools
- Manages tool execution contexts

### ChatForm
Main Windows Forms interface providing:
- Provider selection
- Chat history management
- Tool toggle functionality
- Real-time status updates

## Dependencies

- **Microsoft.SemanticKernel**: Core AI orchestration framework
- **ModelContextProtocol**: MCP client implementation
- **Markdig**: Markdown processing for chat rendering
- **Microsoft.Extensions.Configuration**: Configuration management
- **Microsoft.Web.WebView2**: Enhanced web content rendering

## Usage

1. **Launch the application** and select your preferred LLM provider from the dropdown
2. **Enable/disable tools** using the tools toggle to control MCP server integration
3. **Start chatting** with your chosen AI model
4. **Leverage MCP tools** for enhanced capabilities like file system access, git operations, or custom integrations

## Development

### Building from Source

```powershell
# Clone and build
git clone <repository-url>
cd MyChatApp
dotnet restore
dotnet build

# Run in development mode
dotnet run --project MyChatApp
```

### Adding New Providers

1. Add provider configuration to `appSettings.json`
2. Update `AIChatProviders.cs` to handle the new provider type
3. Implement provider-specific settings if needed

### Adding MCP Servers

1. Install the MCP server (npm, pip, etc.)
2. Add server configuration to `mcp.json`
3. Restart the application to load the new server

## Acknowledgments

- Built with Microsoft Semantic Kernel
- Leverages Model Context Protocol for extensibility
- Inspired by Claude Desktop's MCP integration



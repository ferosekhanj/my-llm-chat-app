using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using System.ComponentModel;
using Microsoft.Extensions.Logging;

namespace MyChatApp
{
    public class McpServerConfig
    {
        public string Command { get; set; } = string.Empty;
        public string[] Args { get; set; } = Array.Empty<string>();
        public Dictionary<string, string>? Env { get; set; }
    }

    public class McpConfiguration
    {
        public Dictionary<string, McpServerConfig> McpServers { get; set; } = new();
    }

    public class ToolRepository
    {
        private readonly ILogger<ToolRepository> _logger;
        private readonly string _configFilePath;
        private McpConfiguration? _configuration;
        private List<IMcpClient> _mcpClients = new();
        private List<McpClientTool> _mcpTools = new ();
        private MyChatAppSettings _appSettings;
        public ToolRepository(MyChatAppSettings appSettings)
        {
            _logger = AppLogger.GetLogger<ToolRepository>();
            _appSettings = appSettings;
            _configFilePath = appSettings.McpConfigFilePath;
            _logger.LogInformation("Initializing ToolRepository with config file: {ConfigFilePath}", _configFilePath);
            Task.Run(async ()=>await InitializeMcpClients());
        }

        private async Task LoadConfiguration()
        {
            _logger.LogInformation("Loading MCP configuration from {ConfigFilePath}", _configFilePath);
            OnStatusChanged("Loading MCP configuration...");
            if (!File.Exists(_configFilePath))
            {
                _logger.LogWarning("MCP configuration file not found at {ConfigFilePath}", _configFilePath);
                return;
            }

            try
            {
                var jsonContent =  await File.ReadAllTextAsync(_configFilePath);
                _configuration = JsonSerializer.Deserialize<McpConfiguration>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                _logger.LogInformation("Successfully loaded MCP configuration with {ServerCount} servers", _configuration?.McpServers.Count ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load MCP configuration from {ConfigFilePath}", _configFilePath);
                throw;
            }
        }

        private async Task InitializeMcpClients()
        {
            _logger.LogInformation("Starting MCP client initialization");
            if (_configuration == null)
            {
                await LoadConfiguration();
            }

            if (_configuration?.McpServers == null || !_configuration.McpServers.Any())
            {
                _logger.LogInformation("No MCP servers configured");
                OnStatusChanged("Ready.");
                return;
            }

            OnStatusChanged("Please wait creating MCP Servers...");
            _logger.LogInformation("Initializing {ServerCount} MCP servers", _configuration.McpServers.Count);

            var total = _configuration!.McpServers.Count;
            var successful = 0;
            foreach (var server in _configuration!.McpServers)
            {
                try
                {
                    _logger.LogInformation("Creating MCP client for server: {ServerName}", server.Key);
                    OnStatusChanged($"Please wait creating {server.Key}");
                    var clientTransport = new StdioClientTransport(new()
                    {
                        Name = server.Key,
                        Command = server.Value.Command,
                        Arguments = server.Value.Args,
                        EnvironmentVariables = server.Value.Env
                    });

                    var mcpClient = await McpClientFactory.CreateAsync(clientTransport);
                    _mcpClients.Add(mcpClient);
                    var tools = await mcpClient.ListToolsAsync();
                    _mcpTools.AddRange(tools);
                    
                    successful++;
                    _logger.LogInformation("Successfully initialized MCP client for {ServerName} with {ToolCount} tools", 
                        server.Key, tools.Count);
                }
                catch (Exception ex)
                {
                    // Log error but continue with other servers
                    _logger.LogError(ex, "Failed to initialize MCP client for {ServerName}", server.Key);
                    Console.WriteLine($"Failed to initialize MCP client for {server.Key}: {ex.Message}");
                }
            }
            
            _logger.LogInformation("MCP client initialization complete. {Successful}/{Total} servers initialized successfully with {TotalTools} total tools", 
                successful, total, _mcpTools.Count);
            OnToolsLoaded();
            OnStatusChanged("Ready.");
        }

        public IEnumerable<IMcpClient> GetAvailableServers()
        {
            _logger.LogDebug("Getting available servers. Count: {ServerCount}", _mcpClients.Count);
            return _mcpClients;
        }

        public IList<IMcpClient> McpClients
        {
            get
            {
                return _mcpClients;
            }
        }

        public IList<McpClientTool> GetAvailableTools()
        {
            _logger.LogDebug("Getting available tools. Count: {ToolCount}", _mcpTools.Count);
            return _mcpTools;
        }

        public async Task DisposeAsync()
        {
            _logger.LogInformation("Disposing ToolRepository. Cleaning up {ClientCount} MCP clients", _mcpClients.Count);
            foreach (var client in _mcpClients)
            {
                try
                {
                    await client.DisposeAsync();
                    _logger.LogDebug("Successfully disposed MCP client");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error disposing MCP client");
                    Console.WriteLine($"Error disposing MCP client: {ex.Message}");
                }
            }
            _mcpClients.Clear();
            _mcpTools.Clear();
            _logger.LogInformation("ToolRepository disposal complete");
        }

        public event EventHandler<string>? StatusChanged;
        protected virtual void OnStatusChanged(string status)
        {
            _logger.LogDebug("Status changed: {Status}", status);
            StatusChanged?.Invoke(this, status);
        }

        public event EventHandler? ToolsLoaded;
        protected virtual void OnToolsLoaded()
        {
            _logger.LogInformation("Tools loaded event triggered");
            ToolsLoaded?.Invoke(this, EventArgs.Empty);
        }

    }
}

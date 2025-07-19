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
        private readonly string _configFilePath;
        private McpConfiguration? _configuration;
        private List<IMcpClient> _mcpClients = new();
        private List<McpClientTool> _mcpTools = new ();
        private MyChatAppSettings _appSettings;
        public ToolRepository(MyChatAppSettings appSettings)
        {
            _appSettings = appSettings;
            _configFilePath = appSettings.McpConfigFilePath;
            Task.Run(async ()=>await InitializeMcpClients());
        }

        private async Task LoadConfiguration()
        {
            OnStatusChanged("Loading MCP configuration...");
            if (!File.Exists(_configFilePath))
            {
                return;
            }

            var jsonContent =  await File.ReadAllTextAsync(_configFilePath);
            _configuration = JsonSerializer.Deserialize<McpConfiguration>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task InitializeMcpClients()
        {
            if (_configuration == null)
            {
                await LoadConfiguration();
            }

            OnStatusChanged("Please wait creating MCP Servers...");

            var total = _configuration!.McpServers.Count;
            var loaded = 0;
            foreach (var server in _configuration!.McpServers)
            {
                try
                {
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
                }
                catch (Exception ex)
                {
                    // Log error but continue with other servers
                    Console.WriteLine($"Failed to initialize MCP client for {server.Key}: {ex.Message}");
                }
            }
            OnToolsLoaded();
            OnStatusChanged("Ready.");
        }

        public IEnumerable<IMcpClient> GetAvailableServers()
        {
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
            return _mcpTools;
        }

        public async Task DisposeAsync()
        {
            foreach (var client in _mcpClients)
            {
                try
                {
                    await client.DisposeAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error disposing MCP client: {ex.Message}");
                }
            }
            _mcpClients.Clear();
        }

        public event EventHandler<string> StatusChanged;
        protected virtual void OnStatusChanged(string status)
        {
            StatusChanged?.Invoke(this, status);
        }

        public event EventHandler ToolsLoaded;
        protected virtual void OnToolsLoaded()
        {
            ToolsLoaded?.Invoke(this, EventArgs.Empty);
        }

    }
}

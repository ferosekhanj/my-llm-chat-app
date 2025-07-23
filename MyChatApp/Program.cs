using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.Logging;
using System.Windows.Forms;

namespace MyChatApp
{
    public class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                // Set up dependency injection and logging first
                var serviceProvider = ConfigureServices();
                
                // Initialize unified logging across the app
                AppLogger.Initialize(serviceProvider);
                
                var logger = AppLogger.GetLogger<Program>();
                
                logger.LogInformation("Application starting - {ApplicationName} v{Version}", 
                    VersionInfo.ProductName, VersionInfo.Version);

                // Load configuration from User Secrets
                var config = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();

                logger.LogDebug("Configuration loaded from {BasePath}", AppDomain.CurrentDomain.BaseDirectory);

                // Bind to strongly typed class
                MyChatAppSettings _appSettings = new MyChatAppSettings();
                config.Bind(_appSettings);
                
                logger.LogInformation("Application settings loaded. MCP Config: {McpPath}", 
                    _appSettings.McpConfigFilePath ?? "Not specified");
                logger.LogDebug("Loaded {ProviderCount} LLM providers from configuration", 
                    _appSettings.LLMProviders?.Count ?? 0);

                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.
                ApplicationConfiguration.Initialize();
                
                logger.LogInformation("WinForms application configuration initialized");
                
                // Set up global exception handling
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.ThreadException += Application_ThreadException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                
                logger.LogDebug("Global exception handlers registered");
                
                logger.LogInformation("Starting main chat form");
                Application.Run(new ChatForm(_appSettings));
                
                logger.LogInformation("Application shutting down normally");
            }
            catch (Exception ex)
            {
                // Try to log the error, but use MessageBox as fallback
                try
                {
                    var logger = AppLogger.GetLogger<Program>();
                    logger.LogCritical(ex, "Fatal error during application startup");
                }
                catch
                {
                    // Logger not available, use MessageBox
                }
                
                MessageBox.Show($"Fatal startup error: {ex.Message}", "MyChatApp - Startup Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            
            // Configure logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
                
                // Filter out noisy logs from other libraries
                builder.AddFilter("Microsoft", LogLevel.Warning);
                builder.AddFilter("System", LogLevel.Warning);
                builder.AddFilter("Microsoft.SemanticKernel", LogLevel.Information);
            });

            return services.BuildServiceProvider();
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            try
            {
                var logger = AppLogger.GetLogger<Program>();
                logger.LogError(e.Exception, "Unhandled thread exception occurred");
            }
            catch
            {
                // Logger not available
            }
            
            var result = MessageBox.Show(
                $"An unexpected error occurred:\n\n{e.Exception.Message}\n\nWould you like to continue running the application?",
                "MyChatApp - Unexpected Error",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Error);

            if (result == DialogResult.No)
            {
                try
                {
                    var logger = AppLogger.GetLogger<Program>();
                    logger.LogInformation("User chose to exit application after unhandled exception");
                }
                catch { }
                
                Application.Exit();
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            
            try
            {
                var logger = AppLogger.GetLogger<Program>();
                logger.LogCritical(exception, "Unhandled domain exception occurred. Terminating: {IsTerminating}", e.IsTerminating);
            }
            catch
            {
                // Logger not available
            }
            
            if (e.IsTerminating)
            {
                MessageBox.Show(
                    $"A fatal error occurred and the application must close:\n\n{exception?.Message ?? "Unknown error"}",
                    "MyChatApp - Fatal Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop);
            }
        }
    }
}
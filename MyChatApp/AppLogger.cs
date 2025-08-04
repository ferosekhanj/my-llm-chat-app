using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace MyChatApp
{
    /// <summary>
    /// Static logger factory for unified logging across the application using Serilog
    /// </summary>
    public static class AppLogger
    {
        /// <summary>
        /// Get the underlying Serilog logger for DI integration
        /// </summary>
        public static Serilog.ILogger GetSerilogLogger() => _serilogLogger;
        private static IServiceProvider? _serviceProvider;
        private static ILoggerFactory? _loggerFactory;
        private static readonly Serilog.ILogger _serilogLogger = CreateSerilogLogger();

        /// <summary>
        /// Create and configure the Serilog logger
        /// </summary>
        private static Serilog.ILogger CreateSerilogLogger()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "MyChatApp")
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/mychatapp-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }

        /// <summary>
        /// Initialize the logger with a service provider
        /// </summary>
        /// <param name="serviceProvider">The service provider containing logging configuration</param>
        public static void Initialize(IServiceProvider? serviceProvider = null)
        {
            _serviceProvider = serviceProvider;
            
            // Create a Serilog logger factory
            var serilogLoggerFactory = new SerilogLoggerFactory(_serilogLogger);
            _loggerFactory = serilogLoggerFactory;
            
            // Set Serilog as the global logger
            Log.Logger = _serilogLogger;
            
            _serilogLogger.Information("AppLogger initialized with Serilog");
        }

        /// <summary>
        /// Get a logger for the specified type
        /// </summary>
        /// <typeparam name="T">The type to create a logger for</typeparam>
        /// <returns>A logger instance</returns>
        public static ILogger<T> GetLogger<T>()
        {
            if (_loggerFactory == null)
            {
                throw new InvalidOperationException("Logger not initialized. Call AppLogger.Initialize() first.");
            }
            return _loggerFactory.CreateLogger<T>();
        }

        /// <summary>
        /// Get a logger for the specified category name
        /// </summary>
        /// <param name="categoryName">The category name for the logger</param>
        /// <returns>A logger instance</returns>
        public static Microsoft.Extensions.Logging.ILogger GetLogger(string categoryName)
        {
            if (_loggerFactory == null)
            {
                throw new InvalidOperationException("Logger not initialized. Call AppLogger.Initialize() first.");
            }
            return _loggerFactory.CreateLogger(categoryName);
        }

        /// <summary>
        /// Get the global service provider
        /// </summary>
        /// <returns>The service provider</returns>
        public static IServiceProvider GetServiceProvider()
        {
            return _serviceProvider ?? throw new InvalidOperationException("Service provider not initialized.");
        }

        /// <summary>
        /// Check if the logger is initialized
        /// </summary>
        public static bool IsInitialized => _loggerFactory != null;
    }
}

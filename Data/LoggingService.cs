using Microsoft.AspNetCore.Connections;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System.Runtime.CompilerServices;

namespace LittleArkFoundation.Data
{
    public class LoggingService
    {
        private static readonly Serilog.ILogger _logger;

        static LoggingService()
        {
            try
            {
                Serilog.Debugging.SelfLog.Enable(Console.Out);

                var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

                var connectionService = new ConnectionService(configuration);

                _logger = new LoggerConfiguration()
                    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
                    .WriteTo.MSSqlServer(
                        connectionString: connectionService.GetConnectionString("main"),
                        sinkOptions: new MSSqlServerSinkOptions
                        {
                            TableName = "Logs",
                            AutoCreateSqlTable = true
                        })
                    .CreateLogger();

                _logger.Information("Logging service initialized successfully. Test log.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoggingService initialization failed: {ex.Message}");
                throw;
            }
        }

        // Public method to expose the logger
        public static Serilog.ILogger GetLogger() => _logger;

        // Helper methods to log directly from this service
        public static void LogInformation(string message) => _logger.Information(message);
        public static void LogError(string message, Exception ex = null) => _logger.Error(ex, message);
        public static void LogWarning(string message) => _logger.Warning(message);
        public static void Shutdown() => Log.CloseAndFlush();
    }
}

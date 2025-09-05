using LittleArkFoundation.Hubs;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System.Runtime.CompilerServices;

namespace LittleArkFoundation.Data
{
    public class LoggingService
    {
        private static readonly Serilog.ILogger _logger;
        public static IHubContext<LogsHub>? HubContext { get; set; }

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
                        connectionString: connectionService.GetDefaultConnectionString(),
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

        private static async Task BroadcastAsync(string level, string message)
        {
            if (HubContext != null)
            {
                await HubContext.Clients.All.SendAsync("ReceiveLog", new
                {
                    id = 0, // database auto-generates ID
                    message,
                    level,
                    timeStamp = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt")
                });
            }
        }

        // Public method to expose the logger
        public static Serilog.ILogger GetLogger() => _logger;

        // Helper methods to log directly from this service
        public static void LogInformation(string message)
        {
            _logger.Information(message);
            _ = BroadcastAsync("Information", message);
        }

        public static void LogError(string message, Exception? ex = null)
        {
            _logger.Error(ex, message);
            _ = BroadcastAsync("Error", message);
        }

        public static void LogWarning(string message)
        {
            _logger.Warning(message);
            _ = BroadcastAsync("Warning", message);
        }

        public static void Shutdown() => Log.CloseAndFlush();
    }
}

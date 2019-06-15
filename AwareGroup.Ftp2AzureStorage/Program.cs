using AwareGroup.FtpServer.FileSystem.AzureStorage;
using FubarDev.FtpServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Threading.Tasks;

namespace AwareGroup.Ftp2AzureStorage
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            #region Logging Setup
            //configure Serilog
            var loggerConfig = new LoggerConfiguration();
            var logLevel = Configuration.FtpServerLogLevel.ToLower();
            if (logLevel == "verbose") loggerConfig = loggerConfig.MinimumLevel.Verbose();
            else if (logLevel == "information") loggerConfig = loggerConfig.MinimumLevel.Information();
            else if (logLevel == "warning") loggerConfig = loggerConfig.MinimumLevel.Warning();
            else if (logLevel == "debug") loggerConfig = loggerConfig.MinimumLevel.Debug();
            else if (logLevel == "error") loggerConfig = loggerConfig.MinimumLevel.Error();
            else if (logLevel == "fatal") loggerConfig = loggerConfig.MinimumLevel.Fatal();
            else loggerConfig = loggerConfig.MinimumLevel.Verbose();

            Log.Logger = loggerConfig
                .Enrich.FromLogContext()
                .WriteTo.Console()
                //TODO: Add additional log outputs
                .CreateLogger();
            #endregion

            var hostBuilder = new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                //attach serillog
                services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

                //attach ftp server
                services.AddFtpServer(builder => builder.UseAzureStorage());
                services.Configure<FtpServerOptions>(opt =>
                {
                    opt.ServerAddress = Configuration.FtpServerIpAddress;
                    opt.MaxActiveConnections = Configuration.FtpServerMaxConnections;
                    opt.Port = Configuration.FtpServerTcpPort;
                });

                //attach plumbing to start service
                services.AddHostedService<FtpServerBridgeService>();
            });

            await hostBuilder.RunConsoleAsync();
        }
    }
}

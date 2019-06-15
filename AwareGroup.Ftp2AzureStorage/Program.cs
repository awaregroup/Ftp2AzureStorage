using AwareGroup.FtpServer.FileSystem.AzureStorage;
using FubarDev.FtpServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Threading;

namespace AwareGroup.Ftp2AzureStorage
{
    class Program
    {
        static void Main(string[] args)
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


            //set up dependency injection
            var services = new ServiceCollection();
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
            services.AddFtpServer(builder => builder.UseAzureStorage());
            services.Configure<FtpServerOptions>(opt =>
            {
                opt.ServerAddress = Configuration.FtpServerIpAddress;
                opt.MaxActiveConnections = Configuration.FtpServerMaxConnections;
                opt.Port = Configuration.FtpServerTcpPort;
            });


            using (var serviceProvider = services.BuildServiceProvider())
            {
                var logger = serviceProvider.GetService<ILogger<Program>>();
                var source = new CancellationTokenSource();

                //initialize the FTP server
                var ftpServerHost = serviceProvider.GetRequiredService<IFtpServerHost>();

                //start the FTP server
                logger.LogInformation("Starting server...");
                ftpServerHost.StartAsync(source.Token).Wait();
                logger.LogInformation("");
                logger.LogInformation("                      __");
                logger.LogInformation(" _____ _____ _____    \\ \\   _____                    _____ _");
                logger.LogInformation("|   __|_   _|  _  |___ \\ \\ |  _  |___ _ _ ___ ___   |   __| |_ ___ ___ ___ ___ ___");
                logger.LogInformation("|   __| | | |   __|___| > >|     |- _| | |  _| -_|  |__   |  _| . |  _| .'| . | -_|");
                logger.LogInformation("|__|    |_| |__|       / / |__|__|___|___|_| |___|  |_____|_| |___|_| |__,|_  |___|");
                logger.LogInformation("                      /_/                                                 |___|");
                logger.LogInformation("");
                logger.LogInformation("");

                //loop to keep main thread alive
                while (source.Token.IsCancellationRequested == false)
                {
                    logger.LogInformation("Type 'exit' to quit...");
                    var line = Console.ReadLine().Trim().ToLower();
                    if (line == "exit") source.Cancel();
                }

                ftpServerHost.StopAsync(source.Token).Wait();
            }
        }
    }
}

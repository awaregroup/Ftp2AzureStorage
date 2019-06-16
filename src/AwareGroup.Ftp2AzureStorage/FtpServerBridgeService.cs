using FubarDev.FtpServer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AwareGroup.Ftp2AzureStorage
{
    public class FtpServerBridgeService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IFtpServerHost _ftpServerHost;
        private readonly FtpServerOptions _ftpServerOptions;

        public FtpServerBridgeService(IFtpServerHost ftpServerHost, ILogger<FtpServerBridgeService> logger)
        {
            _ftpServerHost = ftpServerHost;
            _ftpServerOptions = Configuration.FtpServerOptions;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //initialize the FTP server
            _logger.LogInformation("Starting server...");
            await _ftpServerHost.StartAsync(cancellationToken);

            _logger.LogInformation("");
            _logger.LogInformation(Configuration.WelcomeMessage);
            _logger.LogInformation("");
            _logger.LogInformation($"\r\n{JsonConvert.SerializeObject(Configuration.FtpServerOptions, Formatting.Indented)}");
            _logger.LogInformation("");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping server...");
            await _ftpServerHost.StopAsync(cancellationToken);
            _logger.LogInformation("Server stopped.");
        }

        public void Dispose()
        {
        }
    }
}

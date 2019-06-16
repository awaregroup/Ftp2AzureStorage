using AwareGroup.FtpServer.FileSystem.AzureStorage;
using FubarDev.FtpServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Linq;
using System.Threading.Tasks;

namespace AwareGroup.Ftp2AzureStorage
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            Configuration.ConfigureLogging();

            var hostBuilder = new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                //attach serilog
                services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

                //attach ftp server
                services.AddFtpServer(builder => builder.UseAzureStorage());
                services.Configure<FtpServerOptions>(opt =>
                {
                    opt.MaxActiveConnections = Configuration.FtpServerOptions.MaxActiveConnections;
                    opt.PasvAddress = Configuration.FtpServerOptions.PasvAddress;
                    opt.PasvMaxPort = Configuration.FtpServerOptions.PasvMaxPort;
                    opt.PasvMinPort = Configuration.FtpServerOptions.PasvMinPort;
                    opt.Port = Configuration.FtpServerOptions.Port;
                    opt.ServerAddress = Configuration.FtpServerOptions.ServerAddress;
                });

                //attach plumbing to start service
                services.AddHostedService<FtpServerBridgeService>();
            });

            await hostBuilder.RunConsoleAsync();
        }
    }
}

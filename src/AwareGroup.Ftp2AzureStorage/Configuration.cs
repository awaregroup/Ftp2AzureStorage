using FubarDev.FtpServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Serilog;
using System.Threading.Tasks;

namespace AwareGroup.Ftp2AzureStorage
{
    internal static class Configuration
    {
        //FTP Server Settings
        private static string FtpServerIpAddress => GetEnvironmentVariable(nameof(FtpServerIpAddress), "0.0.0.0");
        private static int FtpServerTcpPort => GetEnvironmentVariable(nameof(FtpServerTcpPort), 21);
        private static int FtpServerMaxConnections => GetEnvironmentVariable(nameof(FtpServerMaxConnections), 10);

        private static string FtpServerPasvIpAddress => GetEnvironmentVariable(nameof(FtpServerPasvIpAddress), string.Empty);
        private static string FtpServerPasvFqdn => GetEnvironmentVariable(nameof(FtpServerPasvFqdn), "localhost");
        private static int FtpServerPasvMinPort => GetEnvironmentVariable(nameof(FtpServerPasvMinPort), 50000);
        private static int FtpServerPasvMaxPort => GetEnvironmentVariable(nameof(FtpServerPasvMaxPort), 50010);

        //Logging
        private static string FtpServerLogLevel => GetEnvironmentVariable(nameof(FtpServerLogLevel), "Verbose");
        public static string WelcomeMessage => "\r\n                      __\r\n _____ _____ _____    \\ \\   _____                    _____ _\r\n|   __|_   _|  _  |___ \\ \\ |  _  |___ _ _ ___ ___   |   __| |_ ___ ___ ___ ___ ___\r\n|   __| | | |   __|___| > >|     |- _| | |  _| -_|  |__   |  _| . |  _| .'| . | -_|\r\n|__|    |_| |__|       / / |__|__|___|___|_| |___|  |_____|_| |___|_| |__,|_  |___|\r\n                      /_/                                                 |___|";


        //Helpers
        public static void ConfigureLogging() 
        {
            //configure Serilog
            var loggerConfig = new LoggerConfiguration();
            var logLevel = FtpServerLogLevel.ToLower();
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
        }

        private static FtpServerOptions _serverOptions;
        internal static FtpServerOptions FtpServerOptions
        {
            get
            {
                if (_serverOptions == null)
                {
                    //select correct PASV IP Address
                    var pasvIpAddress = FtpServerPasvIpAddress;
                    if (string.IsNullOrEmpty(pasvIpAddress) == true)
                    {
                        var ip = Dns.GetHostEntry(FtpServerPasvFqdn);
                        pasvIpAddress = ip.AddressList.FirstOrDefault()?.MapToIPv4().ToString();
                    }

                    _serverOptions = new FtpServerOptions
                    {
                        MaxActiveConnections = FtpServerMaxConnections,
                        ServerAddress = FtpServerIpAddress,
                        Port = FtpServerTcpPort,
                        PasvAddress = pasvIpAddress,
                        PasvMinPort = FtpServerPasvMinPort,
                        PasvMaxPort = FtpServerPasvMaxPort
                    };
                }

                return _serverOptions;
            }
        }

        private static T GetEnvironmentVariable<T>(string name, T defaultValue)
        {
            T ret = defaultValue;

            var val = Environment.GetEnvironmentVariable(name);
            if (val != null)
            {
                if (typeof(T) == typeof(int))
                {
                    var convert = int.TryParse(val, out int newval);
                    if (convert == true) ret = (T)(object)newval;
                }
                else ret = (T)(object)val;
            }

            return ret;
        }
    }
}

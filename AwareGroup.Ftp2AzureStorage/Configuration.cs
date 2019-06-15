using System;
using System.Collections.Generic;
using System.Text;

namespace AwareGroup.Ftp2AzureStorage
{
    internal static class Configuration
    {
        //FTP Server Settings
        public static string FtpServerIpAddress => GetEnvironmentVariable(nameof(FtpServerIpAddress), "0.0.0.0");
        public static int FtpServerTcpPort => GetEnvironmentVariable(nameof(FtpServerTcpPort), 21);
        public static int FtpServePasvMinPort => GetEnvironmentVariable(nameof(FtpServePasvMinPort), 50000);
        public static int FtpServePasvMaxPort => GetEnvironmentVariable(nameof(FtpServePasvMaxPort), 50010);
        public static int FtpServerMaxConnections => GetEnvironmentVariable(nameof(FtpServerMaxConnections), 10);

        //Logging
        public static string FtpServerLogLevel => GetEnvironmentVariable(nameof(FtpServerLogLevel), "Verbose");
        public static string WelcomeMessage => "\r\n                      __\r\n _____ _____ _____    \\ \\   _____                    _____ _\r\n|   __|_   _|  _  |___ \\ \\ |  _  |___ _ _ ___ ___   |   __| |_ ___ ___ ___ ___ ___\r\n|   __| | | |   __|___| > >|     |- _| | |  _| -_|  |__   |  _| . |  _| .'| . | -_|\r\n|__|    |_| |__|       / / |__|__|___|___|_| |___|  |_____|_| |___|_| |__,|_  |___|\r\n                      /_/                                                 |___|";


        //Helpers
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

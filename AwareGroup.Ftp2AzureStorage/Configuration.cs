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
        public static int FtpServerMaxConnections => GetEnvironmentVariable(nameof(FtpServerMaxConnections), 10);

        //Logging
        public static string FtpServerLogLevel => GetEnvironmentVariable(nameof(FtpServerLogLevel), "Verbose");

        ////Azure Blob Storage Settings
        //public static string AzureStorageAccountName => GetEnvironmentVariable(nameof(AzureStorageAccountName), "");
        //public static string AzureStorageAccountKey => GetEnvironmentVariable(nameof(AzureStorageAccountKey), "");



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

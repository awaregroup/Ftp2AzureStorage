using FubarDev.FtpServer;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.AccountManagement;

namespace AwareGroup.FtpServer.FileSystem.AzureStorage
{
    public static class AzureStorageFtpServerBuilderExtensions
    {
        /// <summary>
        /// Uses Azure Blob Storage as file system.
        /// </summary>
        /// <param name="builder">The server builder used to configure the FTP server.</param>
        /// <returns>the server builder used to configure the FTP server.</returns>
        public static IFtpServerBuilder UseAzureStorage(this IFtpServerBuilder builder)
        {
            builder.Services
                .AddSingleton<IFileSystemClassFactory, AzureStorageFileSystemProvider>()
                .AddSingleton<IMembershipProvider, AzureStorageMembershipProvider>()
                ;

            return builder;
        }
    }
}

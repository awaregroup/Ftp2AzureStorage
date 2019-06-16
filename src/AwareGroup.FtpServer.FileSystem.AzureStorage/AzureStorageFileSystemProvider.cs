using FubarDev.FtpServer;
using FubarDev.FtpServer.FileSystem;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AwareGroup.FtpServer.FileSystem.AzureStorage
{
    public class AzureStorageFileSystemProvider : IFileSystemClassFactory
    {
        ILogger<AzureStorageFileSystemProvider> _logger;
        ILogger<AzureStorageFileSystem> _fslogger;

        public AzureStorageFileSystemProvider(ILogger<AzureStorageFileSystemProvider> logger, ILogger<AzureStorageFileSystem> fslogger)
        {
            _logger = logger;
            _fslogger = fslogger;
        }

        public async Task<IUnixFileSystem> Create(IAccountInformation accountInformation)
        {
            _logger.LogTrace($"Generating new {nameof(AzureStorageFileSystem)} using storage account name and key...");
            var user = accountInformation.User as AzureStorageFtpUser;
            var fs = new AzureStorageFileSystem(user.AccountName, user.AccountKey, _fslogger);
            return fs;
        }
    }
}

using FubarDev.FtpServer.AccountManagement;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwareGroup.FtpServer.FileSystem.AzureStorage
{
    public class AzureStorageMembershipProvider : IMembershipProvider
    {
        ILogger _logger;

        public AzureStorageMembershipProvider(ILogger<AzureStorageMembershipProvider> logger)
        {
            _logger = logger;
        }

        public async Task<MemberValidationResult> ValidateUserAsync(string username, string password)
        {
            _logger.LogInformation($"Validating credentials...");

            //reject details that don't conform to azure storage conventions
            _logger.LogTrace($"Validating password complexity");
            if (password.Length < 80 || password.Length > 92)
            {
                _logger.LogTrace("Password is not compliant.");
                return new MemberValidationResult(MemberValidationStatus.InvalidLogin);
            }

            //verify details by creating a storage client
            var cred = new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(username, password);

            _logger.LogTrace("Logging into storage account...");
            var storageAccount = new CloudStorageAccount(cred, true);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var props = await blobClient.GetAccountPropertiesAsync();
            _logger.LogTrace($"Login successful?  {props != null}");
            _logger.LogTrace($"Storage account kind: '{props.AccountKind}'   SKU: '{props.SkuName}'");


            var ftpUser = new AzureStorageFtpUser { AccountName = username, AccountKey = password };
            return props != null ? 
                new MemberValidationResult(MemberValidationStatus.AuthenticatedUser, ftpUser) : 
                new MemberValidationResult(MemberValidationStatus.InvalidLogin);
        }
    }
}

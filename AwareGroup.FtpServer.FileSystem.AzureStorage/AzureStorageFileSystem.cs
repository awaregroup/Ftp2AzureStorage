using FubarDev.FtpServer.BackgroundTransfer;
using FubarDev.FtpServer.FileSystem;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AwareGroup.FtpServer.FileSystem.AzureStorage
{
    public class AzureStorageFileSystem : IUnixFileSystem, IDisposable
    {
        private CloudStorageAccount _storageAccount;
        private CloudBlobClient _blobClient;
        private ILogger _logger;

        public AzureStorageFileSystem(string storageAccountName, string storageAccountKey, ILogger<AzureStorageFileSystem> logger)
        {
            _logger = logger;

            var cred = new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(storageAccountName, storageAccountKey);
            _storageAccount = new CloudStorageAccount(cred, true);
            _blobClient = _storageAccount.CreateCloudBlobClient();
        }

        public bool SupportsAppend => false;

        public bool SupportsNonEmptyDirectoryDelete => true;

        public StringComparer FileSystemEntryComparer => throw new NotImplementedException();


        public IUnixDirectoryEntry Root
        {
            get
            {
                var ret = new AzureStorageDirectoryEntry("/", 1, false);

                return ret;
            }
        }

        public async Task<IBackgroundTransfer> AppendAsync(IUnixFileEntry fileEntry, long? startPosition, Stream data, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IBackgroundTransfer> CreateAsync(IUnixDirectoryEntry targetDirectory, string fileName, Stream data, CancellationToken cancellationToken)
        {
            var split = targetDirectory.Name.Split('/');
            if (split != null && split.Length > 0)
            {
                var container = _blobClient.GetContainerReference(split.FirstOrDefault());
                var subdir = split.Length > 1 ? string.Join("/", split.Skip(1).Take(split.Length - 1)) : "";
                var blobPath = subdir != "" ? $"{subdir}/{fileName}" : fileName;

                CloudBlockBlob blob = container.GetBlockBlobReference(blobPath);
                _logger.LogTrace($"Blob URI: {blob.Uri}");
                await blob.DeleteIfExistsAsync();

                _logger.LogTrace($"Uploading stream...");
                await blob.UploadFromStreamAsync(data);
                _logger.LogTrace($"Upload complete");
            }

            return null;
        }

        public async Task<IUnixDirectoryEntry> CreateDirectoryAsync(IUnixDirectoryEntry targetDirectory, string directoryName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<IUnixFileSystemEntry>> GetEntriesAsync(IUnixDirectoryEntry directoryEntry, CancellationToken cancellationToken)
        {
            var ret = new List<IUnixFileSystemEntry>();

            //handle container listing
            if (directoryEntry.IsRoot == true)
            {
                var containers = _blobClient.ListContainersSegmentedAsync(null).Result;
                ret.AddRange(containers.Results.Select(c => new AzureStorageDirectoryEntry(c)));
            }
            //handle a url inside a container
            else
            {
                var split = directoryEntry.Name.Split('/').Where(s => string.IsNullOrEmpty(s) == false).ToArray();
                if (split != null && split.Length > 0)
                {
                    var container = _blobClient.GetContainerReference(split.FirstOrDefault());
                    var subdir = split.Length > 1 ? string.Join("/", split.Skip(1).Take(split.Length - 1)) : "";

                    var directoryReference = container.GetDirectoryReference(subdir);

                    //get blobs in dir
                    var blobs = await directoryReference.ListBlobsSegmentedAsync(false, BlobListingDetails.Copy, 10000, null, null, null);
                    ret.AddRange(blobs.Results.Where(b => b.GetType() == typeof(CloudBlobDirectory)).Select(b => new AzureStorageDirectoryEntry(b as CloudBlobDirectory)));
                    ret.AddRange(blobs.Results.Where(b => b.GetType() != typeof(CloudBlobDirectory)).Select(b => new AzureStorageFileEntry(b as CloudBlockBlob)));
                }
            }

            return ret;
        }

        public async Task<IUnixFileSystemEntry> GetEntryByNameAsync(IUnixDirectoryEntry directoryEntry, string name, CancellationToken cancellationToken)
        {
            var dirEntry = directoryEntry as AzureStorageDirectoryEntry;

            if (directoryEntry.IsRoot == true)
            {
                if (string.IsNullOrEmpty(name) == true) return Root;
                else
                {
                    var container = _blobClient.GetContainerReference(name);
                    var directoryReference = new AzureStorageDirectoryEntry(container);
                    return directoryReference;

                }
            }
            else
            {
                var split = dirEntry.Name.Split('/').Where(s => string.IsNullOrEmpty(s) == false).ToArray();
                if (split != null && split.Length > 0)
                {
                    var container = _blobClient.GetContainerReference(split.FirstOrDefault());
                    var subdir = split.Length > 1 ? string.Join("/", split.Skip(1).Take(split.Length - 1)) : "";

                    var directoryReference = container.GetDirectoryReference(subdir);

                    //get blob
                    var blob = directoryReference.GetBlockBlobReference(name);
                    var ret = new AzureStorageFileEntry(blob);
                    ret.Directory = dirEntry;
                    return ret;
                }
            }

            return null;
        }

        public async Task<IUnixFileSystemEntry> MoveAsync(IUnixDirectoryEntry parent, IUnixFileSystemEntry source, IUnixDirectoryEntry target, string fileName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<Stream> OpenReadAsync(IUnixFileEntry fileEntry, long startPosition, CancellationToken cancellationToken)
        {
            var file = fileEntry as AzureStorageFileEntry;

            var policy = new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTimeOffset.Now.AddHours(1),
                SharedAccessStartTime = DateTimeOffset.Now.AddSeconds(-5)
            };

            var sas = file.CloudBlob.GetSharedAccessSignature(policy);
            var url = $"{file.CloudBlob.Uri}{sas}";
            using (var httpClient = new HttpClient())
            {
                // we use the HttpClient to download the file so that we can pass the stream directly to the client
                // the blob storage library forces you to pull the whole stream down first
                var ret = await httpClient.GetStreamAsync(url);
                if (startPosition > 0) ret.Seek(startPosition, SeekOrigin.Begin);
                return ret;
            }
        }

        public async Task<IBackgroundTransfer> ReplaceAsync(IUnixFileEntry fileEntry, Stream data, CancellationToken cancellationToken)
        {
            var file = fileEntry as AzureStorageFileEntry;
            var ret = await CreateAsync(file.Directory, file.Name, data, cancellationToken);
            return ret;
        }

        public async Task<IUnixFileSystemEntry> SetMacTimeAsync(IUnixFileSystemEntry entry, DateTimeOffset? modify, DateTimeOffset? access, DateTimeOffset? create, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task UnlinkAsync(IUnixFileSystemEntry entry, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {
            _blobClient = null;
            _storageAccount = null;
        }
    }
}

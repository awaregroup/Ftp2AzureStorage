using FubarDev.FtpServer.FileSystem;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Text;

namespace AwareGroup.FtpServer.FileSystem.AzureStorage
{
    public class AzureStorageFileEntry : IUnixFileEntry
    {
        public AzureStorageFileEntry(ICloudBlob cloudBlob)
        {
            CloudBlob = cloudBlob;
        }
        public ICloudBlob CloudBlob { get; private set; }

        public AzureStorageDirectoryEntry Directory { get; set; }

        public long Size => CloudBlob.Properties.Length;

        public string Name => CloudBlob.Name;

        public IUnixPermissions Permissions => Common.GenericAllowPermissions;

        public DateTimeOffset? LastWriteTime => CloudBlob.Properties.BlobTierLastModifiedTime;

        public DateTimeOffset? CreatedTime => CloudBlob.Properties.Created;

        public long NumberOfLinks => 1;

        public string Owner => Common.GenericOwner;

        public string Group => Common.GenericGroup;
    }
}

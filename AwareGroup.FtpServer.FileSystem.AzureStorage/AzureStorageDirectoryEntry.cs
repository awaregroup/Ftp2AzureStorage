using FubarDev.FtpServer.FileSystem;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Text;

namespace AwareGroup.FtpServer.FileSystem.AzureStorage
{
    public class AzureStorageDirectoryEntry : IUnixDirectoryEntry
    {
        private CloudBlobDirectory _directory;
        private CloudBlobContainer _container;

        public AzureStorageDirectoryEntry(CloudBlobDirectory directory)
        {
            _directory = directory;
            IsDeletable = true;

            Name = $"{directory.Prefix}";
            if (Name.Length == 1 && Name[0] == '/') IsRoot = true;
            else if (Name[Name.Length - 1] == '/') { Name = Name.Substring(0, Name.Length - 1); }

            LastWriteTime = DateTimeOffset.MinValue;
            CreatedTime = DateTimeOffset.MinValue;
            NumberOfLinks = 1;
        }

        public AzureStorageDirectoryEntry(CloudBlobContainer container)
        {
            _container = container;
            IsDeletable = false;
            Name = $"{container.Name}";
            if (Name.Length == 1 && Name[0] == '/') IsRoot = true;
            LastWriteTime = container.Properties.LastModified;
            CreatedTime = DateTimeOffset.MinValue;
            NumberOfLinks = 1;
        }

        public AzureStorageDirectoryEntry(string name, int numberOfLinks, bool isRoot = false)
        {
            IsRoot = isRoot;
            IsDeletable = false;
            Name = name;
            LastWriteTime = DateTimeOffset.MinValue;
            CreatedTime = DateTimeOffset.MinValue;
            NumberOfLinks = numberOfLinks;

            if (Name.Length == 1 && Name[0] == '/') IsRoot = true;
        }


        /// <summary>
        /// We treat the container enumeration as the base directory
        /// </summary>
        public bool IsRoot { get; private set; }

        public bool IsDeletable { get; private set; }

        public string Name { get; private set; }

        public IUnixPermissions Permissions { get; private set; } = Common.GenericAllowPermissions;

        public DateTimeOffset? LastWriteTime { get; private set; }

        public DateTimeOffset? CreatedTime { get; private set; }

        public long NumberOfLinks { get; private set; }

        public string Owner => Common.GenericOwner;

        public string Group => Common.GenericGroup;
    }
}

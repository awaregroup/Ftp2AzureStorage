using FubarDev.FtpServer.AccountManagement;
using System;
using System.Collections.Generic;
using System.Text;

namespace AwareGroup.FtpServer.FileSystem.AzureStorage
{
    public class AzureStorageFtpUser : IFtpUser

    {
        public string Name => AccountName;
        public string AccountName { get; set; }
        public string AccountKey { get; set; }

        public bool IsInGroup(string groupName) => true;
    }
}

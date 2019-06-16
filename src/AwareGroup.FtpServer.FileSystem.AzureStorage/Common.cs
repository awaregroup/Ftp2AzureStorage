using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.FileSystem.Generic;
using System;
using System.Collections.Generic;
using System.Text;

namespace AwareGroup.FtpServer.FileSystem.AzureStorage
{
    internal static class Common
    {
        public static IUnixPermissions GenericAllowPermissions = new GenericUnixPermissions(new GenericAccessMode(true, true, true), new GenericAccessMode(true, true, true), new GenericAccessMode(true, true, true));
        public static IUnixPermissions GenericDenyPermissions = new GenericUnixPermissions(new GenericAccessMode(true, true, true), new GenericAccessMode(true, true, true), new GenericAccessMode(true, true, true));

        public static string GenericOwner = "AZURE";
        public static string GenericGroup = "AZURE";
    }
}

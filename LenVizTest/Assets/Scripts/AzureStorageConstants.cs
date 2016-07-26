using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzureUtil
{
    public static class AzureStorageConstants
    {
        public static string KeyString = "<>";
        public static string Account = "deftgeneralstorage";
        //public static string BlobEndPoint = "http://deftgeneralstorage.blob.core.windows.net/";
        public static string SharedKeyAuthorizationScheme = "SharedKey";
        //public static byte[] Key = Convert.FromBase64String(AzureStorageConstants.KeyString);
        public static string container = "holograph";
    }
}

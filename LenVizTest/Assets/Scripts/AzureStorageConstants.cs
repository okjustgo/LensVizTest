using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzureUtil
{
    public static class AzureStorageConstants
    {
        public static readonly string KeyString = "<>";
        public static readonly string Account = "deftgeneralstorage";
        public static readonly string BlobEndPoint = "http://deftgeneralstorage.blob.core.windows.net/";
        public static readonly string SharedKeyAuthorizationScheme = "SharedKey";
        public static readonly byte[] Key = Convert.FromBase64String(AzureStorageConstants.KeyString);
        public static readonly string container = "holograph";
    }
}

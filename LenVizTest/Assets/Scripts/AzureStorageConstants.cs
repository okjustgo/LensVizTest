using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzureUtil
{
    public static class AzureStorageConstants
    {
        public static readonly string KeyString = "q4A+xqucu+NFyDlgQQ5s4Gg0rM5Jk5g80CmiroP/vJB2r1UhJojxc4cdx3d5g9l97ICNkt7io05ihxWoGWa1Kw==";
        public static readonly string Account = "deftgeneralstorage";
        public static readonly string BlobEndPoint = "http://deftgeneralstorage.blob.core.windows.net/";
        public static readonly string SharedKeyAuthorizationScheme = "SharedKey";
        public static readonly byte[] Key = Convert.FromBase64String(AzureStorageConstants.KeyString);
        public static readonly string container = "holograph";
    }
}

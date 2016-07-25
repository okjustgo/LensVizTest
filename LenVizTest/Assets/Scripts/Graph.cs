using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Threading;

public class Sha256
{
    private static readonly UInt32[] K = new UInt32[64] {
            0x428A2F98, 0x71374491, 0xB5C0FBCF, 0xE9B5DBA5, 0x3956C25B, 0x59F111F1, 0x923F82A4, 0xAB1C5ED5,
            0xD807AA98, 0x12835B01, 0x243185BE, 0x550C7DC3, 0x72BE5D74, 0x80DEB1FE, 0x9BDC06A7, 0xC19BF174,
            0xE49B69C1, 0xEFBE4786, 0x0FC19DC6, 0x240CA1CC, 0x2DE92C6F, 0x4A7484AA, 0x5CB0A9DC, 0x76F988DA,
            0x983E5152, 0xA831C66D, 0xB00327C8, 0xBF597FC7, 0xC6E00BF3, 0xD5A79147, 0x06CA6351, 0x14292967,
            0x27B70A85, 0x2E1B2138, 0x4D2C6DFC, 0x53380D13, 0x650A7354, 0x766A0ABB, 0x81C2C92E, 0x92722C85,
            0xA2BFE8A1, 0xA81A664B, 0xC24B8B70, 0xC76C51A3, 0xD192E819, 0xD6990624, 0xF40E3585, 0x106AA070,
            0x19A4C116, 0x1E376C08, 0x2748774C, 0x34B0BCB5, 0x391C0CB3, 0x4ED8AA4A, 0x5B9CCA4F, 0x682E6FF3,
            0x748F82EE, 0x78A5636F, 0x84C87814, 0x8CC70208, 0x90BEFFFA, 0xA4506CEB, 0xBEF9A3F7, 0xC67178F2
        };

    private static UInt32 ROTL(UInt32 x, byte n)
    {
        Debug.Assert(n < 32);
        return (x << n) | (x >> (32 - n));
    }

    private static UInt32 ROTR(UInt32 x, byte n)
    {
        Debug.Assert(n < 32);
        return (x >> n) | (x << (32 - n));
    }

    private static UInt32 Ch(UInt32 x, UInt32 y, UInt32 z)
    {
        return (x & y) ^ ((~x) & z);
    }

    private static UInt32 Maj(UInt32 x, UInt32 y, UInt32 z)
    {
        return (x & y) ^ (x & z) ^ (y & z);
    }

    private static UInt32 Sigma0(UInt32 x)
    {
        return ROTR(x, 2) ^ ROTR(x, 13) ^ ROTR(x, 22);
    }

    private static UInt32 Sigma1(UInt32 x)
    {
        return ROTR(x, 6) ^ ROTR(x, 11) ^ ROTR(x, 25);
    }

    private static UInt32 sigma0(UInt32 x)
    {
        return ROTR(x, 7) ^ ROTR(x, 18) ^ (x >> 3);
    }

    private static UInt32 sigma1(UInt32 x)
    {
        return ROTR(x, 17) ^ ROTR(x, 19) ^ (x >> 10);
    }


    private UInt32[] H = new UInt32[8] {
            0x6A09E667, 0xBB67AE85, 0x3C6EF372, 0xA54FF53A, 0x510E527F, 0x9B05688C, 0x1F83D9AB, 0x5BE0CD19
        };

    private byte[] pending_block = new byte[64];
    private uint pending_block_off = 0;
    private UInt32[] uint_buffer = new UInt32[16];

    private UInt64 bits_processed = 0;

    private bool closed = false;

    private void processBlock(UInt32[] M)
    {
        Debug.Assert(M.Length == 16);

        // 1. Prepare the message schedule (W[t]):
        UInt32[] W = new UInt32[64];
        for (int t = 0; t < 16; ++t)
        {
            W[t] = M[t];
        }

        for (int t = 16; t < 64; ++t)
        {
            W[t] = sigma1(W[t - 2]) + W[t - 7] + sigma0(W[t - 15]) + W[t - 16];
        }

        // 2. Initialize the eight working variables with the (i-1)-st hash value:
        UInt32 a = H[0],
               b = H[1],
               c = H[2],
               d = H[3],
               e = H[4],
               f = H[5],
               g = H[6],
               h = H[7];

        // 3. For t=0 to 63:
        for (int t = 0; t < 64; ++t)
        {
            UInt32 T1 = h + Sigma1(e) + Ch(e, f, g) + K[t] + W[t];
            UInt32 T2 = Sigma0(a) + Maj(a, b, c);
            h = g;
            g = f;
            f = e;
            e = d + T1;
            d = c;
            c = b;
            b = a;
            a = T1 + T2;
        }

        // 4. Compute the intermediate hash value H:
        H[0] = a + H[0];
        H[1] = b + H[1];
        H[2] = c + H[2];
        H[3] = d + H[3];
        H[4] = e + H[4];
        H[5] = f + H[5];
        H[6] = g + H[6];
        H[7] = h + H[7];
    }

    public void AddData(byte[] data, uint offset, uint len)
    {
        if (closed)
            throw new InvalidOperationException("Adding data to a closed hasher.");

        if (len == 0)
            return;

        bits_processed += len * 8;

        while (len > 0)
        {
            uint amount_to_copy;

            if (len < 64)
            {
                if (pending_block_off + len > 64)
                    amount_to_copy = 64 - pending_block_off;
                else
                    amount_to_copy = len;
            }
            else
            {
                amount_to_copy = 64 - pending_block_off;
            }

            Array.Copy(data, (int)offset, pending_block, (int)pending_block_off, (int)amount_to_copy);
            len -= amount_to_copy;
            offset += amount_to_copy;
            pending_block_off += amount_to_copy;

            if (pending_block_off == 64)
            {
                toUintArray(pending_block, uint_buffer);
                processBlock(uint_buffer);
                pending_block_off = 0;
            }
        }
    }

    public byte[] GetHash()
    {
        return toByteArray(GetHashUInt32());
    }

    public UInt32[] GetHashUInt32()
    {
        if (!closed)
        {
            UInt64 size_temp = bits_processed;

            AddData(new byte[1] { 0x80 }, 0, 1);

            uint available_space = 64 - pending_block_off;

            if (available_space < 8)
                available_space += 64;

            // 0-initialized
            byte[] padding = new byte[available_space];
            // Insert lenght uint64
            for (uint i = 1; i <= 8; ++i)
            {
                padding[padding.Length - i] = (byte)size_temp;
                size_temp >>= 8;
            }

            AddData(padding, 0u, (uint)padding.Length);

            Debug.Assert(pending_block_off == 0);

            closed = true;
        }

        var ret = new UInt32[H.Length];
        for (int i = 0; i < H.Length; i++)
        {
            ret[i] = H[i];
        }

        return ret;
    }

    private static void toUintArray(byte[] src, UInt32[] dest)
    {
        for (uint i = 0, j = 0; i < dest.Length; ++i, j += 4)
        {
            dest[i] = ((UInt32)src[j + 0] << 24) | ((UInt32)src[j + 1] << 16) | ((UInt32)src[j + 2] << 8) | ((UInt32)src[j + 3]);
        }
    }

    private static byte[] toByteArray(UInt32[] src)
    {
        byte[] dest = new byte[src.Length * 4];
        int pos = 0;

        for (int i = 0; i < src.Length; ++i)
        {
            dest[pos++] = (byte)(src[i] >> 24);
            dest[pos++] = (byte)(src[i] >> 16);
            dest[pos++] = (byte)(src[i] >> 8);
            dest[pos++] = (byte)(src[i]);
        }

        return dest;
    }

    public static byte[] HashFile(Stream fs)
    {
        Sha256 sha = new Sha256();
        byte[] buf = new byte[8196];

        uint bytes_read;
        do
        {
            bytes_read = (uint)fs.Read(buf, 0, buf.Length);
            if (bytes_read == 0)
                break;

            sha.AddData(buf, 0, bytes_read);
        }
        while (bytes_read == 8196);

        return sha.GetHash();
    }
}

class AzureHelper
{
    protected bool IsTableStorage { get; set; }

    private string endpoint;
    public string Endpoint
    {
        get
        {
            return endpoint;
        }
        internal set
        {
            endpoint = value;
        }
    }

    private string storageAccount;
    public string StorageAccount
    {
        get
        {
            return storageAccount;
        }
        internal set
        {
            storageAccount = value;
        }
    }

    private string storageKey;
    public string StorageKey
    {
        get
        {
            return storageKey;
        }
        internal set
        {
            storageKey = value;
        }
    }


    public AzureHelper(string storageAccount, string storageKey)
    {
        this.Endpoint = "http://" + storageAccount + ".blob.core.windows.net/";
        this.StorageAccount = storageAccount;
        this.StorageKey = storageKey;
    }

    public HttpWebRequest CreateGetBlobRequest(string container, string blob)
    {
        return CreateRESTRequest("GET", container + "/" + blob);
    }

    #region REST HTTP Request Helper Methods

    // Construct and issue a REST request and return the response.

    public HttpWebRequest CreateRESTRequest(string method, string resource, string requestBody = null, SortedList<string, string> headers = null,
        string ifMatch = "", string md5 = "")
    {
        DateTime now = DateTime.UtcNow;
        string uri = Endpoint + resource;

        HttpWebRequest request = HttpWebRequest.Create(uri) as HttpWebRequest;
        request.Method = method;
        request.Headers["x-ms-date"] = now.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
        request.Headers["x-ms-version"] = "2009-09-19";

        if (headers != null)
        {
            foreach (KeyValuePair<string, string> header in headers)
            {
                request.Headers[header.Key] = header.Value;
            }
        }

        request.Headers["Authorization"] = AuthorizationHeader(method, now, request, ifMatch, md5);

        return request;
    }


    // Generate an authorization header.

    public string AuthorizationHeader(string method, DateTime now, HttpWebRequest request, string ifMatch = "", string md5 = "")
    {
        string MessageSignature;

        if (IsTableStorage)
        {
            MessageSignature = String.Format("{0}\n\n{1}\n{2}\n{3}",
                method,
                "application/atom+xml",
                now.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
                GetCanonicalizedResource(request.RequestUri, StorageAccount)
                );
        }
        else
        {
            MessageSignature = String.Format("{0}\n\n\n{1}\n{5}\n\n\n\n{2}\n\n\n\n{3}{4}",
                method,
                string.Empty,//(method == "GET" || method == "HEAD") ? String.Empty : request.ContentLength.ToString(),
                ifMatch,
                GetCanonicalizedHeaders(request),
                GetCanonicalizedResource(request.RequestUri, StorageAccount),
                md5
                );
        }
        byte[] SignatureBytes = System.Text.Encoding.UTF8.GetBytes(MessageSignature);
        //System.Security.Cryptography.HMACSHA256 SHA256 = new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(StorageKey));
        //String AuthorizationHeader = "SharedKey " + StorageAccount + ":" + Convert.ToBase64String(SHA256.ComputeHash(SignatureBytes));
        var hash = HmacSha256(Convert.FromBase64String(StorageKey), SignatureBytes);
        String AuthorizationHeader = "SharedKey " + StorageAccount + ":" + Convert.ToBase64String(hash);
        return AuthorizationHeader;
    }

    // http://stackoverflow.com/questions/21498696/try-to-code-hmac-sha256-using-c-net
    public static byte[] HmacSha256(byte[] key, byte[] message)
    {
        var encoding = Encoding.UTF8;
        const int hashBlockSize = 64;

        var opadKeySet = new byte[hashBlockSize];
        var ipadKeySet = new byte[hashBlockSize];

        if (key.Length > hashBlockSize)
        {
            key = GetHash(key);
        }

        if (key.Length < hashBlockSize)
        {
            var newKeyBytes = new byte[hashBlockSize];
            key.CopyTo(newKeyBytes, 0);
            key = newKeyBytes;
        }

        for (int i = 0; i < key.Length; i++)
        {
            opadKeySet[i] = (byte)(key[i] ^ 0x5C);
            ipadKeySet[i] = (byte)(key[i] ^ 0x36);
        }

        //var hash = GetHash(ByteConcat(opadKeySet, GetHash(ByteConcat(ipadKeySet, message))));
        Sha256 sha1 = new Sha256();
        Sha256 sha2 = new Sha256();
        sha1.AddData(ByteConcat(ipadKeySet, message), 0, (uint)(ByteConcat(ipadKeySet, message)).Length);
        var subhash = sha1.GetHash();
        sha2.AddData(ByteConcat(opadKeySet, subhash), 0, (uint)(ByteConcat(opadKeySet, subhash)).Length);
        var hash = sha2.GetHash();

        return hash;
    }


    public static byte[] GetHash(byte[] bytes)
    {
        using (var hash = System.Security.Cryptography.HashAlgorithm.Create("sha256"))
        {
            return hash.ComputeHash(bytes);
        }
    }

    public static byte[] ByteConcat(byte[] left, byte[] right)
    {
        if (null == left)
        {
            return right;
        }

        if (null == right)
        {
            return left;
        }

        byte[] newBytes = new byte[left.Length + right.Length];
        left.CopyTo(newBytes, 0);
        right.CopyTo(newBytes, left.Length);

        return newBytes;
    }

    // Get canonicalized headers.
    public string GetCanonicalizedHeaders(HttpWebRequest request)
    {
        ArrayList headerNameList = new ArrayList();
        StringBuilder sb = new StringBuilder();
        foreach (string headerName in request.Headers.AllKeys)
        {
            if (headerName.ToLowerInvariant().StartsWith("x-ms-", StringComparison.Ordinal))
            {
                headerNameList.Add(headerName.ToLowerInvariant());
            }
        }
        headerNameList.Sort();
        foreach (string headerName in headerNameList)
        {
            StringBuilder builder = new StringBuilder(headerName);
            string separator = ":";
            foreach (string headerValue in GetHeaderValues(request.Headers, headerName))
            {
                string trimmedValue = headerValue.Replace("\r\n", String.Empty);
                builder.Append(separator);
                builder.Append(trimmedValue);
                separator = ",";
            }
            sb.Append(builder.ToString());
            sb.Append("\n");
        }
        return sb.ToString();
    }

    // Get header values.

    public ArrayList GetHeaderValues(WebHeaderCollection headers, string headerName)
    {
        ArrayList list = new ArrayList() { headers[headerName] };
        //string[] values = headers.GetValues(headerName);
        //if (values != null)
        //{
        //    foreach (string str in values)
        //    {
        //        list.Add(str.TrimStart(null));
        //    }
        //}
        return list;
    }

    // Get canonicalized resource.

    public string GetCanonicalizedResource(Uri address, string accountName)
    {
        StringBuilder str = new StringBuilder();
        StringBuilder builder = new StringBuilder("/");
        builder.Append(accountName);
        builder.Append(address.AbsolutePath);
        str.Append(builder.ToString());
        NameValueCollection values2 = new NameValueCollection();
        ArrayList list2 = new ArrayList(values2.AllKeys);
        list2.Sort();
        foreach (string str3 in list2)
        {
            StringBuilder builder3 = new StringBuilder(string.Empty);
            builder3.Append(str3);
            builder3.Append(":");
            builder3.Append(values2[str3]);
            str.Append("\n");
            str.Append(builder3.ToString());
        }
        return str.ToString();
    }

    #endregion
}

public class RequestState
{
    // This class stores the State of the request.
    public int BUFFER_SIZE = 1024;
    public StringBuilder requestData;
    public byte[] BufferRead;
    public HttpWebRequest request;
    public HttpWebResponse response;
    public Stream streamResponse;
    public ManualResetEvent isThisDone;
    public RequestState()
    {
        BufferRead = new byte[BUFFER_SIZE];
        requestData = new StringBuilder("");
        request = null;
        streamResponse = null;
        isThisDone = new ManualResetEvent(false);
    }
}


public class Graph : MonoBehaviour {

    private float[,] data;

    // Use this for initialization
    void Start () {
        var whatToRender = "radartube";
        if(whatToRender == "scatterplot")
        {
            GetDataFromAzure("holograph", "irisData2.hgd");

            var x = new float[data.GetLength(0)];
            var y = new float[data.GetLength(0)];
            var z = new float[data.GetLength(0)];
            var series = new float[data.GetLength(0)];
            for (int i = 0; i < data.GetLength(0); i++)
            {
                x[i] = data[i, 1];
                y[i] = data[i, 2];
                z[i] = data[i, 3];
                series[i] = data[i, 0];
            }
            ScatterPlot.Render(gameObject, x, y, z, series);
        }
        if (whatToRender == "barplot")
        {
            int m = 50;
            int n = 50;
            var x = new float[n * m];
            var y = new float[n * m];
            var z = new float[n * m];
            
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    x[i + n * j] = i;
                    y[i + n * j] = j;
                    var X = 7f * (float)((2.0 * i - n) / n);
                    var Y = 7f * (float)((2.0 * j - m) / m);
                    z[i + n * j] = 0.5f * (float)(Math.Sin(X + Y) + Math.Sin(X - Y));
                }
            }

            BarGraph.Render(gameObject, x, y, z);
        }
        if (whatToRender == "surface")
        {
            int m = 50;
            int n = 50;
            var x = new float[n * m];
            var y = new float[n * m];
            var z = new float[n * m];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    x[i + n * j] = i;
                    y[i + n * j] = j;
                    var X = 7f * (float)((2.0 * i - n) / n);
                    var Y = 7f * (float)((2.0 * j - m) / m);
                    z[i + n * j] = 0.5f * (float)(Math.Sin(X + Y) + Math.Sin(X - Y));
                }
            }

            SurfaceChart.Render(gameObject, x, y, z);
        }
        if (whatToRender == "radartube")
        {
            // From Kaggle bikeshare competition.
            // Schema is: "year + month","temp","humidity","windspeed","casual","registered"
            // For each year/month (from Jan 2011 - Dec 2012) gives median value for each other variable.
            data = new float[,]
            {
                { 0f, 0.183673469387755f, 0.51f, 0.228047490302104f, 2f, 43f },
                { 1f, 0.224489795918367f, 0.495f, 0.263195015869284f, 4f, 52.5f },
                { 2f, 0.326530612244898f, 0.565f, 0.263195015869284f, 9f, 56f },
                { 3f, 0.408163265306122f, 0.67f, 0.263195015869284f, 12f, 65f },
                { 4f, 0.510204081632653f, 0.77f, 0.228047490302104f, 27f, 120f },
                { 5f, 0.673469387755102f, 0.58f, 0.193017514987657f, 32f, 134.5f },
                { 6f, 0.714285714285714f, 0.61f, 0.193017514987657f, 39f, 123.5f },
                { 7f, 0.714285714285714f, 0.62f, 0.228047490302104f, 32f, 115.5f },
                { 8f, 0.612244897959184f, 0.73f, 0.193017514987657f, 23f, 115f },
                { 9f, 0.510204081632653f, 0.76f, 0.157869989420477f, 20f, 115f },
                { 10f, 0.36734693877551f, 0.63f, 0.193017514987657f, 12f, 114.5f },
                { 11f, 0.285714285714286f, 0.625f, 0.193017514987657f, 7f, 101f },
                { 12f, 0.244897959183673f, 0.51f, 0.263195015869284f, 6f, 88f },
                { 13f, 0.285714285714286f, 0.56f, 0.193017514987657f, 6f, 107f },
                { 14f, 0.428571428571429f, 0.55f, 0.228047490302104f, 16f, 144f },
                { 15f, 0.469387755102041f, 0.43f, 0.263195015869284f, 34f, 165f },
                { 16f, 0.571428571428572f, 0.675f, 0.193017514987657f, 37f, 181.5f },
                { 17f, 0.612244897959184f, 0.585f, 0.228047490302104f, 45f, 193f },
                { 18f, 0.755102040816326f, 0.555f, 0.157869989420477f, 49f, 182f },
                { 19f, 0.714285714285714f, 0.66f, 0.193017514987657f, 53f, 183.5f },
                { 20f, 0.653061224489796f, 0.7f, 0.193017514987657f, 34f, 190f },
                { 21f, 0.510204081632653f, 0.69f, 0.193017514987657f, 27f, 197f },
                { 22f, 0.326530612244898f, 0.56f, 0.228047490302104f, 17f, 174f },
                { 23f, 0.346938775510204f, 0.75f, 0.157869989420477f, 13f, 169f }
            };

            var t = new float[data.GetLength(0)];
            var R = new float[data.GetLength(0), data.GetLength(1) - 1];
            for(int i = 0; i < data.GetLength(0); i++)
            {
                t[i] = data[i,0];
                for(int j = 1; j < data.GetLength(1); j++)
                {
                    R[i, j - 1] = data[i, j];
                }
            }

            RadarTube.Render(gameObject, t, R);
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void GetDataFromAzure(string containerName, string blobName)
    {
        var blobHelper = new AzureHelper("deftgeneralstorage", "<>");
        var request = blobHelper.CreateGetBlobRequest(containerName, blobName);

        var requestState = new RequestState();
        requestState.request = request;

        IAsyncResult result = (IAsyncResult)request.BeginGetResponse(new AsyncCallback(ResponseCallback), requestState);

        WaitHandle.WaitAll(new[] { requestState.isThisDone }, 60 * 1000);
    }

    private void ResponseCallback(IAsyncResult result)
    {
        try
        {
            var requestState = (RequestState)result.AsyncState;
            var request = requestState.request;
            requestState.response = (HttpWebResponse)request.EndGetResponse(result);

            Stream responseStream = requestState.response.GetResponseStream();

            var sb = new StringBuilder();
            var responseLength = responseStream.Read(requestState.BufferRead, 0, requestState.BUFFER_SIZE);
            while (responseLength > 0)
            {
                sb.Append(Encoding.UTF8.GetString(requestState.BufferRead, 0, responseLength));
                responseLength = responseStream.Read(requestState.BufferRead, 0, requestState.BUFFER_SIZE);
            }

            var stream = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString())));

            string firstLine = stream.ReadLine();
            if (firstLine == null)
            {
                Debug.Log("Data file was empty");
                throw new Exception("Data file was empty");
            }

            var s = firstLine.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (s.Length != 2)
            {
                Debug.Log("Couldn't read data size");
                throw new Exception("Couldn't read data size");
            }
            int nrows = int.Parse(s[0]);
            int ncols = int.Parse(s[1]);
            data = new float[nrows, ncols];

            string line;
            for (int i = 0; i < nrows; i++)
            {
                line = stream.ReadLine();
                s = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (s.Length != ncols)
                {
                    Debug.Log("Wrong number of columns");
                    throw new Exception("Wrong number of columns");
                }

                for (int j = 0; j < ncols; j++)
                {
                    data[i, j] = float.Parse(s[j]);
                }
            }

            requestState.isThisDone.Set();

            return;
        }
        catch (WebException e)
        {
            Debug.Log("\nRespCallback Exception raised!");
            Debug.Log(string.Format("\nMessage:{0}", e.Message));
            Debug.Log(string.Format("\nStatus:{0}", e.Status));
            throw;
        }
    }
}

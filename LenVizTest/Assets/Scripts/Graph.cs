using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using UnityEngine.UI;
using AzureUtil;
using HoloGraph;

internal static class ExtensionMethods
{
    internal static T[][] ToJaggedArray<T>(this T[,] twoDimensionalArray)
    {
        int rowsFirstIndex = twoDimensionalArray.GetLowerBound(0);
        int rowsLastIndex = twoDimensionalArray.GetUpperBound(0);
        int numberOfRows = rowsLastIndex + 1;

        int columnsFirstIndex = twoDimensionalArray.GetLowerBound(1);
        int columnsLastIndex = twoDimensionalArray.GetUpperBound(1);
        int numberOfColumns = columnsLastIndex + 1;

        T[][] jaggedArray = new T[numberOfRows][];
        for (int i = rowsFirstIndex; i <= rowsLastIndex; i++)
        {
            jaggedArray[i] = new T[numberOfColumns];

            for (int j = columnsFirstIndex; j <= columnsLastIndex; j++)
            {
                jaggedArray[i][j] = twoDimensionalArray[i, j];
            }
        }
        return jaggedArray;
    }

    internal static T[,] To2D<T>(this T[][] source)
    {
        try
        {
            int FirstDim = source.Length;
            int SecondDim = source.GroupBy(row => row.Length).Single().Key; // throws InvalidOperationException if source is not rectangular

            var result = new T[FirstDim, SecondDim];
            for (int i = 0; i < FirstDim; ++i)
                for (int j = 0; j < SecondDim; ++j)
                    result[i, j] = source[i][j];

            return result;
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("The given jagged array is not rectangular.");
        }
    }
}

public class Graph : MonoBehaviour {

    public enum PivotAxis
    {
        // Rotate about all axes.
        Free,
        // Rotate about an individual axis.
        X,
        Y
    }

    public string datasetToRender;
    private bool needToGetData = true;
    private bool shouldRender = false;
    private bool hadError = false;
    private int errorCode = 0;
    private string errorMsg = "";
    private float[,] data;
    private HoloGraphData hgd;
    private Text title;
    private Text xAxis;
    private Text yAxis;
    private Text zAxis;
    private GameObject tooltipPrefab, tooltip, msgObj, statusObj;

    private Quaternion prevRotation;
    private Vector3 prevPosition;
    
    // The axis about which the object will rotate.
    private PivotAxis pivotAxis = PivotAxis.Free;

    // Overrides the cached value of the title's default rotation.
    public Quaternion titleDefaultRotation { get; private set; }
    
    public static GameObject createGraph(string dataset)
    {
        var graphPrefab = Resources.Load(@"Graph", typeof(GameObject)) as GameObject;
        var graph = Instantiate(graphPrefab);
        graph.SendMessage("OnSelect");
        graph.GetComponent<Graph>().datasetToRender = dataset;
        return graph;
        //graph.transform.parent = gameObject.transform;
    }
    void Destroy()
    {
        Destroy(this.gameObject);
    }

    void Rotate()
    {
        var transform = this.gameObject.transform;
        transform.Rotate(transform.rotation.x, transform.rotation.y + 90, transform.rotation.z);    
    }

    void Awake()
    {
        title = this.gameObject.GetComponentInChildren<Transform>().Find("Canvas/GraphTitle").gameObject.GetComponent<Text>();
        xAxis = this.gameObject.GetComponentInChildren<Transform>().Find("Canvas/XAxis").gameObject.GetComponent<Text>();
        yAxis = this.gameObject.GetComponentInChildren<Transform>().Find("Canvas/YAxis").gameObject.GetComponent<Text>();
        zAxis = this.gameObject.GetComponentInChildren<Transform>().Find("Canvas/ZAxis").gameObject.GetComponent<Text>();

        tooltipPrefab = Resources.Load(@"Tooltip", typeof(GameObject)) as GameObject;
        tooltip = Instantiate(tooltipPrefab);
        tooltip.transform.parent = this.gameObject.GetComponentInChildren<Transform>().Find("Canvas");

        msgObj = Instantiate(Resources.Load(@"Tooltip", typeof(GameObject)) as GameObject);
        msgObj.transform.parent = this.gameObject.GetComponentInChildren<Transform>().Find("Canvas");
        msgObj.transform.localPosition = new Vector3(0f, -2f, -5f);
        msgObj.transform.rotation = this.gameObject.transform.rotation;
        msgObj.GetComponent<Text>().enabled = false;

        statusObj = Instantiate(Resources.Load(@"Tooltip", typeof(GameObject)) as GameObject);
        statusObj.transform.parent = this.gameObject.GetComponentInChildren<Transform>().Find("Canvas");
        statusObj.transform.localPosition = new Vector3(0f, 3f, -5f);
        statusObj.transform.rotation = this.gameObject.transform.rotation;
        statusObj.GetComponent<Text>().enabled = false;

        this.SetMsgText("Loading...", true, statusObj);

        // Cache the title's default rotation.
        titleDefaultRotation = title.transform.rotation;
    }

    void SetMsgText(string text, bool lg, GameObject textObj)
    {
        var errorMsgObjText = textObj.transform.GetComponent<Text>();
        errorMsgObjText.text = text;
        errorMsgObjText.fontSize = lg ? 40 : 25;
        textObj.GetComponent<Text>().enabled = true;
    }

    void ClearMsgText(GameObject textObj)
    {
        var errorMsgObjText = textObj.transform.GetComponent<Text>();
        errorMsgObjText.text = "";
        textObj.GetComponent<Text>().enabled = false;
    }

    // Use this for initialization
    void Start ()
    {
        prevRotation = transform.rotation;
        prevPosition = transform.position;
    }

    // Update is called once per frame
    void Update () {
        updateTitlePivotAxis();
        
        if (this.gameObject.GetComponent<GraphMover>().placing)
        {
            this.SetMsgText("Tap to place, or say \"remove\"", true, msgObj);
            var tooltipText = tooltip.transform.GetComponent<Text>();
            tooltipText.text = "";
            tooltipText.enabled = false;
        }
        else
        {
            this.ClearMsgText(msgObj);
            var origin = new Vector3(-0.5f, -0.5f, -5);
            RaycastHit hitInfo;
            if (Physics.Raycast(
                    Camera.main.transform.position,
                    Camera.main.transform.forward,
                    out hitInfo,
                    20.0f,
                    Physics.DefaultRaycastLayers))
            {
                // If the Raycast has succeeded and hit a hologram
                // hitInfo's point represents the position being gazed at
                // hitInfo's collider GameObject represents the hologram being gazed at
                // Requires Tooltip.prefab in Resources folder.
                tooltip.transform.localPosition = origin + new Vector3(hitInfo.point.x, hitInfo.point.y, 0);
                tooltip.transform.rotation = gameObject.transform.rotation;
                var tooltipText = tooltip.transform.GetComponent<Text>();
                tooltipText.text = hitInfo.point.ToString();
                tooltipText.enabled = true;
            }
            else
            {
                var tooltipText = tooltip.transform.GetComponent<Text>();
                tooltipText.text = "";
                tooltipText.enabled = false;
            }
        }  

        if (hadError)
        {
            hadError = false;
            this.SetMsgText("ERROR: " + errorMsg, false, statusObj);
        }

        if (needToGetData && !string.IsNullOrEmpty(datasetToRender))
        {
            Debug.Log("Getting Data From Azure");
            //GetDataFromAzure(AzureStorageConstants.container, "irisData2.hgd");            
            GetDataFromHgd(AzureStorageConstants.container, datasetToRender);
            needToGetData = false;
        }
        if (shouldRender || (hgd != null && (prevRotation != transform.rotation || prevPosition != transform.position)))
        {
            shouldRender = false;
            this.ClearMsgText(statusObj);
            renderGraph();
        }

        prevRotation = transform.rotation;
        prevPosition = transform.position;
    }

    private void renderGraph()
    {
        var geometry = hgd.Geometry;

        title.text = hgd.Title;
        xAxis.text = hgd.Aesthetics["x"];
        yAxis.text = hgd.Aesthetics["y"];
        zAxis.text = hgd.Aesthetics["z"];

        this.SetMsgText("Rendering...", true, statusObj);

        // initialize plot
        if (geometry == "point")
        {
            ScatterPlot.Render(gameObject, hgd.GetData("x"), hgd.GetData("y"), hgd.GetData("z"), hgd.GetData("color"));
        }
        if (geometry == "bar")
        {
            //int m = 50;
            //int n = 50;
            //var x = new float[n * m];
            //var y = new float[n * m];
            //var z = new float[n * m];

            //for (int i = 0; i < n; i++)
            //{
            //    for (int j = 0; j < m; j++)
            //    {
            //        x[i + n * j] = i;
            //        y[i + n * j] = j;
            //        var X = 7f * (float)((2.0 * i - n) / n);
            //        var Y = 7f * (float)((2.0 * j - m) / m);
            //        z[i + n * j] = 0.5f * (float)(Math.Sin(X + Y) + Math.Sin(X - Y));
            //    }
            //}

            BarGraph.Render(gameObject, hgd.GetData("x"), hgd.GetData("y"), hgd.GetData("z"));
        }
        if (geometry == "surface")
        {
            //title.text = "Surface Chart";
            //int m = 50;
            //int n = 50;
            //var x = new float[n * m];
            //var y = new float[n * m];
            //var z = new float[n * m];

            //for (int i = 0; i < n; i++)
            //{
            //    for (int j = 0; j < m; j++)
            //    {
            //        x[i + n * j] = i;
            //        y[i + n * j] = j;
            //        var X = 7f * (float)((2.0 * i - n) / n);
            //        var Y = 7f * (float)((2.0 * j - m) / m);
            //        z[i + n * j] = 0.5f * (float)(Math.Sin(X + Y) + Math.Sin(X - Y));
            //    }
            //}

            SurfaceChart.Render(gameObject, hgd.GetData("x"), hgd.GetData("y"), hgd.GetData("z"));
        }
        if (geometry == "radartube")
        {
            title.text = "Radar Tube";
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
            for (int i = 0; i < data.GetLength(0); i++)
            {
                t[i] = data[i, 0];
                for (int j = 1; j < data.GetLength(1); j++)
                {
                    R[i, j - 1] = data[i, j];
                }
            }

            RadarTube.Render(gameObject, t, R);
        }
        this.ClearMsgText(statusObj);
    }

    // Keeps the title facing the camera.
    private void updateTitlePivotAxis()
    {
        // Get a Vector that points from the Camera to the target.
        Vector3 directionToTarget = Camera.main.transform.position - title.transform.position;

        // If we are right next to the camera the rotation is undefined.
        if (directionToTarget.sqrMagnitude < Mathf.Epsilon)
        {
            return;
        }

        // Adjust for the pivot axis.
        switch (pivotAxis)
        {
            case PivotAxis.X:
                directionToTarget.x = title.transform.position.x;
                break;

            case PivotAxis.Y:
                directionToTarget.y = title.transform.position.y;
                break;

            case PivotAxis.Free:
            default:
                // No changes needed.
                break;
        }

        // Calculate and apply the rotation required to reorient the object and apply the default rotation to the result.
        title.transform.rotation = Quaternion.LookRotation(-directionToTarget) * titleDefaultRotation;
    
	}

    public void GetDataFromAzure(string containerName, string blobName)
    {
        try
        {
            var blobHelper = new AzureHelper(AzureStorageConstants.Account, AzureStorageConstants.KeyString);
            var request = blobHelper.CreateGetBlobRequest(containerName, blobName);

            var requestState = new RequestState();
            requestState.request = request;

            IAsyncResult result = (IAsyncResult)request.BeginGetResponse(new AsyncCallback(ResponseCallback), requestState);
        }
        catch (Exception e)
        {
            errorMsg = e.Message;
            hadError = true;
        }

        //WaitHandle.WaitAll(new[] { requestState.isThisDone }, 15 * 1000);
    }

    public void GetDataFromHgd(string containerName, string blobName)
    {
        try
        {
            var blobHelper = new AzureHelper(AzureStorageConstants.Account, AzureStorageConstants.KeyString);
            var request = blobHelper.CreateGetBlobRequest(containerName, blobName);

            var requestState = new RequestState();
            requestState.request = request;

            Debug.Log("Making Web Call");

            IAsyncResult result = (IAsyncResult)request.BeginGetResponse(new AsyncCallback(ParseHgdCallback), requestState);
        }
        catch (Exception e)
        {
            errorMsg = e.Message;
            hadError = true;
        }        
        //WaitHandle.WaitAll(new[] { requestState.isThisDone }, 15 * 1000);
    }
   

    private void ParseHgdCallback(IAsyncResult result)
    {
        try
        {
            Debug.Log("ParseHgdCallback");
            var requestState = (RequestState)result.AsyncState;
            var request = requestState.request;
            requestState.response = (HttpWebResponse)request.EndGetResponse(result);

            Stream responseStream = requestState.response.GetResponseStream();
            Stream memStream = new MemoryStream();
            byte[] buffer = new byte[32768];
            int read;
            while ((read = responseStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                memStream.Write(buffer, 0, read);
            }

            memStream.Seek(0, SeekOrigin.Begin);

            Debug.Log("Parsing data");

            hgd = new HoloGraphData(memStream);       

            requestState.isThisDone.Set();

            this.shouldRender = true;
            Debug.Log("setting should render");

            return;
        }
        catch (WebException e)
        {
            Debug.Log("\nRespCallback WebException raised!");
            Debug.Log(string.Format("\nMessage:{0}", e.Message));
            //Debug.Log(string.Format("\nStatus:{0}", e.Status));
            errorMsg = e.Message;
            hadError = true;
            //throw;
        }
        catch (SystemException e)
        {
            Debug.Log("\nRespCallback SystemException raised!");
            Debug.Log(string.Format("\nMessage:{0}", e.Message));
            //Debug.Log(string.Format("\nStatus:{0}", e.Status));
            errorMsg = e.Message;
            hadError = true;
            //throw;
        }
        catch (Exception e)
        {
            Debug.Log("\nRespCallback Exception raised!");
            Debug.Log(string.Format("\nMessage:{0}", e.Message));
            //Debug.Log(string.Format("\nStatus:{0}", e.Status));
            errorMsg = e.Message;
            hadError = true;
            //throw;
        }

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


            this.shouldRender = true;
            Debug.Log("setting should render");            

            return;
        }
        catch (WebException e)
        {
            Debug.Log("\nRespCallback Exception raised!");
            Debug.Log(string.Format("\nMessage:{0}", e.Message));
            Debug.Log(string.Format("\nStatus:{0}", e.Status));
            errorMsg = e.Message;
            hadError = true;
            //throw;
        }
    }
}

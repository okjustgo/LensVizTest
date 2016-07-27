using UnityEngine;
using System.Collections;
using UnityEngine.VR.WSA.WebCam;
using System.Collections.Generic;
using System.Linq;
//#if NETFX_CORE
using ZXing;
using ZXing.QrCode;
using ZXing.Common;
using UnityEngine.UI;
using AzureUtil;
//#endif


public class QRCodeManager : MonoBehaviour {

    public float cornerDistance; // distance between adjacent QR corners in meters
    public float verticalFOV = 48; // Vertical FOV in degrees of the physical camera, not the unity camera

    private GameObject audio = null;
    private PhotoCapture _photoCapture = null;
    private Texture2D _sourceTexture;
    private Ray _debugRay;
    private float _distance;
    private GameObject[] _markers;
//#if NETFX_CORE
    private Result qrResult;
    //#endif

    private Dictionary<string, bool> renderedQR = new Dictionary<string, bool>();

    public float colorThreshold;
    public string qrResultText { get; private set; }
    public Vector3 QRPosotion { get; private set; }


    // Use this for initialization
    void Start()
    {
        audio = GameObject.Find("Beep");
        //PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
        //InitMarkers();
    }

    void StartReading()
    {
        PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
        audio.GetComponent<AudioSource>().Play();
    }

    void StopReading()
    {
        _photoCapture.StopPhotoModeAsync(OnStoppedPhotoMode);
        audio.GetComponent<AudioSource>().Play();
    }

    private void InitMarkers()
    {
        const int markerCount = 4;
        _markers = new GameObject[markerCount];
        UnityEngine.Object markerRes = Resources.Load(@"Marker");
        for (int i = 0; i < markerCount; i++)
        {
            _markers[i] = (GameObject)GameObject.Instantiate(markerRes);
            _markers[i].GetComponent<Renderer>().material.color = (i == 3) ? Color.red : Color.green;
        }
    }

    private void OnPhotoCaptureCreated(PhotoCapture photoCapture)
    {
        _photoCapture = photoCapture;

        IEnumerable<Resolution> cameraResolutions = PhotoCapture.SupportedResolutions;
        //Resolution cameraResolution = cameraResolutions.ToArray()[15]; // 800x600 using the logicool webcam. Closest I could get to to HoloLens' 896x504 resolution
        var reso = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).ToArray();
        Resolution cameraResolution = reso.Last(); //reso[8]; //reso.Last();
        Debug.Log(string.Format("Camera Resolution: {0}x{1}", cameraResolution.width, cameraResolution.height));

        // GameObject source = GameObject.Find("Display");
        _sourceTexture = new Texture2D(cameraResolution.width, cameraResolution.height, TextureFormat.RGB24, false);
        //  source.GetComponent<Renderer>().material.mainTexture = _sourceTexture;

        CameraParameters c = new CameraParameters();
        c.hologramOpacity = 0.0f;
        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        c.pixelFormat = CapturePixelFormat.BGRA32;

        _photoCapture.StartPhotoModeAsync(c, false, OnPhotoModeStarted);
    }

    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            _photoCapture.TakePhotoAsync(OnCapturedPhotoToMemory);
        }
        else
        {
            Debug.LogError("Unable to start photo mode!");
        }
    }

    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        _photoCapture.TakePhotoAsync(OnCapturedPhotoToMemory);

        if (result.success)
        {
            photoCaptureFrame.UploadImageDataToTexture(_sourceTexture);
            FindQRCode(photoCaptureFrame);
        }
    }

    private void FindQRCode(PhotoCaptureFrame photoCaptureFrame)
    {
//#if NETFX_CORE
        RGBLuminanceSource source = new RGBLuminanceSource(_sourceTexture.GetRawTextureData(), _sourceTexture.width, _sourceTexture.height);
        BinaryBitmap bitmap = new BinaryBitmap(new HybridBinarizer(source));
        ZXing.QrCode.QRCodeReader qrCodeReader = new ZXing.QrCode.QRCodeReader();       
        qrResult = qrCodeReader.decode(bitmap);



        if (qrResult != null)
        {

            //GameObject.Find("/Graph/Canvas/GraphTitle").GetComponent<Text>().text = qrResult.Text;

            #region marker code
            if (false)
            {
                Vector2[] resultPoints = new Vector2[qrResult.ResultPoints.Length];
                for (int i = 0; i < resultPoints.Length; i++)
                {
                    resultPoints[i] = new Vector2(qrResult.ResultPoints[i].X, qrResult.ResultPoints[i].Y);
                    _markers[i].transform.position = MarkerPosition(resultPoints[i]);
                    //_markers[i].GetComponent<Renderer>().enabled = true;
                }

                // Average the 2 diagonal points to get an estimate of the QR code center
                Vector2 qrCenter = (resultPoints[0] + resultPoints[2]) / 2.0f;
                _markers[3].transform.position = MarkerPosition(qrCenter);
                //_markers[3].GetComponent<Renderer>().enabled = true;

                Matrix4x4 cameraToWorldMatrix;
                Matrix4x4 projectionMatrix;
                if (!photoCaptureFrame.TryGetCameraToWorldMatrix(out cameraToWorldMatrix))
                {
                    cameraToWorldMatrix = Camera.main.cameraToWorldMatrix;
                    Debug.Log("Failed to get view matrix from photo");
                }

                if (!photoCaptureFrame.TryGetProjectionMatrix(out projectionMatrix))
                {
                    projectionMatrix = Camera.main.projectionMatrix;
                    Debug.Log("Failed to get view matrix from photo");
                }

                Vector3 normalizedPos = Normalize(qrCenter);
                Vector3 cameraSpacePos = UnProjectVector(projectionMatrix, normalizedPos);

                Vector3 origin = cameraToWorldMatrix * new Vector4(0, 0, 0, 1);
                Vector3 worldPos;
                _debugRay = ImageToWorld(photoCaptureFrame, qrCenter, out worldPos);
                QRPosotion = worldPos;

                float qrProportion = Vector2.Distance(resultPoints[0], resultPoints[1]) / _sourceTexture.height;
                float halfHeight = (cornerDistance / qrProportion) / 2.0f;
                float halfFOV = verticalFOV / 2.0f;
                _distance = halfHeight / Mathf.Tan(halfFOV * Mathf.Deg2Rad);
            }
            #endregion

            Debug.Log(string.Format("QR Text: {0}", qrResult.Text));
            qrResultText = qrResult.Text;

            var segments = qrResultText.Split('|');
            switch (segments[0])
            {
                case "auth":
                    if(segments.Length != 3)
                    {
                        Debug.Log("ERROR: invalid auth QR Code expected 3 segments got: " + segments.Length);
                        break;
                    }
                    AzureStorageConstants.Account = segments[1];
                    AzureStorageConstants.KeyString = segments[2];
                    Debug.Log("Set Auth correctly");
                    audio.GetComponent<AudioSource>().Play();
                    break;
                case "graph":
                    if (segments.Length != 4)
                    {
                        Debug.Log("ERROR: invalid auth QR Code expected 4 segments got: " + segments.Length);
                        break;
                    }
                    var timestamp = segments[1];
                    AzureStorageConstants.container = segments[2];
                    var blob = segments[3];
                    //blob = "iris.hgd";

                    if (!renderedQR.ContainsKey(timestamp))
                    {
                        renderedQR.Add(timestamp, true);
                        Graph.createGraph(blob);
                        Debug.Log("rendering graph");
                        audio.GetComponent<AudioSource>().Play();
                    }
                    break;
            }          



        }
        else
        {
            for (int i = 0; i < _markers.Length; i++)
            {
                _markers[i].GetComponent<Renderer>().enabled = false;
            }

            //Debug.Log("No QR code found");
        }
//#endif
    }

    private Ray ImageToWorld(PhotoCaptureFrame photoCaptureFrame, Vector2 pos, out Vector3 worldPos)
    {
        Matrix4x4 cameraToWorldMatrix;
        Matrix4x4 projectionMatrix;
        if (!photoCaptureFrame.TryGetCameraToWorldMatrix(out cameraToWorldMatrix))
        {
            cameraToWorldMatrix = Camera.main.cameraToWorldMatrix;
            Debug.Log("Failed to get view matrix from photo");
        }

        if (!photoCaptureFrame.TryGetProjectionMatrix(out projectionMatrix))
        {
            projectionMatrix = Camera.main.projectionMatrix;
            Debug.Log("Failed to get view matrix from photo");
        }

        Vector3 normalizedPos = Normalize(pos);
        Vector4 cameraSpacePos = UnProjectVector(projectionMatrix, normalizedPos);

        Vector3 origin = cameraToWorldMatrix * new Vector4(0, 0, 0, 1);
        worldPos = cameraToWorldMatrix * cameraSpacePos;
        return new Ray(origin, (worldPos - origin));
    }

    private Vector3 MarkerPosition(Vector2 p)
    {
        return Vector3.Scale((Normalize(p) + Vector3.forward * 5.0f), new Vector3(4.6f, 3.45f, 1.0f));
    }
    // Convert coordinates to [-1, 1]
    private Vector3 Normalize(Vector2 p)
    {
        return new Vector3((p.x / _sourceTexture.width - 0.5f) * 2.0f, (p.y / _sourceTexture.height - 0.5f) * 2.0f, 1);
    }

    public static Vector4 UnProjectVector(Matrix4x4 proj, Vector3 to)
    {
        Vector4 from = new Vector4(0, 0, 0, 1);
        var axsX = proj.GetRow(0);
        var axsY = proj.GetRow(1);
        var axsZ = proj.GetRow(2);
        from.z = to.z / axsZ.z;
        from.y = (to.y - (from.z * axsY.z)) / axsY.y;
        from.x = (to.x - (from.z * axsX.z)) / axsX.x;
        return from;
    }
    void Update()
    {
        Debug.DrawRay(_debugRay.origin, _debugRay.direction * _distance, Color.green);
    }

    void OnDestroy()
    {
        if(_photoCapture != null)
        {
            _photoCapture.StopPhotoModeAsync(OnStoppedPhotoMode);
        }
        
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        _photoCapture.Dispose();
        _photoCapture = null;
    }

}

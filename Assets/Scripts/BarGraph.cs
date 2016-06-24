using UnityEngine;
using System.Collections;
using System;

public class BarGraph : MonoBehaviour {

    public GameObject barPrefab;

    //private MeshRenderer meshRenderer;
    //private Mesh mesh;
    // Use this for initialization
    void Start() {
        // meshRenderer = this.gameObject.GetComponentInChildren<MeshRenderer>();
        var transform = this.gameObject.transform;
        Debug.Log(transform);

        int n = 20;
        float[] arr = new float[n];
        float[,] data = new float[n, n];
        for (float i = 1; i <= arr.Length; i++)
        {
            arr[(int)i - 1] = i;
        }

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                float x = (float)i / ((float)Math.PI*1);
                float y = (float)j / ((float)Math.PI*1);
                data[i, j] = (float)Math.Sin(Math.Abs(x) + Math.Abs(y))*2 + (UnityEngine.Random.value/2) + 2;
                //data[i, j] = (float)Math.Abs((x * Math.Pow(y, 2)) - (float)(y * Math.Pow(x, 2)));
                //data[i, j] = (float)(Math.Pow(x, 2) + 3 * Math.Pow(y, 2)) * (float)Math.Pow(Math.E, (Math.Pow(-x, 2) - Math.Pow(y, 2)));
            }
        }

        //double start = transform.position.x + transform.localScale.x;
        //Vector2 v2Pos = new Vector2(transform.position.x, transform.position.z);
        //Vector2 v2Scale = new Vector2(transform.localScale.x, transform.position.z);
        //var foo = transform.up;        
        //Vector2 v2_extents = meshRenderer.bounds.extents.xz();
        //Vector2 v2_size = meshRenderer.bounds.size.xz();
        //Vector2 itemSize = v2_size;
        //var itemY = meshRenderer.bounds.max.y;
        //itemSize.Scale(new Vector2(1/(float)n, 1/(float)n));
        //Vector2 v2_start = meshRenderer.bounds.min.xz();
        //Vector2 v2_end = meshRenderer.bounds.max.xz();
        //Vector2 v2_curr = v2_start;

        var localScale = transform.localScale;
        var localExtents = localScale; localExtents.Scale(new Vector3(0.5f, 0f, 0.5f));
        //var start = localScale;
        //start.Scale(new Vector3(-0.5f, 1f, -0.5f));

        var start = new Vector3(-0.5f, 0, -0.5f);

        //var start = meshRenderer.bounds.extents;
        //start.y = meshRenderer.bounds.extents.y;
        //var end = meshRenderer.bounds.extents;
        //end.y = meshRenderer.bounds.extents.y;
        var curr = start;

        var v3_itemSize = new Vector3(1f, 0f, 1f) / (float)n;
        var v3_itemExtents = v3_itemSize / 2;
        curr += v3_itemExtents;

        var padding = v3_itemSize / 20;
        var heightFactor = (v3_itemSize.x + v3_itemSize.z) / 2;

        for (int x = 0; x < data.GetLength(0); x += 1)
        {
            for (int z = 0; z < data.GetLength(1); z += 1)
            {
                float i = data[x, z];
                var height = i * heightFactor;
                //var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                var bar = Instantiate(barPrefab);
                bar.transform.parent = this.gameObject.transform;
                var pos = start + new Vector3(x / (float)n, 0f, z / (float)n) + v3_itemExtents;
                bar.transform.localPosition = pos + new Vector3(0, height / 2 - 0.5f, 0);
                bar.transform.localScale = v3_itemSize + new Vector3(0f, height, 0f) - padding;
                bar.transform.rotation = this.gameObject.transform.rotation;
                curr += v3_itemSize;
            }
        }

        //foreach (float i in arr)
        //{
        //    var height = i * heightFactor;
        //    //var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //    var bar = Instantiate(barPrefab);
        //    bar.transform.parent = this.gameObject.transform;
        //    bar.transform.localPosition = curr + new Vector3(0, height/2 - 0.5f, 0);
        //    bar.transform.localScale = v3_itemSize + new Vector3(0f, height, 0f) - padding;
        //    bar.transform.rotation = this.gameObject.transform.rotation;
        //    curr += v3_itemSize;

        //    //cube.transform.localPosition = new Vector3((i-2.5f)/2, 0, 0);
        //    //v3_itemSize.Scale(new Vector3(0.1f, 0, 0.1f));
           

        //}

        
        

    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
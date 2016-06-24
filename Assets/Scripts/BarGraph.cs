using UnityEngine;
using System.Collections;

public class BarGraph : MonoBehaviour {

    public GameObject barPrefab;
    double[,] data = new double[10,10];
    //private MeshRenderer meshRenderer;
    //private Mesh mesh;
    // Use this for initialization
    void Start() {
       // meshRenderer = this.gameObject.GetComponentInChildren<MeshRenderer>();
        var transform = this.gameObject.transform;
        Debug.Log(transform);

        int n = 10;
        float[] arr = new float[n];
        for(float i=1; i<=arr.Length; i++)
        {
            arr[(int)i-1] = i;
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

        var v3_itemSize = new Vector3(1f,0f,1f) / (float)n;
        var v3_itemExtents = v3_itemSize / 2;
        curr += v3_itemExtents;

        var padding = v3_itemSize / 20;
        var heightFactor = (v3_itemSize.x + v3_itemSize.z)/2;
        foreach (float i in arr)
        {
            var height = i * heightFactor;
            //var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var bar = Instantiate(barPrefab);
            bar.transform.parent = this.gameObject.transform;
            bar.transform.localPosition = curr + new Vector3(0, height/2 - 0.5f, 0);
            bar.transform.localScale = v3_itemSize + new Vector3(0f, height, 0f) - padding;
            bar.transform.rotation = this.gameObject.transform.rotation;
            curr += v3_itemSize;

            //cube.transform.localPosition = new Vector3((i-2.5f)/2, 0, 0);
            //v3_itemSize.Scale(new Vector3(0.1f, 0, 0.1f));
           

        }

        
        

    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
using UnityEngine;
using System.Collections;
using System;

public class BarGraph : MonoBehaviour
{
    //void OnSelect()
    //{ }
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public static void Render(GameObject gameObject, float[] x, float[] y, float[] z)
    {
        if (!(x.Length == y.Length && y.Length == z.Length))
        {
            throw new ArgumentException("Length of x, y, z, and series to scatterplot must all be equal");
        }

        var transform = gameObject.transform;

        // Requires Bar.prefab in Resources folder.
        GameObject barPrefab = Resources.Load(@"Bar", typeof(GameObject)) as GameObject;

        var localScale = transform.localScale;
        var localExtents = localScale; localExtents.Scale(new Vector3(0.5f, 0f, 0.5f));
        
        var start = new Vector3(-0.5f, 0, -0.5f);
        
        var curr = start;

        int n = (int)Math.Sqrt(x.Length); // TODO: Figure this out dynamically from values in data, for now assume symmetric
        var v3_itemSize = new Vector3(1f, 0f, 1f) / (float)n;
        var v3_itemExtents = v3_itemSize / 2;
        curr += v3_itemExtents;

        var padding = v3_itemSize / 20;
        var heightFactor = (v3_itemSize.x + v3_itemSize.z) / 2;
        for (int i = 0; i < x.Length; i += 1)
        {
            var x0 = x[i];
            var y0 = y[i];
            var height = z[i] * heightFactor;
            //var bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var bar = Instantiate(barPrefab);
            bar.transform.parent = gameObject.transform;
            var pos = start + new Vector3(x0 / (float)n, 0f, y0 / (float)n) + v3_itemExtents;
            bar.transform.localPosition = pos + new Vector3(0, height / 2 - 0.5f, 0);
            bar.transform.localScale = v3_itemSize + new Vector3(0f, height, 0f) - padding;
            bar.transform.rotation = gameObject.transform.rotation;
            curr += v3_itemSize;
        }
    }
}
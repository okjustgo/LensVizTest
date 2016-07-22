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

        int numBars = x.Length;
        float xMin = float.MaxValue;
        float xMax = float.MinValue;
        float yMin = float.MaxValue;
        float yMax = float.MinValue;
        float zMin = float.MaxValue;
        float zMax = float.MinValue;
        for (long i = 0; i < numBars; i++)
        {
            if (x[i] < xMin)
            {
                xMin = x[i];
            }
            if (x[i] > xMax)
            {
                xMax = x[i];
            }

            if (y[i] < yMin)
            {
                yMin = y[i];
            }
            if (y[i] > yMax)
            {
                yMax = y[i];
            }

            if (z[i] < zMin)
            {
                zMin = z[i];
            }
            if (z[i] > zMax)
            {
                zMax = z[i];
            }
        }
        int xNumBars = ((int)xMax - (int)xMin) + 1;
        int yNumBars = ((int)yMax - (int)yMin) + 1;

        var transform = gameObject.transform;

        // Requires Bar.prefab in Resources folder.
        GameObject barPrefab = Resources.Load(@"Bar", typeof(GameObject)) as GameObject;

        var localScale = transform.localScale;
        var localExtents = localScale;
        localExtents.Scale(new Vector3(0.5f, 0f, 0.5f));

        var origin = new Vector3(-0.5f, 0, -0.5f);

        // Want to tile bars on a 1 x 1 base. Think about x dimension first:
        // 1 = (xPad + xWidth + xPad) * xNumBars
        // Restrict xPad to be 5% of width, so xPad = xWidth * 0.05,
        // hence 1 = (1.1*xWidth) * xNumBars, solve for xWidth.
        // Calculating yWidth uses the same formula.
        float xWidth = (float)(1 / (1.1 * xNumBars));
        float yWidth = (float)(1 / (1.1 * yNumBars));
        float xPad = (float)(xWidth * 0.05);
        float yPad = (float)(yWidth * 0.05);
        
        for (int i = 0; i < x.Length; i += 1)
        {
            var bar = Instantiate(barPrefab);
            bar.transform.parent = gameObject.transform;

            var xVal = (int)(x[i] - xMin);
            var yVal = (int)(y[i] - yMin);
            var height = (z[i] - zMin) / (zMax - zMin);   // Restrict height to range [0,1] (fitting within a 1 x 1 x 1 cube)
            
            // Place bar within the area of the base.
            var xPos = xVal * (2 * xPad + xWidth) + (xPad + xWidth / 2);
            var yPos = yVal * (2 * yPad + yWidth) + (yPad + yWidth / 2);
            var pos = origin + new Vector3(xPos, 0f, yPos);// + v3_itemExtents;
            // Translate bar to sit directly on top of base.
            bar.transform.localPosition = pos + new Vector3(0, height / 2 - 0.5f, 0);
            bar.transform.localScale = new Vector3(xWidth, height, yWidth);
            bar.transform.rotation = gameObject.transform.rotation;
        }
    }
}
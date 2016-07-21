using UnityEngine;
using System;

public class ScatterPlot : MonoBehaviour
{
    private Color[] availableColors = new Color[] {
        new Color(255, 255, 255),
        new Color(255, 0, 0),
        new Color(0, 255, 0),
        new Color(0, 0, 255),
        new Color(255, 0, 0),
        new Color(0, 255, 0),
        new Color(0, 0, 255),
        new Color(255, 0, 0),
        new Color(0, 255, 0),
        new Color(0, 0, 255)
    };
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public static void Render(GameObject gameObject, float[] x, float[] y, float[] z, float[] series)
    {
        if(!(x.Length == y.Length && y.Length == z.Length && z.Length == series.Length))
        {
            throw new ArgumentException("Length of x, y, z, and series to scatterplot must all be equal");
        }

        int numPoints = x.Length;
        float xMin = float.MaxValue;
        float xMax = float.MinValue;
        float yMin = float.MaxValue;
        float yMax = float.MinValue;
        float zMin = float.MaxValue;
        float zMax = float.MinValue;
        for(long i = 0; i < numPoints; i++)
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
        
        var particles = gameObject.GetComponentInChildren<ParticleSystem>();
        var points = new ParticleSystem.Particle[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            // Scale point to range [0, 1].
            float xVal = (x[i] - 1.0f*xMin) / (1.0f * xMax - 1.0f * xMin);
            float yVal = (y[i] - 1.0f*yMin) / (1.0f * yMax - 1.0f * yMin);
            float zVal = (z[i] - 1.0f*zMin) / (1.0f * zMax - 1.0f * zMin);
            // Flip Z and Y so Z values scale vertically.
            points[i].position = new Vector3(xVal, zVal, yVal);
            if (series[i] == 1)
            {
                points[i].color = new Color(255, 0, 0);
            }
            if (series[i] == 2)
            {
                points[i].color = new Color(0, 255, 0);
            }
            if (series[i] == 3)
            {
                points[i].color = new Color(0, 0, 255);
            }

            points[i].size = 0.05f;
        }

        particles.SetParticles(points, numPoints);
    }
}
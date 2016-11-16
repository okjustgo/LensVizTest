using UnityEngine;
using System;
using Assets.Scripts;
using System.Collections.Generic;

public class ScatterPlot : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public static string Render(GameObject gameObject, float[] x, float[] y, float[] z, float[] color, string legendLabel = null, List<string> categoryLabels = null)
    {
        if(!(x.Length == y.Length && y.Length == z.Length && z.Length == color.Length))
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
        float colorMin = float.MaxValue;
        float colorMax = float.MinValue;
        for (long i = 0; i < numPoints; i++)
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

            if (color[i] < colorMin)
            {
                colorMin = color[i];
            }
            if (color[i] > colorMax)
            {
                colorMax = color[i];
            }
        }

        var numUniqueColors = (int) (colorMax - colorMin) + 1;
        
        var particles = gameObject.GetComponentInChildren<ParticleSystem>();
        var points = new ParticleSystem.Particle[numPoints];
        var parentPosition = gameObject.transform.position;
        var parentRotation = gameObject.transform.rotation;
        var parentScale = gameObject.transform.localScale;
        for (int i = 0; i < numPoints; i++)
        {
            // Scale point to range [0, 1].
            float xVal = (x[i] - 1.0f*xMin) / (1.0f * xMax - 1.0f * xMin) - 0.5f;
            float yVal = (y[i] - 1.0f*yMin) / (1.0f * yMax - 1.0f * yMin) - 0.5f;
            float zVal = (z[i] - 1.0f*zMin) / (1.0f * zMax - 1.0f * zMin) - 0.5f;
            // Flip Z and Y so Z values scale vertically.
            var position = new Vector3(parentScale.x * xVal, parentScale.y * yVal, parentScale.z * zVal);
            points[i].position = parentRotation * position + parentPosition;
            
            if (categoryLabels != null && numUniqueColors >= 1 && numUniqueColors <= VisualizationColors.Discrete.Keys.Count)
            {
                var c = (color[i] - 1.0f * colorMin);
                points[i].startColor = VisualizationColors.Discrete[numUniqueColors][(int) c];
            }
            else
            {
                var c = (color[i] - 1.0f * colorMin) / (1.1f * (1.0f * colorMax - 1.0f * colorMin));
                points[i].startColor = VisualizationColors.Rainbow(c);
            }
            points[i].startSize = 0.03f;
        }

        particles.SetParticles(points, numPoints);

        // Create legend text.
        var legendText = string.Format("<color=white><b>{0}:</b></color>\n", legendLabel);
        if (categoryLabels != null && numUniqueColors >= 1 && numUniqueColors <= VisualizationColors.Discrete.Keys.Count)
        {
            for (var i = 0; i < numUniqueColors; i++)
            {
                var c = VisualizationColors.Discrete[numUniqueColors][i];
                var label = categoryLabels[i];
                legendText += string.Format("<size=4><color=#{0}>{1}</color>\n</size>", VisualizationColors.ColorToHex(c), label);
            }
        }
        else
        {
            const float levels = 10.0f;
            for (var i = (int)levels; i >= 0; i--)
            {
                var c = VisualizationColors.Rainbow(i/levels);
                legendText += string.Format("<size=4><color=#{0}>@{1}</color>\n</size>", VisualizationColors.ColorToHex(c), i == (int)levels ? " - High" : i == 0 ? " - Low" : string.Empty);
            }
        }
        return legendText;
    }
}
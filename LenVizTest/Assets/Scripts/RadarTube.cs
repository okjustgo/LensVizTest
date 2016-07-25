using UnityEngine;
using System;
using System.Collections.Generic;

public class RadarTube : MonoBehaviour
{
    private static Color[] availableColors = new Color[] {
        new Color(1, 0, 0),
        new Color(0, 1, 0),
        new Color(0, 0, 1),
        new Color(1, 1, 0),
        new Color(1, 0, 1),
        new Color(1, 1, 0),
        new Color(0, 1, 1)
    };

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public static void Render(GameObject gameObject, float[] t, float[,] R)
    {
        if (t.Length != R.GetLength(0))
        {
            throw new ArgumentException("Length of t and R to radar tube must all be equal");
        }

        int numLevels = t.Length;
        int numRadarPoints = R.GetLength(1);
        float tMin = float.MaxValue;
        float tMax = float.MinValue;
        float[] radarMins = new float[numRadarPoints];
        float[] radarMaxs = new float[numRadarPoints];
        for(int i = 0; i < numRadarPoints; i++)
        {
            radarMins[i] = 0; // float.MaxValue;
            radarMaxs[i] = float.MinValue;
        }
        for(int i = 0; i < numLevels; i++)
        {
            if(t[i] < tMin)
            {
                tMin = t[i];
            }
            if(t[i] > tMax)
            {
                tMax = t[i];
            }
            for(int j = 0; j < numRadarPoints; j++)
            {
                //if (R[i, j] < radarMins[j])
                //{
                //    radarMins[j] = R[i, j];
                //}
                if (R[i, j] > radarMaxs[j])
                {
                    radarMaxs[j] = R[i, j];
                }
            }
        }
        
        // Find the right mesh filter.
        MeshFilter meshFilter = null;
        var meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i].name == "FrontSurfaceRenderer")
            {
                meshFilter = meshFilters[i];
            }
        }
        var mesh = new Mesh();
        meshFilter.mesh = mesh;

        // Set vertices of surface mesh.
        var radius = 0.5;
        var angle = 2 * Math.PI / numRadarPoints;
        var vertices = new Vector3[numLevels * numRadarPoints];
        var uv = new Vector2[vertices.Length];
        for (int i = 0; i < numLevels; i++)
        {
            var level = (t[i] - tMin) / (tMax - tMin);
            for (int j = 0; j < numRadarPoints; j++)
            {
                var magnitude = (R[i, j] - radarMins[j]) / (radarMaxs[j] - radarMins[j]);
                var xVal = radius * magnitude * Math.Cos(j * angle);
                var yVal = radius * magnitude * Math.Sin(j * angle);
                var zVal = level - 0.5f;
                vertices[i * numRadarPoints + j] = new Vector3((float)xVal, zVal, (float)yVal);
                uv[i * numRadarPoints + j] = new Vector2((float)i / numLevels, (float)j / numRadarPoints);
            }
        }
        mesh.vertices = vertices;
        mesh.uv = uv;

        // Set triangles of mesh (it's not visible without them!)
        var triangles = new int[(numLevels - 1) * numRadarPoints * 6];
        for (int i = 0, ti = 0; i < numLevels - 1; i++)
        {
            for (int j = 0; j < numRadarPoints; j++, ti += 6)
            {
                if (j < numRadarPoints - 1)
                {
                    triangles[ti] = i * numRadarPoints + j;
                    triangles[ti + 1] = i * numRadarPoints + j + numRadarPoints;
                    triangles[ti + 2] = i * numRadarPoints + j + 1;
                    triangles[ti + 3] = i * numRadarPoints + j + 1;
                    triangles[ti + 4] = i * numRadarPoints + j + numRadarPoints;
                    triangles[ti + 5] = i * numRadarPoints + j + numRadarPoints + 1;
                }
                else
                {
                    // Add another two triangles to form a quad which closes this layer.
                    triangles[ti] = i * numRadarPoints + j;
                    triangles[ti + 1] = i * numRadarPoints + j + numRadarPoints;
                    triangles[ti + 2] = i * numRadarPoints + j + 1 - numRadarPoints;
                    triangles[ti + 3] = i * numRadarPoints + j + 1 - numRadarPoints;
                    triangles[ti + 4] = i * numRadarPoints + j + numRadarPoints;
                    triangles[ti + 5] = i * numRadarPoints + j + 1;
                }
            }
        }
        mesh.triangles = triangles;

        var colors = new Color[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            colors[i] = availableColors[i % numRadarPoints];
        }
        mesh.SetColors(new List<Color>(colors));
        
        mesh.RecalculateNormals();
        
    }
}
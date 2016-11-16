using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class VisualizationColors
    {
        // Discrete colorschemes
        public static Dictionary<int, List<Color>> Discrete = new Dictionary<int, List<Color>>
        {
            { 1, new List<Color> {HexToColor("#a50026")}},
            { 2, new List<Color> {HexToColor("#a50026"),HexToColor("#313695")}},
            { 3, new List<Color> {HexToColor("#fc8d59"),HexToColor("#ffffbf"),HexToColor("#91bfdb")}},
            { 4, new List<Color> {HexToColor("#d7191c"),HexToColor("#fdae61"),HexToColor("#abd9e9"),HexToColor("#2c7bb6")}},
            { 5, new List<Color> {HexToColor("#d7191c"),HexToColor("#fdae61"),HexToColor("#ffffbf"),HexToColor("#abd9e9"),HexToColor("#2c7bb6")}},
            { 6, new List<Color> {HexToColor("#d73027"),HexToColor("#fc8d59"),HexToColor("#fee090"),HexToColor("#e0f3f8"),HexToColor("#91bfdb"),HexToColor("#4575b4")}},
            { 7, new List<Color> {HexToColor("#d73027"),HexToColor("#fc8d59"),HexToColor("#fee090"),HexToColor("#ffffbf"),HexToColor("#e0f3f8"),HexToColor("#91bfdb"),HexToColor("#4575b4")}},
            { 8, new List<Color> {HexToColor("#d73027"),HexToColor("#f46d43"),HexToColor("#fdae61"),HexToColor("#fee090"),HexToColor("#e0f3f8"),HexToColor("#abd9e9"),HexToColor("#74add1"),HexToColor("#4575b4")}},
            { 9, new List<Color> {HexToColor("#d73027"),HexToColor("#f46d43"),HexToColor("#fdae61"),HexToColor("#fee090"),HexToColor("#ffffbf"),HexToColor("#e0f3f8"),HexToColor("#abd9e9"),HexToColor("#74add1"),HexToColor("#4575b4")}},
            {10, new List<Color> {HexToColor("#a50026"),HexToColor("#d73027"),HexToColor("#f46d43"),HexToColor("#fdae61"),HexToColor("#fee090"),HexToColor("#e0f3f8"),HexToColor("#abd9e9"),HexToColor("#74add1"),HexToColor("#4575b4"),HexToColor("#313695")}},
            {11, new List<Color> {HexToColor("#a50026"),HexToColor("#d73027"),HexToColor("#f46d43"),HexToColor("#fdae61"),HexToColor("#fee090"),HexToColor("#ffffbf"),HexToColor("#e0f3f8"),HexToColor("#abd9e9"),HexToColor("#74add1"),HexToColor("#4575b4"),HexToColor("#313695")}}
        };

        public static Color Rainbow(float level)
        {
            if (level <= 0.0f)
            {
                level = 0.0001f;
            }
            if (level >= 1.0f)
            {
                level = 1.0f;
            }

            return Color.HSVToRGB(level, 1.0f, 1.0f);
        }

        public static Color HexToColor(string hex)
        {
            var color = new Color();
            if (!ColorUtility.TryParseHtmlString(hex, out color))
            {
                throw new ArgumentException(string.Format("'{0}' is not a hex color"));
            }
            return color;
        }

        public static string ColorToHex(Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }
    }
}

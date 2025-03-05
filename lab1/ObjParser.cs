using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

public class ObjParser
{
    public List<Vector3> Vertices { get; } = new();
    public List<(int, int)> Faces { get; } = new();

    public void Load(string filePath)
    {
        foreach (var line in File.ReadLines(filePath))
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0 || parts[0].StartsWith("#"))
                continue;

            switch (parts[0])
            {
                case "v":
                    Vertices.Add(ParseVector(parts));
                    break;
                case "f":
                    ParseFace(parts);
                    break;
            }
        }
    }

    private static Vector3 ParseVector(string[] parts)
    {
        float x = float.Parse(parts[1], CultureInfo.InvariantCulture);
        float y = parts.Length > 2 ? float.Parse(parts[2], CultureInfo.InvariantCulture) : 0f;
        float z = parts.Length > 3 ? float.Parse(parts[3], CultureInfo.InvariantCulture) : 1f;
        return new Vector3(x, y, z);
    }

    private void ParseFace(string[] parts)
    {
        if (parts.Length >= 4)
        {
            var indices = new List<int>();
            for (var i = 1; i < parts.Length; i++)
            {
                var vertexParts = parts[i].Split('/');
                if (int.TryParse(vertexParts[0], out var vertexIndex))
                {
                    indices.Add(vertexIndex - 1);
                }
            }

            for (var i = 0; i < indices.Count; i++)
            {
                Faces.Add((indices[i], indices[(i + 1) % indices.Count]));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

public class ObjParser
{
    public static IReadOnlyList<Vector3> ParseVertices(string filePath)
    {
        var culture = CultureInfo.InvariantCulture;
        var vertexList = new List<Vector3>();
        foreach (var line in File.ReadLines(filePath))
        {
            if (line.StartsWith("v "))
            {
                var parts = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 4)
                {
                    if (float.TryParse(parts[1], NumberStyles.Float, culture, out var x) &&
                        float.TryParse(parts[2], NumberStyles.Float, culture, out var y) &&
                        float.TryParse(parts[3], NumberStyles.Float, culture, out var z))
                    {
                        vertexList.Add(new Vector3(x, y, z));
                    }
                }
            }
        }
        return vertexList;
    }

    public static IReadOnlyList<(int, int, int)> ParseFaces(string filePath)
    {
        var faceList = new List<(int, int, int)>();
        foreach (var line in File.ReadLines(filePath))
        {
            if (line.StartsWith("f "))
            {
                var parts = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);

                var indices = new List<int>();
                for (var i = 1; i < parts.Length; i++)
                {
                    var vertexParts = parts[i].Split('/');
                    if (int.TryParse(vertexParts[0], out var vertexIndex))
                    {
                        indices.Add(vertexIndex - 1);
                    }
                }

                switch (indices.Count)
                {
                    case 3:
                        faceList.Add((indices[0], indices[1], indices[2]));
                        break;
                    case 4:
                        faceList.Add((indices[0], indices[1], indices[2]));
                        faceList.Add((indices[0], indices[2], indices[3]));
                        break;
                    default:
                        throw new Exception("Unknown face index");
                }
            }
        }
        return faceList;
    }
    //public List<Vector3> Vertices { get; } = new();
    //public List<(int, int, int)> Faces { get; } = new();

    //public void Load(string filePath)
    //{
    //    foreach (var line in File.ReadLines(filePath))
    //    {
    //        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    //        if (parts.Length == 0 || parts[0].StartsWith("#"))
    //            continue;

    //        switch (parts[0])
    //        {
    //            case "v":
    //                Vertices.Add(ParseVector(parts));
    //                break;
    //            case "f":
    //                ParseFace(parts);
    //                break;
    //        }
    //    }
    //}

    //private static Vector3 ParseVector(string[] parts)
    //{
    //    float x = float.Parse(parts[1], CultureInfo.InvariantCulture);
    //    float y = parts.Length > 2 ? float.Parse(parts[2], CultureInfo.InvariantCulture) : 0f;
    //    float z = parts.Length > 3 ? float.Parse(parts[3], CultureInfo.InvariantCulture) : 1f;
    //    return new Vector3(x, y, z);
    //}

    //private void ParseFace(string[] parts)
    //{
    //    if (parts.Length >= 4)
    //    {
    //        var indices = new List<int>();
    //        for (var i = 1; i < parts.Length; i++)
    //        {
    //            var vertexParts = parts[i].Split('/');
    //            if (int.TryParse(vertexParts[0], out var vertexIndex))
    //            {
    //                indices.Add(vertexIndex - 1);
    //            }
    //        }

    //        for (var i = 0; i < indices.Count; i++)
    //        {
    //            Faces.Add((indices[i], indices[(i + 1) % indices.Count], indices[(i+ 2) % indices.Count]));
    //        }
    //    }
    //}

    //public static IReadOnlyList<(int, int, int)> ParseFaces(string filePath)
    //{
    //    var faceList = new List<(int, int, int)>();
    //    foreach (var line in File.ReadLines(filePath))
    //    {
    //        if (line.StartsWith("f "))
    //        {
    //            var parts = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);

    //            var indices = new List<int>();
    //            for (var i = 1; i < parts.Length; i++)
    //            {
    //                var vertexParts = parts[i].Split('/');
    //                if (int.TryParse(vertexParts[0], out var vertexIndex))
    //                {
    //                    indices.Add(vertexIndex - 1);
    //                }
    //            }

    //            switch (indices.Count)
    //            {
    //                case 3:
    //                    faceList.Add((indices[0], indices[1], indices[2]));
    //                    break;
    //                case 4:
    //                    faceList.Add((indices[0], indices[1], indices[2]));
    //                    faceList.Add((indices[0], indices[2], indices[3]));
    //                    break;
    //                default:
    //                    throw new Exception("Unknown face index");
    //            }
    //        }
    //    }
    //    return faceList;
    //}
}

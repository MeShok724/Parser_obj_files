using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace lab1;

public class Vector3Utils
{
    public static Vector3 NormalizeSafe(Vector3 v)
    {
        const float epsilon = 1e-8f;
        float lenSq = v.LengthSquared();

        if (lenSq <= epsilon)
            return Vector3.UnitZ;

        return v * (float)(1.0f / Math.Sqrt(lenSq));
    }

    public static float SafeDot(Vector3 a, Vector3 b)
    {
        float d = Vector3.Dot(a, b);
        return float.IsNaN(d) ? 0f : d;
    }
}


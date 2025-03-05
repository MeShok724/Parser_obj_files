using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

public class WindowTransform
{
    public float width { get; set; }
    public float height { get; set; }

    public Matrix4x4 GetMatrix()
    {
        var windowMatrix = new Matrix4x4
        {
            M11 = width / 2,
            M14 = width / 2,
            M22 = -height / 2,
            M24 = height / 2,
            M33 = 1,
            M44 = 1,
        };

        return Matrix4x4.Transpose(windowMatrix);
    }
}


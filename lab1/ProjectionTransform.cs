using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

public class ProjectionTransform
{
    public float zNear { get; set; } = 1f;
    public float zFar { get; set; } = 100;
    public float width { get; set; } = 1000;
    public float height { get; set; } = 600;
    public float Fov { get; set; } = (float)Math.PI / 2;

    public Matrix4x4 GetMatrix()
    {
        var aspect = width / height;
        var projectionMatrix = new Matrix4x4
        {
            M11 = 1 / (aspect * (float)Math.Tan(Fov / 2)),
            M22 = 1 / (float)Math.Tan(Fov / 2),
            M33 = zFar / (zNear - zFar),
            M34 = -zNear * zFar / (zFar - zNear),
            M43 = -1,
            M44 = 0
        };

        return Matrix4x4.Transpose(projectionMatrix);
    }
}

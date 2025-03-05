using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

public class ObserverTransform
{
    public Vector3 Eye { get; set; } = new Vector3 { X = 0, Y = 0, Z = 100 };
    public Vector3 Target { get; set; } = Vector3.Zero;

    private static readonly Vector3 Up = new(0, 1, 0);

    public Matrix4x4 GetMatrix()
    {
        var axisZ = Vector3.Normalize(Eye - Target);
        var axisX = Vector3.Normalize(Vector3.Cross(Up, axisZ));
        var axisY = Up;

        var observerMatrix = new Matrix4x4
        {
            M11 = axisX.X,
            M12 = axisX.Y,
            M13 = axisX.Z,
            M14 = -Vector3.Dot(axisX, Eye),
            M21 = axisY.X,
            M22 = axisY.Y,
            M23 = axisY.Z,
            M24 = -Vector3.Dot(axisY, Eye),
            M31 = axisZ.X,
            M32 = axisZ.Y,
            M33 = axisZ.Z,
            M34 = -Vector3.Dot(axisZ, Eye),
            M41 = 0f,
            M42 = 0f,
            M43 = 0f,
            M44 = 1f
        };

        return Matrix4x4.Transpose(observerMatrix);
    }
}


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

public class Transformer
{
    public WindowTransform WindowTransform { get; } = new WindowTransform();
    public ProjectionTransform ProjectionTransform { get; } = new ProjectionTransform();
    public ObserverTransform ObserverTransform { get; } = new ObserverTransform();

    public WorldTransform WorldTransform { get; } = new WorldTransform();

    private Matrix4x4 _resultTransformer;

    public IReadOnlyList<Vector3> Transform(IReadOnlyList<Vector3> vertices)
    {

        var result = new List<Vector3>();
        foreach (var vertex in vertices)
        {
            var v4 = Vector4.Transform(vertex, _resultTransformer);
            result.Add(new Vector3(v4.X / v4.W, v4.Y / v4.W, v4.Z / v4.W));
        }
        return result;
    }

    public void RecalculateResultTransform()
    {
        var modelMatrix = WorldTransform.GetTransformationMatrix();
        var viewMatrix = ObserverTransform.GetMatrix();
        var projectionMatrix = ProjectionTransform.GetMatrix();
        var viewportMatrix = WindowTransform.GetMatrix();
        _resultTransformer = modelMatrix * viewMatrix * projectionMatrix * viewportMatrix;
    }
}

﻿using System;
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

    private Matrix4x4 _modelMatrix;

    public (IReadOnlyList<Vector3>, IReadOnlyList<float>) Transform(IReadOnlyList<Vector3> vertices)
    {
        var results = vertices.Select(Transform).ToList();
        return (
            results.Select(r => r.Item1).ToList(),
            results.Select(r => r.Item2).ToList()
        );
    }
    private (Vector3, float) Transform(Vector3 vertex)
    {
        var v4 = Vector4.Transform(vertex, _resultTransformer);
        return (new Vector3(v4.X / v4.W, v4.Y / v4.W, v4.Z / v4.W), 1 / v4.W);
    }
    //public IReadOnlyList<Vector3> Transform(IReadOnlyList<Vector3> vertices)
    //{

    //    var result = new List<Vector3>();
    //    foreach (var vertex in vertices)
    //    {
    //        var v4 = Vector4.Transform(vertex, _resultTransformer);
    //        result.Add(new Vector3(v4.X / v4.W, v4.Y / v4.W, v4.Z / v4.W));
    //    }
    //    return result;
    //}

    public void RecalculateResultTransform()
    {
        _modelMatrix = WorldTransform.GetTransformationMatrix();
        var viewMatrix = ObserverTransform.GetMatrix();
        var projectionMatrix = ProjectionTransform.GetMatrix();
        var viewportMatrix = WindowTransform.GetMatrix();
        _resultTransformer = _modelMatrix * viewMatrix * projectionMatrix * viewportMatrix;
    }

    public Vector3 TransformToWorld(Vector3 vertex)
    {
        var v4 = Vector4.Transform(vertex, _modelMatrix);
        return new Vector3(v4.X, v4.Y, v4.Z);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

public class WorldTransform
{
    public Vector3 Translation { get; private set; } = Vector3.Zero;
    public Vector3 Scale { get; private set; } = Vector3.One;
    public Vector3 RotationAngle { get; private set; } = Vector3.Zero;

    // Матрица перемещения
    public Matrix4x4 TranslationMatrix(Vector3 translation)
    {
        return new Matrix4x4(
            1, 0, 0, translation.X,
            0, 1, 0, translation.Y,
            0, 0, 1, translation.Z,
            0, 0, 0, 1
        );
    }

    // Матрица масштаба
    public Matrix4x4 ScaleMatrix(Vector3 scale)
    {
        return new Matrix4x4(
            scale.X, 0, 0, 0,
            0, scale.Y, 0, 0,
            0, 0, scale.Z, 0,
            0, 0, 0, 1
        );
    }

    // Матрица поворота вокруг оси X
    public Matrix4x4 RotationMatrixX(float angle)
    {
        float cosAngle = (float)Math.Cos(angle);
        float sinAngle = (float)Math.Sin(angle);

        return new Matrix4x4(
            1, 0, 0, 0,
            0, cosAngle, -sinAngle, 0,
            0, sinAngle, cosAngle, 0,
            0, 0, 0, 1
        );
    }

    // Матрица поворота вокруг оси Y
    public Matrix4x4 RotationMatrixY(float angle)
    {
        float cosAngle = (float)Math.Cos(angle);
        float sinAngle = (float)Math.Sin(angle);

        return new Matrix4x4(
            cosAngle, 0, sinAngle, 0,
            0, 1, 0, 0,
            -sinAngle, 0, cosAngle, 0,
            0, 0, 0, 1
        );
    }

    // Матрица поворота вокруг оси Z
    public Matrix4x4 RotationMatrixZ(float angle)
    {
        float cosAngle = (float)Math.Cos(angle);
        float sinAngle = (float)Math.Sin(angle);

        return new Matrix4x4(
            cosAngle, -sinAngle, 0, 0,
            sinAngle, cosAngle, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
        );
    }

    // Применение трансформации к вектору
    public Vector3 ApplyTransformation(Vector3 vector, Matrix4x4 transformationMatrix)
    {
        return Vector3.Transform(vector, transformationMatrix);
    }

    // Функция для комбинирования всех матриц преобразования
    public Matrix4x4 GetTransformationMatrix()
    {
        // Применяем трансформации в следующем порядке: масштаб, повороты, перемещение
        Matrix4x4 translationMatrix = TranslationMatrix(Translation);
        Matrix4x4 scaleMatrix = ScaleMatrix(Scale);
        Matrix4x4 rotationMatrixX = RotationMatrixX(RotationAngle.X);
        Matrix4x4 rotationMatrixY = RotationMatrixY(RotationAngle.Y);
        Matrix4x4 rotationMatrixZ = RotationMatrixZ(RotationAngle.Z);

        // Возвращаем комбинированную матрицу, произведенную на эти матрицы
        return Matrix4x4.Transpose(translationMatrix * scaleMatrix * rotationMatrixX * rotationMatrixY * rotationMatrixZ);
    }

    public void Move(Vector3 delta)
    {
        Translation += delta;
        // Параметры трансформации обновляются автоматически при движении
    }
    public void ScaleBy(float percent)
    {
        var scaleFactor = 1 + (percent / 100f);
        Scale *= new Vector3(scaleFactor, scaleFactor, scaleFactor);
    }
    public void RotateBy(Vector3 deltaRotation) => RotationAngle += deltaRotation;
}

using lab1.lab4;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Windows.Forms;
using static ObjParser;

namespace lab1
{
    public partial class Form1 : Form
    {
        private Bitmap _bitmap;
        private Texture _diffuseTexture;
        private readonly Texture _normalMap;
        private readonly Texture _specularMap;
        private Graphics _graphics;
        private Transformer _transformer;
        private ObjParser _objParser;
        private float[,] _zBuffer;
        private int _width, _height;
        private readonly System.Windows.Forms.Timer _refreshTimer;

        private readonly IReadOnlyList<Vector3> _vertices;
        private Vector3[] _worldVertices;
        private readonly IReadOnlyList<(FaceIndices, FaceIndices, FaceIndices)> _triangles;
        private readonly IReadOnlyList<Vector2> _textureCoordinates;
        private IReadOnlyList<Vector3> _vertexNormals;

        private static readonly Vector3 _light = -new Vector3(1, 1, 1);
        private static readonly Vector3 _color = new Vector3(255, 255, 255);
        private static readonly float _ambientCoeff = 1.0f;
        private static readonly float _specularPower = 15f;
        private static readonly float _diffuseCoeff = 0.7f;

        private HashSet<Keys> pressedKeys = new HashSet<Keys>();
        private const int fps = 150;

        public Form1()
        {
            InitializeComponent();

            DoubleBuffered = true;
            WindowState = FormWindowState.Maximized;
            const string path = "../../../objs/craneo/craneo.OBJ";
            const string diffuse_path = "../../../objs/craneo/craneo_diffuse.jpg";
            const string normal_path = "../../../objs/craneo/craneo_normal.png";
            const string specular_path = "../../../objs/craneo/craneo_specular.jpg";

            Paint += OnPaint;
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
            Resize += WindowResize;

            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = 1000 / fps;
            _refreshTimer.Tick += ApplyTransformations;
            _refreshTimer.Start();


            _objParser = new ObjParser();
            _vertices = ObjParser.ParseVertices(path);
            _triangles = ObjParser.ParseFacesWithUV(path);
            _textureCoordinates = ObjParser.ParseTextureCords(path);
            _vertexNormals = ObjParser.ParseNormals(path);
            _transformer = new Transformer();
            _diffuseTexture = new Texture(diffuse_path);
            _normalMap = new Texture(normal_path);
            _specularMap = new Texture(specular_path);
        }

        private void ApplyTransformations(object? sender, EventArgs e)
        {
            const float moveStep = 1f;
            const float rotateStep = 0.1f;

            const float scalePrecent = 20;

            foreach (var key in pressedKeys)
            {
                switch (key)
                {
                    case Keys.Oemplus:
                        _transformer.WorldTransform.ScaleBy(scalePrecent);
                        break;
                    case Keys.OemMinus:
                        _transformer.WorldTransform.ScaleBy(-scalePrecent);
                        break;

                    case Keys.Left:
                        _transformer.WorldTransform.RotateBy(new Vector3(0, -rotateStep, 0));
                        break;
                    case Keys.Right:
                        _transformer.WorldTransform.RotateBy(new Vector3(0, rotateStep, 0));
                        break;
                    case Keys.Up:
                        _transformer.WorldTransform.RotateBy(new Vector3(rotateStep, 0, 0));
                        break;
                    case Keys.Down:
                        _transformer.WorldTransform.RotateBy(new Vector3(-rotateStep, 0, 0));
                        break;

                    case Keys.A:
                        _transformer.WorldTransform.Move(new Vector3(moveStep, 0, 0));
                        break;
                    case Keys.D:
                        _transformer.WorldTransform.Move(new Vector3(-moveStep, 0, 0));
                        break;
                    case Keys.W:
                        _transformer.WorldTransform.Move(new Vector3(0, 0, moveStep));
                        break;
                    case Keys.S:
                        _transformer.WorldTransform.Move(new Vector3(0, 0, -moveStep));
                        break;
                    case Keys.Tab:
                            _transformer.WorldTransform.Move(new Vector3(0, -moveStep, 0));
                        break;
                    case Keys.ShiftKey:
                        _transformer.WorldTransform.Move(new Vector3(0, moveStep, 0));
                        break;

                    case Keys.Escape:
                        Close();
                        return;
                }
                _transformer.RecalculateResultTransform();
                Invalidate();
            }
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            var (transformedVertices, oneOverWs) = _transformer.Transform(_vertices);
            _worldVertices = _vertices.Select(v => _transformer.TransformToWorld(v)).ToArray();
            var filteredTriangles = FilterTriangles(transformedVertices);
            //var vertexNormals = ComputeVertexNormals();

            _graphics.Clear(Color.Black);
            var data = _bitmap.LockBits(new Rectangle(0, 0, _width, _height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                int* ptr = (int*)data.Scan0;
                int stride = data.Stride / 4;

                for (var i = 0; i < _zBuffer.GetLength(0); i++)
                    for (var j = 0; j < _zBuffer.GetLength(1); j++)
                        _zBuffer[i, j] = 1f;

                Parallel.ForEach(filteredTriangles, triangle =>
                {
                    var (i1, i2, i3) = triangle;
                    RenderTriangle(transformedVertices, oneOverWs, i1, i2, i3, ptr, stride);
                });
            }
            _bitmap.UnlockBits(data);
            e.Graphics.DrawImage(_bitmap, 0, 0);
        }

        private List<(FaceIndices, FaceIndices, FaceIndices)> FilterTriangles(IReadOnlyList<Vector3> transformedVertices)
        {
            return _triangles
                .AsParallel()
                .Where(triangle =>
                {
                    var (i1, i2, i3) = triangle;
                    var p1V2 = new Vector2(transformedVertices[i1.VertexIndex].X, transformedVertices[i1.VertexIndex].Y);
                    var p2V2 = new Vector2(transformedVertices[i2.VertexIndex].X, transformedVertices[i2.VertexIndex].Y);
                    var p3V2 = new Vector2(transformedVertices[i3.VertexIndex].X, transformedVertices[i3.VertexIndex].Y);

                    var v1 = p2V2 - p1V2;
                    var v2 = p3V2 - p1V2;

                    var k = v1.X * v2.Y - v1.Y * v2.X;
                    return k < 0;
                })
                .ToList();
        }

        private unsafe void RenderTriangle(
            IReadOnlyList<Vector3> transformedVertices,
            IReadOnlyList<float> oneOverWs,
            FaceIndices i1, FaceIndices i2, FaceIndices i3,
            int* ptr, int stride)
        {
            var p1 = transformedVertices[i1.VertexIndex];
            var p2 = transformedVertices[i2.VertexIndex];
            var p3 = transformedVertices[i3.VertexIndex];

            var wp1 = _worldVertices[i1.VertexIndex];
            var wp2 = _worldVertices[i2.VertexIndex];
            var wp3 = _worldVertices[i3.VertexIndex];

            var ow1 = oneOverWs[i1.VertexIndex];
            var ow2 = oneOverWs[i2.VertexIndex];
            var ow3 = oneOverWs[i3.VertexIndex];

            var uv1 = _textureCoordinates[i1.UvIndex];
            var uv2 = _textureCoordinates[i2.UvIndex];
            var uv3 = _textureCoordinates[i3.UvIndex];

            Rasterization.ScanlineTriangle(
                (_width, _height),
                new Rasterization.VertexData(p1,  wp1, uv1, ow1),
                new Rasterization.VertexData(p2,  wp2, uv2, ow2),
                new Rasterization.VertexData(p3,  wp3, uv3, ow3),
                (x, y, worldP, uv) =>
                {
                    var z = GetZ(x, y, p1, p2, p3);
                    lock (_zBuffer)
                    {
                        if (_zBuffer[x, y] > z)
                        {
                            _zBuffer[x, y] = z;
                            var texColor = _diffuseTexture.Color(uv.X, uv.Y);
                            var normalFromMap = _normalMap?.Normal(uv.X, uv.Y) ?? new Vector3(0, 0, 1);
                            var specular = _specularMap?.Specular(uv.X, uv.Y) ?? 0;
                            var color = ComputePhongColorWithTextures(normalFromMap, worldP, texColor.ToVector3(), specular);
                            ptr[y * stride + x] = ColorFromVector(color).ToArgb();
                        }
                    }
                }
            );
        }

        private static Color ColorFromVector(Vector3 v)
        {
            return Color.FromArgb(
                255,
                ClampToByte(v.X),
                ClampToByte(v.Y),
                ClampToByte(v.Z)
            );
        }

        private static int ClampToByte(float value)
        {
            if (value < 0f)
                return 0;
            if (value > 255f)
                return 255;
            return (int)value;
        }

        private float GetZ(float x, float y, Vector3 v1, Vector3 v2, Vector3 v3)
        {
            float x1 = v1.X, y1 = v1.Y, z1 = v1.Z;
            float x2 = v2.X, y2 = v2.Y, z2 = v2.Z;
            float x3 = v3.X, y3 = v3.Y, z3 = v3.Z;

            float denominator = (y2 - y3) * (x1 - x3) + (x3 - x2) * (y1 - y3);

            float lambda1 = ((y2 - y3) * (x - x3) + (x3 - x2) * (y - y3)) / denominator;
            float lambda2 = ((y3 - y1) * (x - x3) + (x1 - x3) * (y - y3)) / denominator;
            float lambda3 = 1 - lambda1 - lambda2;

            return lambda1 * z1 + lambda2 * z2 + lambda3 * z3;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            pressedKeys.Add(e.KeyCode);
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            pressedKeys.Remove(e.KeyCode);
        }

        private void WindowResize(object sender, EventArgs e)
        {
            if (ClientSize.Width == 0 || ClientSize.Height == 0)
                return;

            _width = ClientSize.Width;
            _height = ClientSize.Height;
            _bitmap = new Bitmap(_width, _height);
            _graphics = Graphics.FromImage(_bitmap);

            _transformer.WindowTransform.width = ClientSize.Width;
            _transformer.WindowTransform.height = ClientSize.Height;
            _transformer.RecalculateResultTransform();

            _zBuffer = new float[_width, _height];
        }

        private Vector3 ComputePhongColorWithTextures(Vector3 normalMapNormal, Vector3 wp, Vector3 texColor, float specular)
        {
            Vector3 viewDir = Vector3Utils.NormalizeSafe(_transformer.ObserverTransform.Eye - wp);
            Vector3 L = Vector3Utils.NormalizeSafe(-_light);

            Vector3 N = Vector3Utils.NormalizeSafe(normalMapNormal);

            Vector3 ambient = texColor * _ambientCoeff;

            float ndotl = Math.Max(Vector3Utils.SafeDot(N, L), 0);
            Vector3 diffuse = _diffuseCoeff * texColor * ndotl;

            Vector3 R = Vector3.Reflect(-L, N);
            float rdotv = Math.Max(Vector3Utils.SafeDot(R, viewDir), 0);
            Vector3 specularV3 = texColor * (float)Math.Pow(rdotv, _specularPower) * (1 - specular);

            return Vector3.Clamp(ambient + diffuse + specularV3, Vector3.Zero, new Vector3(255, 255, 255));
        }

        //private Vector3 ComputePhongColor(Vector3 N, Vector3 wp, Vector2 uv, Vector3 texColor)
        //{
        //    Vector3 viewDir = Vector3Utils.NormalizeSafe(_transformer.ObserverTransform.Eye - wp);
        //    Vector3 L = Vector3Utils.NormalizeSafe(-_light);

        //    float ndotl = Math.Max(Vector3Utils.SafeDot(N, L), 0);

        //    Vector3 diffuse = _diffuseCoeff * texColor * ndotl;

        //    Vector3 R = Vector3.Reflect(-L, N);
        //    float rdotv = Math.Max(Vector3Utils.SafeDot(R, viewDir), 0);
        //    Vector3 specular = texColor * (float)Math.Pow(rdotv, _specularPower);

        //    return Vector3.Clamp(_ambient + diffuse + specular, Vector3.Zero, new Vector3(255, 255, 255));
        //}

        //private Vector3[] ComputeVertexNormals()
        //{
        //    int vCount = _vertices.Count;
        //    var normals = new Vector3[vCount]; 
        //    var counts = new int[vCount];

        //    foreach (var (i1, i2, i3) in _triangles)
        //    {
        //        var A = _transformer.TransformToWorld(_vertices[i1]);
        //        var B = _transformer.TransformToWorld(_vertices[i2]);
        //        var C = _transformer.TransformToWorld(_vertices[i3]);
        //        Vector3 faceNormal = Vector3Utils.NormalizeSafe(Vector3.Cross(B - A, C - A)); 

        //        normals[i1] += faceNormal;
        //        normals[i2] += faceNormal;
        //        normals[i3] += faceNormal;

        //        counts[i1]++; counts[i2]++; counts[i3]++;
        //    }

        //    for (int i = 0; i < vCount; i++)
        //    {
        //        if (counts[i] > 0)
        //        {
        //            normals[i] = Vector3Utils.NormalizeSafe(normals[i] / counts[i]);
        //        }
        //        else
        //        {
        //            normals[i] = Vector3.UnitY;
        //        }
        //    }

        //    return normals;
        //}
    }
}

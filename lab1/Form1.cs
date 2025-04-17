using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Windows.Forms;

namespace lab1
{
    public partial class Form1 : Form
    {
        private Bitmap _bitmap;
        private Graphics _graphics;
        private Transformer _transformer;
        private ObjParser _objParser;
        private float[,] _zBuffer;
        private int _width, _height;
        private readonly System.Windows.Forms.Timer _refreshTimer;

        private readonly IReadOnlyList<Vector3> _vertices;
        private readonly IReadOnlyList<(int, int, int)> _triangles;

        private readonly Vector3 _light = -new Vector3(1, 1, 1);
        private readonly Vector3 _color1 = new Vector3(255, 255, 255);
        private readonly Vector3 _color2 = new Vector3(255, 255, 255);
        private readonly Vector3 _color3 = new Vector3(255, 255, 255);

        private HashSet<Keys> pressedKeys = new HashSet<Keys>();
        private const int fps = 150;

        public Form1()
        {
            InitializeComponent();

            DoubleBuffered = true;
            WindowState = FormWindowState.Maximized;
            const string path = "../../../objs/figure.obj";

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
            _triangles = ObjParser.ParseFaces(path);

            //_bitmap = new Bitmap(ClientSize.Width, ClientSize.Height);
            //_graphics = Graphics.FromImage(_bitmap);
            _transformer = new Transformer();
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

        //private void OnPaint(object? sender, PaintEventArgs e)
        //{
        //    _graphics.Clear(Color.Black);
        //    var transformedVertices = _transformer.Transform(_objParser.Vertices);
        //    foreach (var (first, second) in _objParser.Faces)
        //    {
        //        Bresenham.BresenhamLine(
        //            new Vector2 { X = transformedVertices[first].X, Y = transformedVertices[first].Y },
        //            new Vector2 { X = transformedVertices[second].X, Y = transformedVertices[second].Y },
        //            (x, y) => {
        //               if (x >= 0 && x < _bitmap.Width && y >= 0 && y < _bitmap.Height)
        //                    _bitmap.SetPixel(x, y, Color.White);
        //            }
        //        );
        //    }
        //    e.Graphics.DrawImage(_bitmap, 0, 0);
        //}

        private void OnPaint(object sender, PaintEventArgs e)
        {
            var transformedVertices = _transformer.Transform(_vertices);
            var filteredTriangles = FilterTriangles(transformedVertices);

            _graphics.Clear(Color.Black);
            var data = _bitmap.LockBits(new Rectangle(0, 0, _width, _height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                int* ptr = (int*)data.Scan0;
                int stride = data.Stride / 4;

                _zBuffer = new float[_width, _height];
                for (var i = 0; i < _zBuffer.GetLength(0); i++)
                    for (var j = 0; j < _zBuffer.GetLength(1); j++)
                        _zBuffer[i, j] = float.MaxValue;

                Parallel.ForEach(filteredTriangles, triangle =>
                {
                    var (i1, i2, i3) = triangle;
                    RenderTriangle(transformedVertices, i1, i2, i3, ptr, stride);
                });
            }
            _bitmap.UnlockBits(data);
            e.Graphics.DrawImage(_bitmap, 0, 0);
        }

        private List<(int, int, int)> FilterTriangles(IReadOnlyList<Vector3> transformedVertices)
        {
            return _triangles
                .AsParallel()
                .Where(triangle =>
                {
                    var (i1, i2, i3) = triangle;
                    var p1V2 = new Vector2(transformedVertices[i1].X, transformedVertices[i1].Y);
                    var p2V2 = new Vector2(transformedVertices[i2].X, transformedVertices[i2].Y);
                    var p3V2 = new Vector2(transformedVertices[i3].X, transformedVertices[i3].Y);

                    var v1 = p2V2 - p1V2;
                    var v2 = p3V2 - p1V2;

                    var k = v1.X * v2.Y - v1.Y * v2.X;
                    return k < 0;
                })
                .ToList();
        }

        private unsafe void RenderTriangle(IReadOnlyList<Vector3> transformedVertices, int i1, int i2, int i3, int* ptr, int stride)
        {
            var p1 = transformedVertices[i1];
            var p2 = transformedVertices[i2];
            var p3 = transformedVertices[i3];
            var intensity = GetIntense(i1, i2, i3);

            Rasterization.ScanlineTriangle(
                (_width, _height),
                new Vector2 { X = p1.X, Y = p1.Y },
                new Vector2 { X = p2.X, Y = p2.Y },
                new Vector2 { X = p3.X, Y = p3.Y },
                intensity * _color1,
                intensity * _color2,
                intensity * _color3,
                (x, y, c) =>
                {
                    var z = GetZ(x, y, p1, p2, p3);
                    lock (_zBuffer)
                    {
                        if (_zBuffer[x, y] > z)
                        {
                            _zBuffer[x, y] = z;
                            ptr[y * stride + x] = Color.FromArgb(255, (int)c.X, (int)c.Y, (int)c.Z).ToArgb();
                        }
                    }
                }
            );
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

        private float GetIntense(int i1, int i2, int i3)
        {
            var v1 = _transformer.TransformToWorld(_vertices[i1]);
            var v2 = _transformer.TransformToWorld(_vertices[i2]);
            var v3 = _transformer.TransformToWorld(_vertices[i3]);
            var n = Vector3.Normalize(Vector3.Cross(v2 - v1, v3 - v1));
            return Math.Max(0.0f, Vector3.Dot(n, -Vector3.Normalize(_light)));
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
            //Invalidate();

            _zBuffer = new float[_width, _height];
        }
    }
}

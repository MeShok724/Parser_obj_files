using System.Drawing;
using System.Numerics;

namespace lab1
{
    public partial class Form1 : Form
    {
        private Bitmap _bitmap;
        private Graphics _graphics;
        private Transformer _transformer;
        private ObjParser _objParser;

        public Form1()
        {
            InitializeComponent();

            DoubleBuffered = true;
            WindowState = FormWindowState.Maximized;

            const string path = "../../../objs/figure.obj";

            Paint += OnPaint;
            KeyDown += OnKeyDown;
            Resize += WindowResize;

            _objParser = new ObjParser();
            _objParser.Load(path);

            _bitmap = new Bitmap(ClientSize.Width, ClientSize.Height);
            _graphics = Graphics.FromImage(_bitmap);
            _transformer = new Transformer();
        }

        private void OnPaint(object? sender, PaintEventArgs e)
        {
            _graphics.Clear(Color.Black);
            var transformedVertices = _transformer.Transform(_objParser.Vertices);
            foreach (var (first, second) in _objParser.Faces)
            {
                Bresenham.BresenhamLine(
                    new Vector2 { X = transformedVertices[first].X, Y = transformedVertices[first].Y },
                    new Vector2 { X = transformedVertices[second].X, Y = transformedVertices[second].Y },
                    (x, y) => {
                       if (x >= 0 && x < _bitmap.Width && y >= 0 && y < _bitmap.Height)
                            _bitmap.SetPixel(x, y, Color.White);
                    }
                );
            }
            e.Graphics.DrawImage(_bitmap, 0, 0);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            const float moveStep = 1f;
            const float rotateStep = 0.1f;

            const float scalePrecent = 20;

            switch (e.KeyCode)
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

        private void WindowResize(object sender, EventArgs e)
        {
            if (ClientSize.Width == 0 || ClientSize.Height == 0)
                return;

            _bitmap = new Bitmap(ClientSize.Width, ClientSize.Height);
            _graphics = Graphics.FromImage(_bitmap);

            _transformer.WindowTransform.width = ClientSize.Width;
            _transformer.WindowTransform.height = ClientSize.Height;
            _transformer.RecalculateResultTransform();
            Invalidate();
        }
    }
}

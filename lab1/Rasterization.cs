using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace lab1
{
    public class Rasterization
    {
        public struct VertexData(Vector3 pos3D, /*Vector3 normal, */ Vector3 world, Vector2 uv, float oneOverW)
        {
            public Vector2 Position = new(pos3D.X, pos3D.Y);
            //public Vector3 Normal = normal;
            public Vector3 World = world;
            public Vector2 UV = uv;
            public float OneOverW = oneOverW;
        }

        public static void ScanlineTriangle(
            (int width, int height) s,
            VertexData v1, VertexData v2, VertexData v3,
            Action<int, int, Vector3, Vector2> setPixel) // без нормали!
        {
            if (v1.Position.Y > v3.Position.Y) (v1, v3) = (v3, v1);
            if (v1.Position.Y > v2.Position.Y) (v1, v2) = (v2, v1);
            if (v2.Position.Y > v3.Position.Y) (v2, v3) = (v3, v2);

            var p1 = v1.Position; var p2 = v2.Position; var p3 = v3.Position;
            var w1 = v1.World; var w2 = v2.World; var w3 = v3.World;
            var uv1 = v1.UV; var uv2 = v2.UV; var uv3 = v3.UV;
            var o1 = v1.OneOverW; var o2 = v2.OneOverW; var o3 = v3.OneOverW;

            float dy13 = p3.Y - p1.Y;
            float dy12 = p2.Y - p1.Y;
            float dy23 = p3.Y - p2.Y;

            Vector2 d13 = (p3 - p1) / dy13;
            Vector2 d12 = (p2 - p1) / dy12;
            Vector2 d23 = (p3 - p2) / dy23;

            Vector3 dw13 = (w3 - w1) / dy13;
            Vector3 dw12 = (w2 - w1) / dy12;
            Vector3 dw23 = (w3 - w2) / dy23;

            Vector2 uvd1 = uv1 * o1;
            Vector2 uvd2 = uv2 * o2;
            Vector2 uvd3 = uv3 * o3;

            Vector2 duvd13 = (uvd3 - uvd1) / dy13;
            Vector2 duvd12 = (uvd2 - uvd1) / dy12;
            Vector2 duvd23 = (uvd3 - uvd2) / dy23;

            float doz13 = (o3 - o1) / dy13;
            float doz12 = (o2 - o1) / dy12;
            float doz23 = (o3 - o2) / dy23;

            int yStart = Math.Max(0, (int)Math.Ceiling(p1.Y));
            int yEnd = Math.Min(s.height, (int)Math.Ceiling(p3.Y));

            for (int y = yStart; y < yEnd; y++)
            {
                bool top = y < p2.Y;
                float ya1 = y - p1.Y;
                float ya2 = y - (top ? p1.Y : p2.Y);

                Vector2 xa = p1 + d13 * ya1;
                Vector3 wa = w1 + dw13 * ya1;
                Vector2 uvaD = uvd1 + duvd13 * ya1;
                float oa = o1 + doz13 * ya1;

                Vector2 xb; Vector3 wb; Vector2 uvbD; float ob;
                if (top)
                {
                    xb = p1 + d12 * ya1;
                    wb = w1 + dw12 * ya1;
                    uvbD = uvd1 + duvd12 * ya1;
                    ob = o1 + doz12 * ya1;
                }
                else
                {
                    xb = p2 + d23 * ya2;
                    wb = w2 + dw23 * ya2;
                    uvbD = uvd2 + duvd23 * ya2;
                    ob = o2 + doz23 * ya2;
                }

                if (xa.X > xb.X)
                {
                    (xa, xb) = (xb, xa);
                    (wa, wb) = (wb, wa);
                    (uvaD, uvbD) = (uvbD, uvaD);
                    (oa, ob) = (ob, oa);
                }

                float dx = xb.X - xa.X;
                Vector3 dw = (wb - wa) / dx;
                Vector2 duvd = (uvbD - uvaD) / dx;
                float doz = (ob - oa) / dx;

                int xStart = Math.Max(0, (int)Math.Ceiling(xa.X));
                int xEnd = Math.Min(s.width, (int)Math.Ceiling(xb.X));

                for (int x = xStart; x < xEnd; x++)
                {
                    float t = x - xa.X;
                    var W = wa + dw * t;
                    var uvd = uvaD + duvd * t;
                    var oinv = oa + doz * t;

                    Vector2 uvCorrected = uvd / oinv;

                    setPixel(x, y, W, uvCorrected);
                }
            }
        }
    }
}

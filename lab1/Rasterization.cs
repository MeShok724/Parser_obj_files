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
        public struct VertexData(Vector3 pos3D, Vector3 normal, Vector3 world)
        {
            public Vector2 Position = new(pos3D.X, pos3D.Y);
            public Vector3 Normal = normal;
            public Vector3 World = world;
        }

        public static void ScanlineTriangle(
            (int width, int height) s,
            VertexData v1, VertexData v2, VertexData v3,
            Action<int, int, Vector3, Vector3> setPixel)
        {
            // Сортировка вершин по Y
            if (v1.Position.Y > v3.Position.Y) (v1, v3) = (v3, v1);
            if (v1.Position.Y > v2.Position.Y) (v1, v2) = (v2, v1);
            if (v2.Position.Y > v3.Position.Y) (v2, v3) = (v3, v2);

            var p1 = v1.Position; var p2 = v2.Position; var p3 = v3.Position;
            var n1 = v1.Normal; var n2 = v2.Normal; var n3 = v3.Normal;
            var w1 = v1.World; var w2 = v2.World; var w3 = v3.World;

            float dy13 = p3.Y - p1.Y;
            float dy12 = p2.Y - p1.Y;
            float dy23 = p3.Y - p2.Y;

            Vector2 d13 = (p3 - p1) / dy13;
            Vector2 d12 = (p2 - p1) / dy12;
            Vector2 d23 = (p3 - p2) / dy23;

            Vector3 dn13 = (n3 - n1) / dy13;
            Vector3 dn12 = (n2 - n1) / dy12;
            Vector3 dn23 = (n3 - n2) / dy23;

            Vector3 dw13 = (w3 - w1) / dy13;
            Vector3 dw12 = (w2 - w1) / dy12;
            Vector3 dw23 = (w3 - w2) / dy23;

            int yStart = Math.Max(0, (int)Math.Ceiling(p1.Y));
            int yEnd = Math.Min(s.height, (int)Math.Ceiling(p3.Y));

            for (int y = yStart; y < yEnd; y++)
            {
                bool topHalf = y < p2.Y;
                float ya1 = y - p1.Y;
                float ya2 = y - (topHalf ? p1.Y : p2.Y);

                Vector2 xa = p1 + ya1 * d13;
                Vector3 na = n1 + ya1 * dn13;
                Vector3 wa = w1 + ya1 * dw13;

                Vector2 xb = topHalf ? p1 + ya1 * d12 : p2 + ya2 * d23;
                Vector3 nb = topHalf ? n1 + ya1 * dn12 : n2 + ya2 * dn23;
                Vector3 wb = topHalf ? w1 + ya1 * dw12 : w2 + ya2 * dw23;

                if (xa.X > xb.X)
                {
                    (xa, xb) = (xb, xa);
                    (na, nb) = (nb, na);
                    (wa, wb) = (wb, wa);
                }

                float dx = xb.X - xa.X;
                Vector3 dn = (nb - na) / dx;
                Vector3 dw = (wb - wa) / dx;

                int xStart = Math.Max(0, (int)Math.Ceiling(xa.X));
                int xEnd = Math.Min(s.width, (int)Math.Ceiling(xb.X));

                for (int x = xStart; x < xEnd; x++)
                {
                    float t = x - xa.X;
                    Vector3 interpNormal = na + t * dn;
                    Vector3 interpWorld = wa + t * dw;
                    setPixel(x, y, interpNormal, interpWorld);
                }
            }
        }
    }
}

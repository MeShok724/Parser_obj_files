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
        public static void ScanlineTriangle(
            (int width, int height) s,
            Vector2 p1, Vector2 p2, Vector2 p3, Vector3 c1, Vector3 c2, Vector3 c3,
            Action<int, int, Vector3> setPixel)
        {
            if (p1.Y > p3.Y)
            {
                (p1, p3) = (p3, p1);
                (c1, c3) = (c3, c1);
            }
            if (p1.Y > p2.Y)
            {
                (p1, p2) = (p2, p1);
                (c1, c2) = (c2, c1);
            }
            if (p2.Y > p3.Y)
            {
                (p2, p3) = (p3, p2);
                (c2, c3) = (c3, c2);
            }

            Vector2 kp1 = (p3 - p1) / (p3.Y - p1.Y);
            Vector2 kp2 = (p2 - p1) / (p2.Y - p1.Y);
            Vector2 kp3 = (p3 - p2) / (p3.Y - p2.Y);

            Vector3 kc1 = (c3 - c1) / (p3.Y - p1.Y);
            Vector3 kc2 = (c2 - c1) / (p2.Y - p1.Y);
            Vector3 kc3 = (c3 - c2) / (p3.Y - p2.Y);

            int top = Math.Max(0, (int)Math.Ceiling(p1.Y));
            int bottom = Math.Min(s.height, (int)Math.Ceiling(p3.Y));

            for (int y = top; y < bottom; y++)
            {
                Vector2 ap = p1 + (y - p1.Y) * kp1;
                Vector3 ac = c1 + (y - p1.Y) * kc1;
                Vector2 bp = y < p2.Y ? p1 + (y - p1.Y) * kp2 : p2 + (y - p2.Y) * kp3;
                Vector3 bc = y < p2.Y ? c1 + (y - p1.Y) * kc2 : c2 + (y - p2.Y) * kc3;

                if (ap.X > bp.X)
                {
                    (ap, bp) = (bp, ap);
                    (ac, bc) = (bc, ac);
                }

                Vector3 kc = (bc - ac) / (bp.X - ap.X);
                int left = Math.Max(0, (int)Math.Ceiling(ap.X));
                int right = Math.Min(s.width, (int)Math.Ceiling(bp.X));

                for (int x = left; x < right; x++)
                {
                    Vector3 c = ac + (x - ap.X) * kc;
                    setPixel(x, y, c);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

public static class Bresenham
{
    public static void BresenhamLine(Vector2 a, Vector2 b, Action<int, int> setPixel)
    {
        int x1 = (int)Math.Round(a.X, MidpointRounding.AwayFromZero);
        int y1 = (int)Math.Round(a.Y, MidpointRounding.AwayFromZero);
        int x2 = (int)Math.Round(b.X, MidpointRounding.AwayFromZero);
        int y2 = (int)Math.Round(b.Y, MidpointRounding.AwayFromZero);

        int dx = x2 - x1;
        int dy = y2 - y1;
        int w = Math.Abs(dx);
        int h = Math.Abs(dy);
        int l = Math.Max(w, h);
        int m11 = Math.Sign(dx);
        int m12 = 0;
        int m21 = 0;
        int m22 = Math.Sign(dy);

        if (w < h)
        {
            (m11, m12) = (m12, m11);
            (m21, m22) = (m22, m21);
        }

        int y = 0;
        int e = 0;
        int eDec = 2 * l;
        int eInc = 2 * Math.Min(w, h);

        for (int x = 0; x <= l; x++)
        {
            int xt = x1 + m11 * x + m12 * y;
            int yt = y1 + m21 * x + m22 * y;
            setPixel(xt, yt);

            if ((e += eInc) > l)
            {
                e -= eDec;
                y++;
            }
        }
    }

}

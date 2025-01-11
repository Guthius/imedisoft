using System;

namespace CodeBase;

public class ODMathLib
{
    public static void Swap(ref float x1, ref float x2)
    {
        (x1, x2) = (x2, x1);
    }
        
    public static float[] IntersectSegments(float x1, float x2, float x3, float x4)
    {
        if (x2 < x1)
        {
            Swap(ref x1, ref x2);
        }

        if (x4 < x3)
        {
            Swap(ref x3, ref x4);
        }

        if (x4 < x1 || x3 > x2)
        {
            return [];
        }

        return [Math.Max(x1, x3), Math.Min(x2, x4)];
    }
        
    public static float[] IntersectRectangles(float x1, float y1, float w1, float h1, float x2, float y2, float w2, float h2)
    {
        var xIntersect = IntersectSegments(x1, x1 + w1, x2, x2 + w2);
        var yIntersect = IntersectSegments(y1, y1 + h1, y2, y2 + h2);
            
        if (xIntersect.Length == 0 || yIntersect.Length == 0)
        {
            return [];
        }

        return
        [
            xIntersect[0], yIntersect[0],
            xIntersect[1] - xIntersect[0], yIntersect[1] - yIntersect[0]
        ];
    }
        
    public static DateTime Max(DateTime dateTime1, DateTime dateTime2)
    {
        return dateTime2 > dateTime1 ? dateTime2 : dateTime1;
    }

    public static DateTime Min(DateTime dateTime1, DateTime dateTime2)
    {
        return dateTime2 < dateTime1 ? dateTime2 : dateTime1;
    }
}
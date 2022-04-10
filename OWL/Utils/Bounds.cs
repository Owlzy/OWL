using Microsoft.Xna.Framework;
using System;

namespace OWL.Util
{
    public class Bounds
    {
        private int minX = int.MaxValue;
        private int minY = int.MaxValue;

        private int maxX = int.MinValue;
        private int maxY = int.MinValue;

        public Rectangle Rect;

        public Bounds()
        {
            Rect = new Rectangle(0, 0, 1, 1);
        }

        public void Clear()
        {
            minX = int.MaxValue;
            minY = int.MaxValue;
            maxX = int.MinValue;
            maxY = int.MinValue;
        }

        public void AddPoint(Point point)
        {
            minX = Math.Min(this.minX, point.X);
            maxX = Math.Max(this.maxX, point.X);
            minY = Math.Min(this.minY, point.Y);
            maxY = Math.Max(this.maxY, point.Y);
        }

        public Rectangle GetRectangle()
        {
            Rect.X = minX;
            Rect.Y = minY;

            Rect.Width = maxX - minX;
            Rect.Height = maxY - minY;

            return Rect;
        }

        public static Bounds operator +(Bounds a, Bounds b)
        {
            a.minX = b.minX < a.minX ? b.minX : a.minX;
            a.minY = b.minY < a.minY ? b.minY : a.minY;
            a.maxX = b.maxX > a.maxX ? b.maxX : a.maxX;
            a.maxY = b.maxY > a.maxY ? b.maxY : a.maxY;
            return a;
        }
    }
}

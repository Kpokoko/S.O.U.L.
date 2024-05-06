using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Extensions
{
    public static class RectangleExtensions
    {
        public static Vector2 GetIntersectionDepth(this Rectangle rectA, Rectangle rectB)
        {
            var distances = GetDistancesBetween(rectA, rectB);
            var distanceX = distances[0];
            var distanceY = distances[1];
            var minDistanceX = distances[2];
            var minDistanceY = distances[3];
            if (Math.Abs(distanceX) >= minDistanceX || Math.Abs(distanceY) >= minDistanceY)
                return Vector2.Zero;

            var depthX = distanceX > 0 ? minDistanceX - distanceX : -minDistanceX - distanceX;
            var depthY = distanceY > 0 ? minDistanceY - distanceY : -minDistanceY - distanceY;
            return new Vector2(depthX, depthY);
        }

        public static float[] GetDistancesBetween(this Rectangle rectA, Rectangle rectB)
        {
            var halfWidthA = rectA.Width / 2.0f;
            var halfHeightA = rectA.Height / 2.0f;
            var halfWidthB = rectB.Width / 2.0f;
            var halfHeightB = rectB.Height / 2.0f;

            var centerA = new Vector2(rectA.Left + halfWidthA, rectA.Top + halfHeightA);
            var centerB = new Vector2(rectB.Left + halfWidthB, rectB.Top + halfHeightB);

            var distanceX = centerA.X - centerB.X;
            var distanceY = centerA.Y - centerB.Y;
            var minDistanceX = halfWidthA + halfWidthB;
            var minDistanceY = halfHeightA + halfHeightB;
            return new float[]
            {
                distanceX,
                distanceY,
                minDistanceX,
                minDistanceY
            };
        }

        public static Vector2 GetBottomCenter(this Rectangle rect)
        {
            return new Vector2(rect.X + rect.Width / 2.0f, rect.Bottom);
        }
    }
}
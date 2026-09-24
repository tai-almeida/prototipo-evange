using System.Collections.Generic;
using UnityEngine;

namespace Evange.Core.Strokes
{
    public static class StrokeRasterizer
    {
        public const int DefaultSize = 128;

        private const float MarginFraction = 0.10f;

        private const float StrokeRadius = 1.5f;

        private const float StepPixels = 0.5f;

        private const byte Ink = 255;
        public static byte[] Rasterize(IReadOnlyList<Vector2> points, int size = DefaultSize)
        {
            var bitmap = new byte[size * size];
            if (points == null || points.Count == 0)
            {
                return bitmap;
            }

            var min = points[0];
            var max = points[0];
            for (var i = 1; i < points.Count; i++)
            {
                min = Vector2.Min(min, points[i]);
                max = Vector2.Max(max, points[i]);
            }

            // Uniform scale off the larger axis: proportions survive, and the shorter
            // axis just gets more empty space. Filling both would make a circle and a
            // flat ellipse identical.
            var span = Mathf.Max(max.x - min.x, max.y - min.y);
            var usable = size * (1f - 2f * MarginFraction);

            // A dot or straight line has zero span; scale 0 collapses it to the centre
            // instead of dividing by zero.
            var scale = span > Mathf.Epsilon ? usable / span : 0f;

            var centre = (min + max) * 0.5f;
            var half = size * 0.5f;

            var previous = ToBitmap(points[0], centre, scale, half);
            Stamp(bitmap, size, previous);

            for (var i = 1; i < points.Count; i++)
            {
                var current = ToBitmap(points[i], centre, scale, half);
                DrawSegment(bitmap, size, previous, current);
                previous = current;
            }

            return bitmap;
        }

        /// Screen space (Y up) to bitmap space (Y down), centred and scaled.
        private static Vector2 ToBitmap(Vector2 point, Vector2 centre, float scale, float half)
        {
            return new Vector2(
                half + (point.x - centre.x) * scale,
                half - (point.y - centre.y) * scale);
        }

        /// Walks the segment in sub-pixel steps. The trackpad samples once per frame,
        /// so a fast flick would otherwise come out dotted.
        private static void DrawSegment(byte[] bitmap, int size, Vector2 from, Vector2 to)
        {
            var steps = Mathf.CeilToInt(Vector2.Distance(from, to) / StepPixels);
            for (var step = 1; step <= steps; step++)
            {
                Stamp(bitmap, size, Vector2.Lerp(from, to, (float)step / steps));
            }
        }

        /// Filled disc, so the stroke has body and corners stay round.
        private static void Stamp(byte[] bitmap, int size, Vector2 centre)
        {
            var reach = Mathf.CeilToInt(StrokeRadius);
            var radiusSquared = StrokeRadius * StrokeRadius;

            var left = Mathf.Max(0, Mathf.FloorToInt(centre.x) - reach);
            var right = Mathf.Min(size - 1, Mathf.CeilToInt(centre.x) + reach);
            var top = Mathf.Max(0, Mathf.FloorToInt(centre.y) - reach);
            var bottom = Mathf.Min(size - 1, Mathf.CeilToInt(centre.y) + reach);

            for (var y = top; y <= bottom; y++)
            {
                for (var x = left; x <= right; x++)
                {
                    var dx = x - centre.x;
                    var dy = y - centre.y;
                    if (dx * dx + dy * dy <= radiusSquared)
                    {
                        bitmap[y * size + x] = Ink;
                    }
                }
            }
        }
    }

}

using System;
using System.Collections.Generic;
using System.Windows;

namespace BezierSpline.Math
{
    public static class MidpointDisplacement
    {
        public static List<List<Point>> Generate(int iterations, double roughness, double displacementFactor, int seed, double width, double height)
        {
            var random = new Random(seed);
            var allSteps = new List<List<Point>>();

            double startY = height * 0.5;

            var currentPoints = new List<Point>
            {
                new Point(0, startY),
                new Point(width, startY)
            };

            allSteps.Add(currentPoints);

            double displacement = height * displacementFactor;

            for (int i = 0; i < iterations; i++)
            {
                var newPoints = new List<Point>();
                for (int j = 0; j < currentPoints.Count - 1; j++)
                {
                    Point p1 = currentPoints[j];
                    Point p2 = currentPoints[j + 1];

                    newPoints.Add(p1);

                    double midX = (p1.X + p2.X) / 2;
                    double midY = (p1.Y + p2.Y) / 2;
                    double offset = (random.NextDouble() - 0.5) * 2 * displacement;
                    double newY = midY + offset;
                    newY = System.Math.Max(0, System.Math.Min(height, newY));
                    newPoints.Add(new Point(midX, newY));
                }
                newPoints.Add(currentPoints[currentPoints.Count - 1]);

                currentPoints = newPoints;
                allSteps.Add(currentPoints);

                displacement *= System.Math.Pow(2, -roughness);
            }

            return allSteps;
        }
    }
}
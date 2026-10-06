using System;
using System.Collections.Generic;
using System.Text;
using LABA_4.Core.Matrix;
using static LABA_4.Core.Heometric.HeometricHelpers;

namespace LABA_4.Core.Models
{
    public abstract class BaseGeometricClass
    {
        public bool IsSelected = false;

        public abstract void ApplyMatrix(MatrixAffine matr);
        public abstract bool HitTest(double x, double y, double threshold);
        public abstract (double X, double Y) GetCenter();

        public void Translate(double dx, double dy)
            => ApplyMatrix(MatrixAffine.Translation(dx, dy));

        public void Rotate(double angleRad, double px, double py)
            => ApplyMatrix(MatrixAffine.RotationAround(angleRad, px, py));

        public void RotateDeg(double angleDeg, double px, double py)
            => ApplyMatrix(MatrixAffine.RotationAroundDeg(angleDeg, px, py));

        public void RotateAroundCenter(double angleRad)
        {
            var (cx, cy) = GetCenter();
            ApplyMatrix(MatrixAffine.RotationAround(angleRad, cx, cy));
        }

        public void RotateAroundCenterDeg(double angleDeg)
        {
            var (cx, cy) = GetCenter();
            ApplyMatrix(MatrixAffine.RotationAroundDeg(angleDeg, cx, cy));
        }

        public void Scale(double sx, double sy)
            => ApplyMatrix(MatrixAffine.Scaling(sx, sy, (0, 0)));

        public void ScaleAround(double sx, double sy, double px, double py)
            => ApplyMatrix(MatrixAffine.Scaling(sx, sy, (px, py)));

        public void ScaleAroundCenter(double sx, double sy)
        {
            var (cx, cy) = GetCenter();
            ApplyMatrix(MatrixAffine.Scaling(sx, sy, (cx, cy)));
        }
    }

    public class PointPol: BaseGeometricClass
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public override (double X, double Y) GetCenter() => (X, Y);
        public PointPol(double x, double y)
        {
            X = x; Y = y;
        }

        public override void ApplyMatrix(MatrixAffine matr)
        {
            double x_t;  double y_t;

            x_t = matr[0, 0] * X + matr[0, 1] * Y + matr[0, 2] * 1;
            y_t = matr[1, 0] * X + matr[1, 1] * Y + matr[1, 2] * 1;

            X = x_t; Y = y_t;
        }

        public override bool HitTest(double x, double y, double threshold)
        => Distance(x, y, X, Y) <= threshold;
    }

    public class Pol: BaseGeometricClass
    {
        protected readonly List<PointPol> _values = new List<PointPol>();
        public bool IsClosed { get; set; } = false;


        public PointPol this[int i]
        {
            get => _values[i];                  
            set => _values[i] = value;
        }

        public int Count => _values.Count;

        public (PointPol a, PointPol b) EdgeAt(int i)
            => (_values[i], _values[(i + 1) % _values.Count]);

        public Pol(params PointPol[] points)
        {
            _values.AddRange(points);
        }
        public override (double X, double Y) GetCenter()
        {
            if (_values.Count == 0) return (0, 0);

            double sx = 0, sy = 0;
            foreach (var p in _values) { sx += p.X; sy += p.Y; }
            return (sx / _values.Count, sy / _values.Count);
        }

        public override void ApplyMatrix(MatrixAffine matr)
        {
            foreach(PointPol point in _values)
            {
                point.ApplyMatrix(matr);
            }
        }

        public override bool HitTest(double x, double y, double threshold)
        => IsNearBorder(x, y, this, threshold);
    }

    public class Edge: BaseGeometricClass
    {
        public PointPol first { get; set; } 
        public PointPol second { get; set; }

        public Edge(PointPol First, PointPol Second)
        {
            first = First;
            second = Second;
        }
        public override void ApplyMatrix(MatrixAffine matr)
        {
            first.ApplyMatrix(matr);
            second.ApplyMatrix(matr);
        }
        public override (double X, double Y) GetCenter()
    => ((first.X + second.X) / 2, (first.Y + second.Y) / 2);
        public override bool HitTest(double x, double y, double threshold)
            => DistanceToSegment(x, y,
                   first.X, first.Y,
                   second.X, second.Y) <= threshold;
    }
}

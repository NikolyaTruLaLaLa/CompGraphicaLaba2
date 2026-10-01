using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace LABA_4.Core.Matrix
{
    public class MatrixAffine
    {
        private readonly double[,] _values = new double[3, 3];
        public const int SIZE = 3;

        public double this[int row, int col]
        {
            get => _values[row, col];
            set => _values[row, col] = value;
        }

        public MatrixAffine()
        {
            for (int i = 0; i < SIZE; i++) { 
                for(int j = 0; j < SIZE; j++)
                {
                    _values[i, j] = 0;
                }
            }
        }
        public MatrixAffine(double m00, double m01, double m02,
                     double m10, double m11, double m12,
                     double m20, double m21, double m22)
        {
            _values[0, 0] = m00; _values[0, 1] = m01; _values[0, 2] = m02;
            _values[1, 0] = m10; _values[1, 1] = m11; _values[1, 2] = m12;
            _values[2, 0] = m20; _values[2, 1] = m21; _values[2, 2] = m22;
        }

        public override string ToString()
        {
            return $"| {_values[0, 0],8:F3} {_values[0, 1],8:F3} {_values[0, 2],8:F3} |\n" +
                   $"| {_values[1, 0],8:F3} {_values[1, 1],8:F3} {_values[1, 2],8:F3} |\n" +
                   $"| {_values[2, 0],8:F3} {_values[2, 1],8:F3} {_values[2, 2],8:F3} |";
        }

        public static MatrixAffine operator *(MatrixAffine a, MatrixAffine b){
            var r = new MatrixAffine();

            for (int i =0;  i < SIZE; i++)
            {
                for (int j = 0; j < SIZE; j++)
                    for (int k = 0; k < SIZE; k++)
                        r[i, j] += a[i, k] * b[k, j];
            }

            return r;
       }

        public static MatrixAffine Identity() => new MatrixAffine(
            1, 0, 0,
            0, 1, 0,
            0, 0, 1);

        public static MatrixAffine Translation(double dx, double dy) => new MatrixAffine(
            1, 0, dx,
            0, 1, dy,
            0, 0, 1);

        public static MatrixAffine Rotation(double angleRad)
        {
            double c = Math.Cos(angleRad);  
            double s = Math.Sin(angleRad);
            return new MatrixAffine(
                 c, -s, 0,
                 s, c, 0,
                 0, 0, 1);
        }

        public static MatrixAffine RotationAround(double angleRad, double px, double py)
            => Translation(px, py) * Rotation(angleRad) * Translation(-px, -py);

        public static MatrixAffine RotationAroundDeg(double angleDeg, double px, double py)
            => RotationAround(angleDeg * Math.PI / 180.0, px, py);
    }
}

using System;
using System.Collections.Generic;
using LABA_4.Core.Matrix;
using LABA_4.Core.Models;
using LABA_4.Core.Heometric;

namespace LABA_4.Core
{
    public class Scene
    {
        private readonly List<BaseGeometricClass> _shapes = new();

        public IReadOnlyList<BaseGeometricClass> Shapes => _shapes;

        public void Add(BaseGeometricClass s) => _shapes.Add(s);
        public void Clear() => _shapes.Clear();

        public void ApplyToAll(MatrixAffine m)
        {
            foreach (var s in _shapes) s.ApplyMatrix(m);
        }

        // === Пространственный запрос ===

        public BaseGeometricClass HitTest(double x, double y, double threshold)
        {
            for (int i = _shapes.Count - 1; i >= 0; i--)
                if (_shapes[i].HitTest(x, y, threshold))
                    return _shapes[i];
            return null;
        }
    }
}
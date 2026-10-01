using System.Collections.Generic;
using System.Linq;
using LABA_4.Core;
using LABA_4.Core.Models;

namespace LABA_4.UI.SceneControls
{
    public class SceneEditor
    {
        private readonly Scene _scene;
        private readonly List<PointPol> _pending = new();

        public bool IsDrawing { get; set; } = false;
        public IReadOnlyList<PointPol> Pending => _pending;
        public BaseGeometricClass Selected { get; private set; }
        public double HitThreshold { get; set; } = 8;

        public SceneEditor(Scene scene)
        {
            _scene = scene;
        }

        // --- Рисование ---

        public void AddPoint(double x, double y)
        {
            if (!IsDrawing) return;
            if (_pending.Count == 0) DeselectAll(); 
            _pending.Add(new PointPol(x, y));
        }

        public void Finish()
        {
            if (_pending.Count == 0) return;

            if (_pending.Count == 1)
                _scene.Add(_pending[0]);
            else if (_pending.Count == 2)
                _scene.Add(new Edge(_pending[0], _pending[1]));
            else
                _scene.Add(new Pol(_pending.ToArray()) { IsClosed = true });

            _pending.Clear();
        }

        public void Cancel() => _pending.Clear();

        // --- Выделение ---

        public void Select(double x, double y)
        {
            DeselectAll();

            var hit = _scene.HitTest(x, y, HitThreshold);
            if (hit != null)
            {
                hit.IsSelected = true;
                Selected = hit;
            }
        }

        public void DeselectAll()
        {
            if (Selected != null)
            {
                Selected.IsSelected = false;
                Selected = null;
            }
        }

        // --- Полная очистка ---

        public void ClearAll()
        {
            DeselectAll();
            _pending.Clear();
            _scene.Clear();
        }
    }
}
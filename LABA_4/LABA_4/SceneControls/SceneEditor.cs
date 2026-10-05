using System.Collections.Generic;
using System.Linq;
using LABA_4.Core;
using LABA_4.Core.Heometric;
using LABA_4.Core.Models;

namespace LABA_4.UI.SceneControls
{
    public enum EditorMode
    {
        None,
        Draw,
        PointInPolygon,
        PointRelativeToEdge
    }

    public class SceneEditor
    {
        private readonly Scene _scene;
        private readonly List<PointPol> _pending = new();

        private EditorMode _mode = EditorMode.None;
        private PointPol _testPoint;               
        private Edge _selectedEdgeForCheck;        
        private int _selectedEdgeIndex = -1;
        private Pol? _polyForCheck;

        public bool IsDrawing => _mode == EditorMode.Draw;
        public bool IsPointInPolygonMode => _mode == EditorMode.PointInPolygon;
        public bool IsPointRelativeToEdgeMode => _mode == EditorMode.PointRelativeToEdge;
        public EditorMode Mode => _mode;

        public IReadOnlyList<PointPol> Pending => _pending;
        public BaseGeometricClass Selected { get; private set; }
        public double HitThreshold { get; set; } = 8;

        public PointPol TestPoint => _testPoint;
        public Edge SelectedEdgeForCheck => _selectedEdgeForCheck;
        public Pol? SelectedPolygonForCheck => _polyForCheck; 
        public int SelectedEdgeIndex => _selectedEdgeIndex;

        public List<PointPol> IntersectionPoints { get; } = new();
        public bool ShowIntersections { get; set; }

        public SceneEditor(Scene scene)
        {
            _scene = scene;
        }

        // --- Режимы ---

        public void SetMode(EditorMode mode)
        {
            if (_mode == EditorMode.PointInPolygon && mode != EditorMode.PointInPolygon)
                ResetPointInPolygonCheck();
            if (_mode == EditorMode.PointRelativeToEdge && mode != EditorMode.PointRelativeToEdge)
                ResetPointEdgeCheck();

            _mode = mode;
            _pending.Clear();
            _testPoint = null;
            if (mode != EditorMode.Draw)
                DeselectAll();
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

        // --- Выделение (единственный источник истины) ---

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

        // --- Проверка: точка в полигоне ---

        public void HandlePointInPolygonSelect(double x, double y)
        {
            if (!IsPointInPolygonMode) return;
            Select(x, y);
            _polyForCheck = Selected as Pol;
        }

        public void HandlePointInPolygonTest(double x, double y)
        {
            if (!IsPointInPolygonMode) return;
            _testPoint = new PointPol(x, y);
        }

        // --- Проверка: точка относительно ребра ---

        public void HandlePointEdgeSelect(double x, double y)
        {
            if (!IsPointRelativeToEdgeMode) return;

            DeselectAll();
            _selectedEdgeForCheck = null;
            _selectedEdgeIndex = -1;
            _polyForCheck = null;

            var hit = _scene.HitTest(x, y, HitThreshold);

            if (hit is Edge edge)
            {
                edge.IsSelected = true;
                Selected = edge;
                _selectedEdgeForCheck = edge;
            }
            else if (hit is Pol poly)
            {
                _polyForCheck = poly;

                double minDist = double.MaxValue;
                int last = poly.IsClosed ? poly.Count : poly.Count - 1;

                for (int i = 0; i < last; i++)
                {
                    var a = poly[i];
                    var b = poly[(i + 1) % poly.Count];
                    double dist = HeometricHelpers.DistanceToSegment(x, y, a.X, a.Y, b.X, b.Y);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        _selectedEdgeForCheck = new Edge(a, b);
                        _selectedEdgeIndex = i;
                    }
                }
            }
        }

        public void HandlePointEdgeTest(double x, double y)
        {
            if (!IsPointRelativeToEdgeMode) return;
            _testPoint = new PointPol(x, y);
        }

        // --- Сброс ---

        public void ResetPointInPolygonCheck()
        {
            _testPoint = null;
            _polyForCheck = null;
            DeselectAll();
        }

        public void ResetPointEdgeCheck()
        {
            _testPoint = null;
            _polyForCheck = null;
            _selectedEdgeForCheck = null;
            _selectedEdgeIndex = -1;
            DeselectAll();
        }

        public void ClearCheckStates()
        {
            ResetPointEdgeCheck();
            ResetPointInPolygonCheck();
        }

        public void ClearTestPoint() => _testPoint = null;

        // --- Полная очистка ---

        public void ClearAll()
        {
            _testPoint = null;
            _polyForCheck = null;
            _selectedEdgeForCheck = null;
            _selectedEdgeIndex = -1;
            IntersectionPoints.Clear();
            ShowIntersections = false;
            DeselectAll();
            _pending.Clear();
            _scene.Clear();
        }
    }
}
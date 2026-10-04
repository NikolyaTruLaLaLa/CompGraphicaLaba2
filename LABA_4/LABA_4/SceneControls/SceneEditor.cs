using System.Collections.Generic;
using System.Linq;
using LABA_4.Core;
using LABA_4.Core.Models;
using LABA_4.Core.Heometric;

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
        private PointPol _firstPointForEdgeCheck = null;
        private Edge _selectedEdgeForCheck = null;
        private Pol _selectedPolygonForCheck = null;
        private int _selectedEdgeIndex = -1;
        private bool _showIntersections = false;
        private readonly List<PointPol> _testPoints = new();

        public bool IsDrawing => _mode == EditorMode.Draw;
        public bool IsPointInPolygonMode => _mode == EditorMode.PointInPolygon;
        public bool IsPointRelativeToEdgeMode => _mode == EditorMode.PointRelativeToEdge;
        public EditorMode Mode => _mode;

        public bool ShowIntersections
        {
            get => _showIntersections;
            set => _showIntersections = value;
        }

        public IReadOnlyList<PointPol> Pending => _pending;
        public BaseGeometricClass Selected { get; private set; }
        public double HitThreshold { get; set; } = 8;
        public PointPol FirstPointForEdgeCheck => _firstPointForEdgeCheck;
        public Edge SelectedEdgeForCheck => _selectedEdgeForCheck;
        public Pol SelectedPolygonForCheck => _selectedPolygonForCheck;
        public int SelectedEdgeIndex => _selectedEdgeIndex;
        public List<PointPol> IntersectionPoints { get; } = new();
        public IReadOnlyList<PointPol> TestPoints => _testPoints;

        public SceneEditor(Scene scene)
        {
            _scene = scene;
        }

        public void SetMode(EditorMode mode)
        {
            if (_mode == EditorMode.PointInPolygon && mode != EditorMode.PointInPolygon)
                ResetPointInPolygonCheck();
            if (_mode == EditorMode.PointRelativeToEdge && mode != EditorMode.PointRelativeToEdge)
                ResetPointEdgeCheck();
            
            _mode = mode;
            _pending.Clear();
            _firstPointForEdgeCheck = null;
            _testPoints.Clear();
            if (mode != EditorMode.Draw)
                DeselectAll();
        }

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

        public void HandlePointInPolygonSelect(double x, double y)
        {
            if (!IsPointInPolygonMode) return;
            
            var hit = _scene.HitTest(x, y, HitThreshold);
            if (hit is Pol poly)
            {
                if (_selectedPolygonForCheck != null)
                    _selectedPolygonForCheck.IsSelected = false;
                
                _selectedPolygonForCheck = poly;
                poly.IsSelected = true;
            }
        }

        public void HandlePointInPolygonTest(double x, double y)
        {
            if (!IsPointInPolygonMode) return;
            
            _firstPointForEdgeCheck = new PointPol(x, y);
            _testPoints.Clear();
            _testPoints.Add(new PointPol(x, y));
        }

        public void HandlePointEdgeSelect(double x, double y)
        {
            if (!IsPointRelativeToEdgeMode) return;

            var hit = _scene.HitTest(x, y, HitThreshold);
            
            if (_selectedEdgeForCheck != null)
            {
                _selectedEdgeForCheck.IsSelected = false;
                _selectedEdgeForCheck = null;
            }
            _selectedEdgeIndex = -1;
            _selectedPolygonForCheck = null;
            
            if (hit is Edge hitEdge)
            {
                _selectedEdgeForCheck = hitEdge;
                hitEdge.IsSelected = true;
            }
            else if (hit is Pol poly)
            {
                double minDist = double.MaxValue;
                Edge closestEdge = null;
                int closestIndex = -1;
                int last = poly.IsClosed ? poly.Count : poly.Count - 1;
                for (int i = 0; i < last; i++)
                {
                    var a = poly[i];
                    var b = poly[(i + 1) % poly.Count];
                    var testEdge = new Edge(a, b);
                    double dist = HeometricHelpers.DistanceToSegment(x, y, a.X, a.Y, b.X, b.Y);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestEdge = testEdge;
                        closestIndex = i;
                    }
                }
                if (closestEdge != null)
                {
                    _selectedEdgeForCheck = closestEdge;
                    _selectedEdgeIndex = closestIndex;
                    _selectedPolygonForCheck = poly;
                }
            }
        }

        public void HandlePointEdgeTest(double x, double y)
        {
            if (!IsPointRelativeToEdgeMode) return;
            
            _firstPointForEdgeCheck = new PointPol(x, y);
            _testPoints.Clear();
            _testPoints.Add(new PointPol(x, y));
        }

        public void ResetPointEdgeCheck()
        {
            _firstPointForEdgeCheck = null;
            _testPoints.Clear();
            _selectedEdgeIndex = -1;
            _selectedPolygonForCheck = null;
            if (_selectedEdgeForCheck != null)
            {
                _selectedEdgeForCheck.IsSelected = false;
                _selectedEdgeForCheck = null;
            }
        }

        public void ResetPointInPolygonCheck()
        {
            _firstPointForEdgeCheck = null;
            _testPoints.Clear();
            if (_selectedPolygonForCheck != null)
            {
                _selectedPolygonForCheck.IsSelected = false;
                _selectedPolygonForCheck = null;
            }
        }

        public void ClearCheckStates()
        {
            ResetPointEdgeCheck();
            ResetPointInPolygonCheck();
        }

        public void ClearTestPoint()
        {
            _firstPointForEdgeCheck = null;
            _testPoints.Clear();
        }

        public void DeselectAll()
        {
            if (Selected != null)
            {
                Selected.IsSelected = false;
                Selected = null;
            }
        }

        public void ClearAll()
        {
            DeselectAll();
            _pending.Clear();
            IntersectionPoints.Clear();
            _firstPointForEdgeCheck = null;
            _selectedEdgeForCheck = null;
            _selectedPolygonForCheck = null;
            _testPoints.Clear();
            _showIntersections = false;
            _scene.Clear();
        }
    }
}
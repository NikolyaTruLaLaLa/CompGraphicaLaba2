using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace LABA_3
{
    public partial class MainWindow
    {
        private const int BlackArgb = unchecked((int)0xFF000000);
        private const int BrushThickness = 3;

        // Красный — цвет подсветки найденной границы (1в)
        private const int HighlightArgb = unchecked((int)0xFFFF0000);

        // Спецзначение: «этот пиксель заливкой не трогать» (для нециклического паттерна)
        private const int NO_COLOR = int.MinValue;

        // Смещения 8 соседей по Муру
        private static readonly int[] DX = new int[] { -1, 0, 1, 1, 1, 0, -1, -1 };
        private static readonly int[] DY = new int[] { -1, -1, -1, 0, 1, 1, 1, 0 };

        private readonly List<Point> _polygon = new List<Point>();

        // Bbox текущей заливки
        private int _fillMinX, _fillMinY, _fillMaxX, _fillMaxY;

        // Для заливки паттерном: массив посещённых пикселей
        private byte[] _visited;

        // Точка, с которой началась заливка (к ней привязывается паттерн)
        private int _seedX, _seedY;

        /// <summary>
        /// Источник цвета для заливки: возвращает ARGB-int для пикселя (x, y).
        /// </summary>
        private delegate int ColorSource(int x, int y);

        // ============================================================
        //  МЫШЬ — ДИСПЕТЧЕР ПО РЕЖИМАМ
        // ============================================================
        private void ImgCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_bitmap == null || _pixels == null) return;

            var p = e.GetPosition(ImgCanvas);
            int x = (int)p.X;
            int y = (int)p.Y;

            // --- Рисование области ---
            if (TglDrawRegion.IsChecked == true)
            {
                HandleDrawRegionClick(e, p, x, y);
                return;
            }

            // --- Заливка цветом (1а) ---
            if (TglFillColor.IsChecked == true)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    int c = GetFillColor();
                    FillRegion(x, y, delegate (int px, int py) { return c; });
                    TxtStatus.Text = "Заливка цветом из (" + x + ", " + y + ")";
                    e.Handled = true;
                }
                return;
            }

            // --- Заливка рисунком (1б) ---
            if (TglFillImage.IsChecked == true)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    if (_patternPixels == null)
                    {
                        MessageBox.Show(this,
                            "Сначала загрузите паттерн (кнопка «Загрузить...» в панели «Паттерн»).",
                            "Паттерн не задан", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    FillRegion(x, y, GetPatternColor);
                    TxtStatus.Text = "Заливка рисунком из (" + x + ", " + y + ")";
                    e.Handled = true;
                }
                return;
            }

            // --- Обход границы (1в) ---
            if (TglPickBoundary.IsChecked == true)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    TraceBoundary(x, y);
                    e.Handled = true;
                }
                return;
            }
        }

        private void HandleDrawRegionClick(MouseButtonEventArgs e, Point p, int x, int y)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (_polygon.Count > 0)
                {
                    Point prev = _polygon[_polygon.Count - 1];
                    DrawLine((int)prev.X, (int)prev.Y, x, y, BlackArgb, BrushThickness);
                }
                _polygon.Add(p);
                TxtStatus.Text = "Точек: " + _polygon.Count;
                e.Handled = true;
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                if (_polygon.Count < 3)
                {
                    _polygon.Clear();
                    TxtStatus.Text = "Нужно минимум 3 точки";
                    return;
                }
                Point first = _polygon[0];
                Point last = _polygon[_polygon.Count - 1];
                DrawLine((int)last.X, (int)last.Y, (int)first.X, (int)first.Y,
                         BlackArgb, BrushThickness);
                TxtStatus.Text = "Область замкнута (" + _polygon.Count + " точек)";
                _polygon.Clear();
            }
        }

        private void ImgCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var pt = e.GetPosition(ImgCanvas);
            TxtCoords.Text = "x: " + (int)pt.X + ", y: " + (int)pt.Y;
        }

        private void ImgCanvas_MouseUp(object sender, MouseButtonEventArgs e) { }

        // ============================================================
        //  ЛИНИЯ (Брезенхэм)
        // ============================================================
        private void DrawLine(int x0, int y0, int x1, int y1, int color, int thickness)
        {
            if (_bitmap == null || _pixels == null) return;

            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;
            int r = thickness / 2;

            int minX = Math.Min(x0, x1) - r;
            int minY = Math.Min(y0, y1) - r;
            int maxX = Math.Max(x0, x1) + r;
            int maxY = Math.Max(y0, y1) + r;

            while (true)
            {
                for (int oy = -r; oy <= r; oy++)
                    for (int ox = -r; ox <= r; ox++)
                        SetPixel(x0 + ox, y0 + oy, color);

                if (x0 == x1 && y0 == y1) break;

                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }

            CommitRegion(minX, minY, maxX, maxY);
        }

        private void SetPixel(int x, int y, int color)
        {
            if (_bitmap == null || _pixels == null) return;
            if (x < 0 || y < 0 || x >= _bitmap.PixelWidth || y >= _bitmap.PixelHeight) return;
            _pixels[y * _bitmap.PixelWidth + x] = color;
        }

        private void CommitRegion(int minX, int minY, int maxX, int maxY)
        {
            if (_bitmap == null || _pixels == null) return;

            minX = Math.Max(0, minX);
            minY = Math.Max(0, minY);
            maxX = Math.Min(_bitmap.PixelWidth - 1, maxX);
            maxY = Math.Min(_bitmap.PixelHeight - 1, maxY);
            if (maxX < minX || maxY < minY) return;

            int w = maxX - minX + 1;
            int h = maxY - minY + 1;
            int[] region = new int[w * h];

            for (int y = 0; y < h; y++)
                Array.Copy(_pixels, (minY + y) * _bitmap.PixelWidth + minX,
                           region, y * w, w);

            _bitmap.WritePixels(new Int32Rect(minX, minY, w, h), region, w * 4, 0);
        }

        // ============================================================
        //  ЗАЛИВКА — ОБЩИЙ АЛГОРИТМ (scanline по сериям)
        // ============================================================
        private void FillRegion(int x, int y, ColorSource source)
        {
            if (_bitmap == null || _pixels == null) return;
            if (x < 0 || y < 0 || x >= _bitmap.PixelWidth || y >= _bitmap.PixelHeight) return;

            int w = _bitmap.PixelWidth;
            int h = _bitmap.PixelHeight;

            int oldColor = _pixels[y * w + x];

            _seedX = x;
            _seedY = y;
            _visited = new byte[w * h];

            _fillMinX = int.MaxValue; _fillMinY = int.MaxValue;
            _fillMaxX = int.MinValue; _fillMaxY = int.MinValue;

            FillSpan(x, y, oldColor, source);

            if (_fillMaxX >= _fillMinX)
                CommitRegion(_fillMinX, _fillMinY, _fillMaxX, _fillMaxY);
        }

        private void FillSpan(int x, int y, int oldColor, ColorSource source)
        {
            int w = _bitmap.PixelWidth;
            int h = _bitmap.PixelHeight;

            if (x < 0 || y < 0 || x >= w || y >= h) return;

            int idx = y * w + x;
            if (_visited[idx] != 0) return;
            if (_pixels[idx] != oldColor) return;

            int x1 = x;
            while (x1 >= 0 && _pixels[y * w + x1] == oldColor && _visited[y * w + x1] == 0) x1--;
            x1++;

            int x2 = x;
            while (x2 < w && _pixels[y * w + x2] == oldColor && _visited[y * w + x2] == 0) x2++;
            x2--;

            for (int i = x1; i <= x2; i++)
            {
                int c = source(i, y);
                if (c != NO_COLOR)
                    _pixels[y * w + i] = c;
                _visited[y * w + i] = 1;
            }

            if (x1 < _fillMinX) _fillMinX = x1;
            if (x2 > _fillMaxX) _fillMaxX = x2;
            if (y < _fillMinY) _fillMinY = y;
            if (y > _fillMaxY) _fillMaxY = y;

            ScanAdjacentRow(x1, x2, y - 1, oldColor, source);
            ScanAdjacentRow(x1, x2, y + 1, oldColor, source);
        }

        private void ScanAdjacentRow(int x1, int x2, int y, int oldColor, ColorSource source)
        {
            if (_bitmap == null) return;
            if (y < 0 || y >= _bitmap.PixelHeight) return;

            int w = _bitmap.PixelWidth;
            int x = x1;

            while (x <= x2)
            {
                int idx = y * w + x;
                if (_pixels[idx] == oldColor && _visited[idx] == 0)
                {
                    FillSpan(x, y, oldColor, source);
                    while (x <= x2 && !(_pixels[y * w + x] == oldColor && _visited[y * w + x] == 0)) x++;
                }
                else
                {
                    x++;
                }
            }
        }

        // ============================================================
        //  ЦВЕТ ИЗ ПАТТЕРНА (1б)
        // ============================================================
        private int GetPatternColor(int x, int y)
        {
            if (_patternPixels == null || _patternWidth == 0 || _patternHeight == 0)
                return GetFillColor();

            if (ChkCyclePattern.IsChecked == true)
            {
                // ЦИКЛИЧЕСКИЙ: паттерн тайлится, начало тайлинга — в точке клика.
                int dx = x - _seedX;
                int dy = y - _seedY;
                int sx = ((dx % _patternWidth) + _patternWidth) % _patternWidth;
                int sy = ((dy % _patternHeight) + _patternHeight) % _patternHeight;
                return _patternPixels[sy * _patternWidth + sx];
            }
            else
            {
                // НЕЦИКЛИЧЕСКИЙ: один паттерн, приклеен к точке клика.
                // Вне границ — не заливаем (NO_COLOR).
                int sx = x - _seedX;
                int sy = y - _seedY;
                if (sx < 0 || sy < 0 || sx >= _patternWidth || sy >= _patternHeight)
                    return NO_COLOR;
                return _patternPixels[sy * _patternWidth + sx];
            }
        }

        // ============================================================
        //  ОБХОД ГРАНИЦЫ (1в) — алгоритм Мура
        // ============================================================
        private void TraceBoundary(int seedX, int seedY)
        {
            if (_bitmap == null || _pixels == null) return;

            Point start = FindNearestBoundary(seedX, seedY, 30);
            if (start.X < 0)
            {
                TxtStatus.Text = "Граница не найдена — кликните ближе к контуру";
                return;
            }

            List<Point> contour = MooreTrace((int)start.X, (int)start.Y);

            if (contour.Count == 0)
            {
                TxtStatus.Text = "Контур пустой";
                return;
            }

            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;

            for (int i = 0; i < contour.Count; i++)
            {
                int x = (int)contour[i].X;
                int y = (int)contour[i].Y;
                _pixels[y * _bitmap.PixelWidth + x] = HighlightArgb;

                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }

            CommitRegion(minX, minY, maxX, maxY);

            TxtStatus.Text = "Обход границы: " + contour.Count + " точек (старт " +
                             (int)start.X + ", " + (int)start.Y + ")";
        }

        private Point FindNearestBoundary(int cx, int cy, int maxRadius)
        {
            if (IsBoundary(cx, cy)) return new Point(cx, cy);

            for (int r = 1; r <= maxRadius; r++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    if (IsBoundary(cx + dx, cy - r)) return new Point(cx + dx, cy - r);
                    if (IsBoundary(cx + dx, cy + r)) return new Point(cx + dx, cy + r);
                }
                for (int dy = -r + 1; dy <= r - 1; dy++)
                {
                    if (IsBoundary(cx - r, cy + dy)) return new Point(cx - r, cy + dy);
                    if (IsBoundary(cx + r, cy + dy)) return new Point(cx + r, cy + dy);
                }
            }
            return new Point(-1, -1);
        }

        private bool IsBoundary(int x, int y)
        {
            if (_bitmap == null || _pixels == null) return false;
            if (x < 0 || y < 0 || x >= _bitmap.PixelWidth || y >= _bitmap.PixelHeight) return false;
            return _pixels[y * _bitmap.PixelWidth + x] == BlackArgb;
        }

        private List<Point> MooreTrace(int startX, int startY)
        {
            List<Point> contour = new List<Point>();

            int x = startX;
            int y = startY;
            int dir = 7;

            long maxSteps = (long)_bitmap.PixelWidth * _bitmap.PixelHeight * 4L;
            long steps = 0;

            do
            {
                contour.Add(new Point(x, y));

                bool found = false;
                for (int i = 1; i <= 8; i++)
                {
                    int nd = (dir + i) % 8;
                    int nx = x + DX[nd];
                    int ny = y + DY[nd];

                    if (IsBoundary(nx, ny))
                    {
                        dir = (nd + 4) % 8;
                        x = nx;
                        y = ny;
                        found = true;
                        break;
                    }
                }

                if (!found) break;

                steps++;
                if (steps > maxSteps) break;
            }
            while (!(x == startX && y == startY && contour.Count > 1));

            return contour;
        }
    }
}
using System.Windows;
using System.Windows.Media;

namespace LABA_3.Drawing
{
    /// <summary>
    /// Вершина градиентного треугольника: координата на холсте и цвет,
    /// которым окрашивается эта точка. Именно цвета вершин задают градиент
    /// (внутри треугольника цвет плавно переходит от вершины к вершине).
    /// </summary>
    /// <remarks>
    /// Тип значимый (<c>struct</c>), поэтому «изменить цвет» нельзя — вместо
    /// этого создаётся новая вершина с той же координатой и новым цветом
    /// (см. <c>GradientTriangleControl.SyncVertexColors</c>).
    /// </remarks>
    public readonly struct GradientVertex
    {
        /// <summary>Координата вершины на пиксельном холсте (в пикселях).</summary>
        public Point Position { get; }

        /// <summary>Цвет в этой вершине — источник градиента в данной точке.</summary>
        public Color Color { get; }

        /// <summary>Создаёт вершину с заданной координатой и цветом.</summary>
        /// <param name="position">Координата вершины на холсте.</param>
        /// <param name="color">Цвет в этой вершине.</param>
        public GradientVertex(Point position, Color color)
        {
            Position = position;
            Color = color;
        }
    }
}

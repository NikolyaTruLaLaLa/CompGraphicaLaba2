using System;
using System.Collections.Generic;
using System.Text;

namespace BezierSpline.Math
{
    /// <summary>
    /// Отрезок кривой L-системы с глубиной ветвления
    /// (для дерева по глубине берутся толщина и цвет).
    /// </summary>
    public readonly struct LSystemSegment
    {
        public LSystemSegment(double x1, double y1, double x2, double y2, int depth)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
            Depth = depth;
        }

        public double X1 { get; }
        public double Y1 { get; }
        public double X2 { get; }
        public double Y2 { get; }
        public int Depth { get; }
    }

    /// <summary>
    /// Описание L-системы: аксиома, угол поворота, начальное направление, правила.
    /// </summary>
    public sealed class LSystemDefinition
    {
        public LSystemDefinition(double angle, double initialDirection, string axiom)
        {
            Angle = angle;
            InitialDirection = initialDirection;
            Axiom = axiom;
        }

        public double Angle { get; }
        public double InitialDirection { get; }
        public string Axiom { get; }

        /// <summary>Символ -> варианты замены (несколько вариантов = случайность).</summary>
        public Dictionary<char, List<string>> Rules { get; } = new Dictionary<char, List<string>>();

        public void AddRule(char atom, string replacement)
        {
            if (!Rules.TryGetValue(atom, out var list))
            {
                list = new List<string>();
                Rules[atom] = list;
            }
            list.Add(replacement);
        }

        /// <summary>
        /// Разбор текстового файла:
        /// строка 1: "&lt;аксиома&gt; &lt;угол поворота&gt; &lt;начальное направление&gt;"
        /// далее: "&lt;символ&gt; &lt;замена&gt;" (несколько правил на символ = случайность).
        /// </summary>
        public static LSystemDefinition Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Описание L-системы пустое.");

            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var header = lines[0].Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (header.Length < 3)
                throw new FormatException("Первая строка: <аксиома> <угол поворота> <начальное направление>");

            if (!double.TryParse(header[1], out double angle))
                throw new FormatException($"Угол поворота не числовой: «{header[1]}»");
            if (!double.TryParse(header[2], out double direction))
                throw new FormatException($"Начальное направление не числовое: «{header[2]}»");

            var def = new LSystemDefinition(angle, direction, header[0]);
            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2 || parts[0].Length == 0)
                    continue;
                def.AddRule(parts[0][0], parts[1]);
            }
            return def;
        }
    }

    /// <summary>
    /// Чистая математика L-систем: переписывание строки и чертежная интерпретация.
    /// Никакого UI — можно тестировать юнит-тестами.
    /// </summary>
    public static class LSystem
    {
        private const double Deg2Rad = System.Math.PI / 180.0;

        /// <summary>
        /// n итераций переписывания. randomness — вероятность выбрать
        /// случайное правило вместо первого (0 — детерминированно).
        /// </summary>
        public static string Expand(LSystemDefinition def, int iterations, double randomness, Random random)
        {
            var current = def.Axiom;
            for (int i = 0; i < iterations; i++)
            {
                var sb = new StringBuilder(current.Length * 2);
                foreach (char c in current)
                {
                    if (def.Rules.TryGetValue(c, out var replacements) && replacements.Count > 0)
                    {
                        string chosen = replacements[0];
                        if (replacements.Count > 1 && random.NextDouble() < randomness)
                            chosen = replacements[random.Next(replacements.Count)];
                        sb.Append(chosen);
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                current = sb.ToString();
            }
            return current;
        }

        /// <summary>
        /// Чертеж: F и G — линия, f — шаг без линии, +/- — поворот,
        /// [ ] — ветвление, | — разворот на 180°.
        /// Направление 0 = вверх, растёт по часовой стрелке (экранные координаты).
        /// angleJitter — случайное отклонение угла поворота в градусах.
        /// </summary>
        public static List<LSystemSegment> Interpret(
            string instructions, double angleDeg, double initialDirection,
            double stepLength, double angleJitter, Random random)
        {
            var segments = new List<LSystemSegment>();
            var stack = new Stack<(double X, double Y, double Dir, int Depth)>();

            double x = 0, y = 0, dir = initialDirection;
            int depth = 0;

            foreach (char c in instructions)
            {
                switch (c)
                {
                    case 'F':
                    case 'G':
                    case 'f':
                        double nx = x + System.Math.Sin(dir * Deg2Rad) * stepLength;
                        double ny = y - System.Math.Cos(dir * Deg2Rad) * stepLength;
                        if (c != 'f') // 'f' — только перемещение; 'F' и 'G' — рисуют
                            segments.Add(new LSystemSegment(x, y, nx, ny, depth));
                        x = nx;
                        y = ny;
                        break;
                    case '+':
                        dir += angleDeg + Jitter(angleJitter, random);
                        break;
                    case '-':
                        dir -= angleDeg + Jitter(angleJitter, random);
                        break;
                    case '|':
                        dir += 180.0;
                        break;
                    case '[':
                        stack.Push((x, y, dir, depth));
                        depth++;
                        break;
                    case ']':
                        var s = stack.Pop();
                        x = s.X;
                        y = s.Y;
                        dir = s.Dir;
                        depth = s.Depth;
                        break;
                }
            }
            return segments;
        }

        private static double Jitter(double angleJitter, Random random)
            => angleJitter > 0 ? (random.NextDouble() * 2.0 - 1.0) * angleJitter : 0.0;

        /// <summary>
        /// Равномерное масштабирование с центрированием: все отрезки помещаются
        /// в прямоугольник width×height с отступом margin.
        /// </summary>
        public static List<LSystemSegment> ScaleToFit(
            List<LSystemSegment> segments, double width, double height, double margin)
        {
            if (segments.Count == 0)
                return new List<LSystemSegment>();

            double minX = double.MaxValue, minY = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue;
            foreach (var s in segments)
            {
                minX = System.Math.Min(minX, System.Math.Min(s.X1, s.X2));
                maxX = System.Math.Max(maxX, System.Math.Max(s.X1, s.X2));
                minY = System.Math.Min(minY, System.Math.Min(s.Y1, s.Y2));
                maxY = System.Math.Max(maxY, System.Math.Max(s.Y1, s.Y2));
            }

            double w = System.Math.Max(maxX - minX, 1e-9);
            double h = System.Math.Max(maxY - minY, 1e-9);
            double scale = System.Math.Min((width - 2 * margin) / w, (height - 2 * margin) / h);
            double offX = (width - w * scale) / 2 - minX * scale;
            double offY = (height - h * scale) / 2 - minY * scale;

            var result = new List<LSystemSegment>(segments.Count);
            foreach (var s in segments)
            {
                result.Add(new LSystemSegment(
                    s.X1 * scale + offX, s.Y1 * scale + offY,
                    s.X2 * scale + offX, s.Y2 * scale + offY, s.Depth));
            }
            return result;
        }
    }
}
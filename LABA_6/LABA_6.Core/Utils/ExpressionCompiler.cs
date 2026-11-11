using Flee.PublicTypes;
using System;
using System.Globalization;

namespace LABA_6.Core.Utils
{
    public static class ExpressionCompiler
    {
        public static Func<float, float, float> Compile(string expression)
        {
            var context = new ExpressionContext();

            context.Options.ParseCulture = CultureInfo.InvariantCulture;

            context.Imports.AddType(typeof(Math));

            context.Variables["x"] = 0f;
            context.Variables["y"] = 0f;

            var compiledExpression = context.CompileDynamic(expression);

            return (x, y) =>
            {
                context.Variables["x"] = x;
                context.Variables["y"] = y;

                return Convert.ToSingle(compiledExpression.Evaluate());
            };
        }
    }
}
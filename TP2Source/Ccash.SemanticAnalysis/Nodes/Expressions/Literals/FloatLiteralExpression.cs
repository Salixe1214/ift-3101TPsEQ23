using System.Globalization;
using Ccash.SemanticAnalysis.Types;
using FloatLiteralExpressionContext = Ccash.Antlr.CcashParser.FloatLiteralExpressionContext;
using Float32LiteralExpressionContext = Ccash.Antlr.CcashParser.Float32LiteralExpressionContext;

namespace Ccash.SemanticAnalysis.Nodes.Expressions.Literals
{
    public class FloatLiteralExpression : IRvalueExpression
    {
        public string Text => $"{Value}";

        public CcashType Type { get; }

        public double Value { get; }

        public FloatLiteralExpression(FloatLiteralExpressionContext context)
        {
            Type = CcashType.Float64;
            Value = double.Parse(context.FloatLiteral().GetText(), CultureInfo.InvariantCulture);
        }

        public FloatLiteralExpression(Float32LiteralExpressionContext context)
        {
            Type = CcashType.Float32;
            var text = context.Float32Literal().GetText();
            text = text.Substring(0, text.Length - 1);
            Value = float.Parse(text, CultureInfo.InvariantCulture);
        }
    }
}

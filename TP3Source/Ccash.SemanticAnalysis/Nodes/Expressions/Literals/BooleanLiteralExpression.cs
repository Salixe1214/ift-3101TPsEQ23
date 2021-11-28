using Ccash.SemanticAnalysis.Types;
using BooleanLiteralExpressionContext = Ccash.Antlr.CcashParser.BooleanLiteralExpressionContext;

namespace Ccash.SemanticAnalysis.Nodes.Expressions.Literals
{
    public class BooleanLiteralExpression : IRvalueExpression
    {
        public string Text => $"{Value}";

        public CcashType Type => CcashType.Boolean;

        public bool Value { get; }

        public BooleanLiteralExpression(BooleanLiteralExpressionContext context)
        {
            Value = bool.Parse(context.GetText());
        }

        public BooleanLiteralExpression(bool value)
        {
            Value = value;
        }
    }
}

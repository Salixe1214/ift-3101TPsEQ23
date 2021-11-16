using System.Text.RegularExpressions;
using Ccash.SemanticAnalysis.Types;
using StringLiteralExpressionContext = Ccash.Antlr.CcashParser.StringLiteralExpressionContext;

namespace Ccash.SemanticAnalysis.Nodes.Expressions.Literals
{
    public class StringLiteralExpression : IRvalueExpression
    {
        public CcashType Type => CcashType.ConstString;

        public string Text => Value;

        public string Value { get; }

        public StringLiteralExpression(StringLiteralExpressionContext context)
        {
            var val = context.StringLiteral().GetText();
            Value = Regex.Unescape(val.Substring(1, val.Length - 2));
        }
    }
}

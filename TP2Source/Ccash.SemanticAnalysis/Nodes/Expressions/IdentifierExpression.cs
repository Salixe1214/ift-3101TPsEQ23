using Antlr4.Runtime;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;
using IdentifierExpressionContext = Ccash.Antlr.CcashParser.IdentifierExpressionContext;

namespace Ccash.SemanticAnalysis.Nodes.Expressions
{
    public class IdentifierExpression : IExpression
    {
        public string Text => Identifier;

        public CcashType Type { get; }

        public string Identifier { get; }

        public bool IsMutable { get; }

        public static IExpression Create(IdentifierExpressionContext context, AbstractScope scope)
        {
            var identifier = context.Identifier().GetText();
            if (!scope.Contains(identifier))
            {
                ErrorManager.IdentifierNotInScope((ParserRuleContext) context.Parent, identifier);
                throw new CannotDetermineTypeException();
            }

            if (scope[identifier].IsStructField)
            {
                var thisExpr = new IdentifierExpression("__this__", scope);
                return new MemberAccessExpression(context, thisExpr, identifier, scope);
            }

            return new IdentifierExpression(identifier, scope);
        }

        public IdentifierExpression(string identifier, AbstractScope scope)
        {
            Identifier = identifier;
            var symbolInfo = scope[identifier];
            Type = symbolInfo.Type;
            IsMutable = symbolInfo.IsMutable;
        }
    }
}

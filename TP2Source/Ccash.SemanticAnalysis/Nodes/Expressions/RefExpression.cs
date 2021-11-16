using Antlr4.Runtime;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;
using RefExpressionContext = Ccash.Antlr.CcashParser.RefExpressionContext;

namespace Ccash.SemanticAnalysis.Nodes.Expressions
{
    public sealed class RefExpression : IExpression
    {
        public string Text => $"ref {Identifier}";

        public CcashType Type { get; }

        public ReferenceType ReferenceType => (ReferenceType) Type;

        public string Identifier { get; }
        
        public RefExpression(RefExpressionContext context, AbstractScope scope)
        {
            Identifier = context.Identifier().GetText();
            if (!scope.Contains(Identifier))
            {
                ErrorManager.IdentifierNotInScope((ParserRuleContext) context.Parent, Identifier);
                throw new CannotDetermineTypeException();
            }

            if (scope[Identifier].Type is ValueType type)
            {
                Type = scope[Identifier].IsMutable ? type.MutRef : type.ConstRef;
            }
            else
            {
                ErrorManager.AddError(context, $"trying to reference non-referable type `{Type}` with operator `ref`");
                Type = scope[Identifier].Type;
            }
        }

        public RefExpression(string identifier, ValueType type, bool isMutable)
        {
            Identifier = identifier;
            Type = new ReferenceType(type, isMutable);
        }

        public RefExpression(IdentifierExpression identifierExpression)
        {
            Identifier = identifierExpression.Identifier;
            Type = new ReferenceType((ValueType) identifierExpression.Type, identifierExpression.IsMutable);
        }
    }
}

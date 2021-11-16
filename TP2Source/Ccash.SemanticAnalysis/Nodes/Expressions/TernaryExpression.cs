using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;
using TernaryExpressionContext = Ccash.Antlr.CcashParser.TernaryExpressionContext;

namespace Ccash.SemanticAnalysis.Nodes.Expressions
{
    public class TernaryExpression : IExpression
    {
        public CcashType Type { get; }

        public string Text { get; }

        public IExpression ConditionExpression { get; }

        public IExpression TrueExpression { get; }

        public IExpression FalseExpression { get; }

        public TernaryExpression(TernaryExpressionContext context, AbstractScope scope)
        {
            Text = context.GetText();

            ConditionExpression = ExpressionFactory.Create(context.expression(0), scope);
            TrueExpression = ExpressionFactory.Create(context.expression(1), scope);
            FalseExpression = ExpressionFactory.Create(context.expression(2), scope);

            if (ConditionExpression.Type.CanBeCoerced(CcashType.Boolean))
            {
                ConditionExpression = ExpressionFactory.Coerce(ConditionExpression, CcashType.Boolean);
            }
            else
            {
                ErrorManager.MismatchedTypes(context, CcashType.Boolean, ConditionExpression.Type);
            }

            if (TrueExpression.Type.CanBeCoerced(FalseExpression.Type))
            {
                TrueExpression = ExpressionFactory.Coerce(TrueExpression, FalseExpression.Type);
            }
            else if (FalseExpression.Type.CanBeCoerced(TrueExpression.Type))
            {
                FalseExpression = ExpressionFactory.Coerce(FalseExpression, TrueExpression.Type);
            }
            else
            {
                ErrorManager.MismatchedTypes(context, TrueExpression.Type, FalseExpression.Type);
            }

            Type = TrueExpression.Type;
        }
    }
}

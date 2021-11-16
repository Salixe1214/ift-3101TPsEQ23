using Ccash.SemanticAnalysis.Types;

namespace Ccash.SemanticAnalysis.Nodes.Expressions
{
    public class AutoDerefExpression : IExpression
    {
        public CcashType Type { get; }
        
        public string Text { get; }

        public IExpression Expression { get; }

        public AutoDerefExpression(IExpression expression)
        {
            Type = ((ReferenceType) expression.Type).ReferredType;
            Text = expression.Text;
            Text = $"*{expression.Text}";
            Expression = expression;
        }
    }
}

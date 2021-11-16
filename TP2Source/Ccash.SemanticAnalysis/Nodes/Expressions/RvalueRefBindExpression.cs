using Ccash.SemanticAnalysis.Nodes.Statements;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;

namespace Ccash.SemanticAnalysis.Nodes.Expressions
{
    public class RvalueRefBindExpression : IExpression
    {
        public CcashType Type => RefExpression.Type;

        public string Text => RefExpression.Text;

        public VariableDeclaration TempVariable { get; }

        public RefExpression RefExpression { get; }

        public RvalueRefBindExpression(IRvalueExpression rvalueExpression, AbstractScope scope)
        {
            TempVariable = TemporaryVariable.New(rvalueExpression, true, scope);
            var variableName = TempVariable.Name;
            var variableType = (ValueType) TempVariable.Type;
            RefExpression = new RefExpression(variableName, variableType, true);
        }
    }
}

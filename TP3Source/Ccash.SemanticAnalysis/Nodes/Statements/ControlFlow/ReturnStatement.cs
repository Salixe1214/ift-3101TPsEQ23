using System.Linq;
using Ccash.SemanticAnalysis.Attributes;
using Ccash.SemanticAnalysis.Nodes.Expressions;
using Ccash.SemanticAnalysis.SymbolTable;
using ReturnStatementContext = Ccash.Antlr.CcashParser.ReturnStatementContext;

namespace Ccash.SemanticAnalysis.Nodes.Statements.ControlFlow
{
    [SemanticRule("this.AlwaysReturns = true")]
    public class ReturnStatement : IStatement
    {
        public IExpression Expression { get; }

        public bool AlwaysReturns => true;

        public ReturnStatement()
        {
        }
        
        public ReturnStatement(ReturnStatementContext context, AbstractScope scope)
        {
            if (context.expression() == null)
            {
                return;
            }
            
            Expression = ExpressionFactory.Create(context.expression(), scope);

            var returnType = scope.Enclosing<FunctionDeclaration>().First().Header.FunctionType.ReturnType;

            if (Expression.Type.CanBeCoerced(returnType))
            {
                Expression = ExpressionFactory.Coerce(Expression, returnType);
            }
            else
            {
                ErrorManager.MismatchedTypes(context, returnType, Expression.Type);
            }
        }
    }
}

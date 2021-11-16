using Ccash.SemanticAnalysis.SymbolTable;
using UnaryOperatorExpressionContext = Ccash.Antlr.CcashParser.UnaryOperatorExpressionContext;

namespace Ccash.SemanticAnalysis.Nodes.Expressions
{
    public class UnaryOperatorExpression : FunctionCallExpression
    {
        public UnaryOperatorExpression(UnaryOperatorExpressionContext context, AbstractScope scope)
        {
            var operand = ExpressionFactory.Create(context.expression(), scope);
            var operatorString = context.children[0].GetText();
            FunctionName = $"operator{operatorString}";
            Arguments = new[] {operand};

            try
            {
                var (header, arguments) = FunctionOverloadResolver.Resolve(FunctionName, scope, Arguments);
                FunctionType = header.FunctionType;
                FullFunctionName = header.FullName;
                Type = FunctionType.ReturnType;
                Arguments = arguments;
            }
            catch (NoValidOverloadsException)
            {
                ErrorManager.NoValidOverload(context, FunctionName, Arguments);
                throw new CannotDetermineTypeException();
            }
            catch (AmbiguousCallException e)
            {
                ErrorManager.AmbiguousCall(context, e.Overloads, Arguments);
                throw new CannotDetermineTypeException();
            }
        }
    }
}

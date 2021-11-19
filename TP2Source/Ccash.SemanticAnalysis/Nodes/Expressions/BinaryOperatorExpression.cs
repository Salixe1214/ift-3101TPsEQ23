using Ccash.SemanticAnalysis.SymbolTable;
using BinaryOperatorExpressionContext = Ccash.Antlr.CcashParser.BinaryOperatorExpressionContext;

namespace Ccash.SemanticAnalysis.Nodes.Expressions
{
    public class BinaryOperatorExpression: FunctionCallExpression
    {
        public BinaryOperatorExpression(BinaryOperatorExpressionContext context, AbstractScope scope)
        {
            var leftOperand = ExpressionFactory.Create(context.expression()[0], scope);
            var rightOperand = ExpressionFactory.Create(context.expression()[1], scope);


            var operatorString = context.children[1].GetText();
            FunctionName = $"operator{operatorString}";
            Arguments = new[] {leftOperand, rightOperand};

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

using System.Linq;
using Ccash.SemanticAnalysis.Nodes.Expressions.Literals;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;
using IndexOperatorExpressionContext = Ccash.Antlr.CcashParser.IndexOperatorExpressionContext;

namespace Ccash.SemanticAnalysis.Nodes.Expressions
{
    public sealed class IndexOperatorExpression : FunctionCallExpression
    {
        public IndexOperatorExpression(IndexOperatorExpressionContext context, AbstractScope scope)
        {
            FunctionName = "operator[]";

            Arguments = context.expression().Select(e => ExpressionFactory.Create(e, scope)).ToArray();

            Arguments[0] = ExpressionFactory.CoerceStructToRef(Arguments[0], scope);

            if (Arguments.First().Type is ReferenceType refType && refType.ReferredType is ArrayType arrayType)
            {
                if (arrayType.ContainedType is ValueType valueType)
                {
                    Type = arrayType.IsMutable ? valueType.MutRef : valueType.ConstRef;
                }
                else
                {
                    Type = arrayType.ContainedType;
                }

                if (Arguments[1].Type.CanBeCoerced(CcashType.Uint32))
                {
                    Arguments[1] = ExpressionFactory.Coerce(Arguments[1], CcashType.Uint32);
                }
                else if (Arguments[1] is IntegerLiteralExpression intLiteral && intLiteral.FitsInto(CcashType.Uint32))
                {
                    Arguments[1] = ExpressionFactory.Coerce(Arguments[1], CcashType.Uint32);
                }
                else
                {
                    ErrorManager.MismatchedTypes(context, CcashType.Uint32, Arguments[1].Type);
                    return;
                }

                var header = new FunctionHeader("operator[]", Type, Arguments.Select(a => a.Type).ToArray());
                FullFunctionName = header.FullName;
                FunctionType = header.FunctionType;

                return;
            }

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

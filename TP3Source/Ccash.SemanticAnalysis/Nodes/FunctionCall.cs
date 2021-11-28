using System.Linq;
using Antlr4.Runtime;
using Ccash.SemanticAnalysis.Nodes.Expressions;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;
using FunctionCallContext = Ccash.Antlr.CcashParser.FunctionCallContext;

namespace Ccash.SemanticAnalysis.Nodes
{
    public static class FunctionCall
    {
        public static (FunctionHeader, IExpression[]) ResolveFunction(FunctionCallContext context, AbstractScope scope)
        {
            var arguments = context
                            .functionArgs()
                            ?.expression()
                            ?.Select(e => ExpressionFactory.Create(e, scope))
                            .ToArray() ?? new IExpression[] { };

            var functionName = context.functionName().GetText();
            if (functionName == "printf")
            {
                for (var i = 0; i < arguments.Length; i++)
                {
                    if (arguments[i].Type == CcashType.ConstString)
                    {
                        arguments[i] = ExpressionFactory.CoerceStructToRef(arguments[i], scope);
                    }
                }
                ValidatePrintf(context, arguments);
                return (new FunctionHeader(functionName, CcashType.Void, CcashType.ConstString), arguments);
            }

            return ResolveFunction(context, scope, arguments, functionName);
        }

        public static (FunctionHeader, IExpression[]) ResolveFunction(ParserRuleContext context, AbstractScope scope, IExpression[] args, string functionName)
        {
            try
            {
                return FunctionOverloadResolver.Resolve(functionName, scope, args);
            }
            catch (FunctionNotFoundException)
            {
                ErrorManager.IdentifierNotInScope(context, functionName);
                return (new FunctionHeader(functionName, CcashType.Void, args.Select(a => a.Type).ToArray()), args);
            }
            catch (NoValidOverloadsException)
            {
                ErrorManager.NoValidOverload(context, functionName, args);
                return (new FunctionHeader(functionName, CcashType.Void, args.Select(a => a.Type).ToArray()), args);
            }
            catch (AmbiguousCallException e)
            {
                ErrorManager.AmbiguousCall(context, e.Overloads, args);
                return (new FunctionHeader(functionName, CcashType.Void, args.Select(a => a.Type).ToArray()), args);
            }
            catch (WrongArityException e)
            {
                ErrorManager.WrongArity(context,
                                        functionName,
                                        e.Overload.FunctionType.ParameterTypes.Count,
                                        args.Length);
                return (e.Overload, args);
            }
            catch (ArgumentTypeMismatchException e)
            {
                for (var i = 0; i < e.Overload.FunctionType.ParameterTypes.Count; i++)
                {
                    var paramType = e.Overload.FunctionType.ParameterTypes[i];
                    var argument = args[i];
                    
                    if (argument.Type != paramType && !argument.Type.CanBeCoerced(paramType))
                    {
                        ErrorManager.MismatchedTypes(context, paramType, argument.Type);
                    }
                }

                return (e.Overload, args);
            }
        }

        private static void ValidatePrintf(ParserRuleContext context, params IExpression[] arguments)
        {
            if (!arguments.Any())
            {
                const string error = "`printf` takes at least 1 argument, but none were supplied";
                ErrorManager.AddError(context, error);
            }
            else if (!(arguments[0].Type is ReferenceType refType && refType.ReferredType == CcashType.ConstString))
            {
                ErrorManager.MismatchedTypes(context, CcashType.ConstString, arguments.First().Type);
            }
            else
            {
                for (var i = 1; i < arguments.Length; i++)
                {
                    var argument = arguments[i];
                    ValidatePrintfType(context, argument.Type);
                    if (argument.Type is ReferenceType type && type.ReferredType != CcashType.ConstString)
                    {
                        arguments[i] = new AutoDerefExpression(argument);
                    }
                }
            }
        }

        private static void ValidatePrintfType(ParserRuleContext context, CcashType type)
        {
            switch (type)
            {
                case ArrayType arrayType when arrayType.ContainedType == CcashType.Uint8: break;
                case IntegerType integerType:
                    if (integerType != CcashType.Int32 && integerType != CcashType.Uint32)
                    {
                        ErrorManager.MismatchedTypes(context, CcashType.Int32, integerType);
                    }

                    break;
                case FloatType floatType:
                    if (floatType != CcashType.Float64)
                    {
                        ErrorManager.MismatchedTypes(context, CcashType.Int32, floatType);
                    }

                    break;
                case ReferenceType refType:
                {
                    ValidatePrintfType(context, refType.ReferredType);
                    break;
                }
                default:
                    var error =
                        $"expected one of `int32`, `uint32`, `float64`, or `string`, found `{type}`";
                    ErrorManager.AddError(context, error);
                    break;
            }
        }
    }
}

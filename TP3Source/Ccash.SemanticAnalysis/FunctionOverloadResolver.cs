using System;
using System.Collections.Generic;
using System.Linq;
using Ccash.SemanticAnalysis.Nodes;
using Ccash.SemanticAnalysis.Nodes.Expressions;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;

namespace Ccash.SemanticAnalysis
{
    public class NoValidOverloadsException : Exception
    {
        public NoValidOverloadsException() : base("no suitable overload")
        {
        }
    }

    public class AmbiguousCallException : Exception
    {
        public List<FunctionHeader> Overloads { get; }

        public AmbiguousCallException(List<FunctionHeader> overloads) : base("too many overloads")
        {
            Overloads = overloads;
        }
    }

    public class WrongArityException : Exception
    {
        public FunctionHeader Overload { get; }

        public WrongArityException(FunctionHeader overload) : base("function was called with wrong number of arguments")
        {
            Overload = overload;
        }
    }

    public class ArgumentTypeMismatchException : Exception
    {
        public FunctionHeader Overload { get; }

        public ArgumentTypeMismatchException(FunctionHeader overload) : base("function was called with wrong arguments")
        {
            Overload = overload;
        }
    }

    public static class FunctionOverloadResolver
    {
        public static (FunctionHeader, IExpression) ResolveConstructor(CcashType type, IExpression argument)
        {
            var resolvedFunction = Resolve(type.Constructors, argument);
            argument = ExpressionFactory.Coerce(argument, resolvedFunction.FunctionType.ParameterTypes.First());

            return (resolvedFunction, argument);

        }

        public static (FunctionHeader, IExpression[]) Resolve(string identifier,
                                                              AbstractScope scope,
                                                              params IExpression[] arguments)
        {
            var overloads = scope.GetFunctionOverloads(identifier);

            var resolvedFunction = Resolve(overloads, arguments);

            arguments = arguments.Zip(resolvedFunction.FunctionType.ParameterTypes, ExpressionFactory.Coerce).ToArray();

            return (resolvedFunction, arguments);
        }

        private static FunctionHeader Resolve<T>(List<T> overloads, params IExpression[] arguments)
            where T : FunctionHeader
        {
            if (overloads.Count == 1)
            {
                var overload = overloads.First();

                if (arguments.Length != overload.FunctionType.ParameterTypes.Count)
                    throw new WrongArityException(overload);

                if (!AreTypesAllCompatible(overload.FunctionType.ParameterTypes, arguments))
                    throw new ArgumentTypeMismatchException(overload);

                return overload;
            }
            var filteredOverloads = FilterOverloads(overloads, arguments).ToList();
            if (!filteredOverloads.Any())
                throw new NoValidOverloadsException();
            

            var scoredOverloads = filteredOverloads
                                  .Select(overload => (overload, ScoreOverload(overload, arguments)))
                                  .ToList();

            var bestScore = scoredOverloads.Max(tuple => tuple.Item2);
            var candidateOverloads = new List<(FunctionHeader, int)>();
            foreach (var (overload, score) in scoredOverloads)
            {
                if (score == bestScore)
                {
                    candidateOverloads.Add((overload, score));
                }
            }

            if (candidateOverloads.Count > 1)
                throw new AmbiguousCallException(filteredOverloads);

            return candidateOverloads.First().Item1;
        }

        private static List<FunctionHeader> FilterOverloads(IEnumerable<FunctionHeader> overloads,
                                                            IExpression[] arguments)
        {
            var filteredOverloads = new List<FunctionHeader>();

            foreach (var overload in overloads)
            {
                if (overload.FunctionType.ParameterTypes.Count == arguments.Length &&
                    AreTypesAllCompatible(overload.FunctionType.ParameterTypes, arguments))
                {
                    filteredOverloads.Add(overload);
                }
            }

            return filteredOverloads;
        }

        private static int ScoreOverload(FunctionHeader overload, IExpression[] arguments)
        {
            var score = arguments.Length * 2;
            for (var i = 0; i < overload.FunctionType.ParameterTypes.Count; i++)
            {
                var paramType = overload.FunctionType.ParameterTypes[i];
                var argument = arguments[i];
                
                var penalty = 0;
                if (paramType != argument.Type)
                {
                    const int dereferencePenalty = 1;
                    const int coercionPenalty = 2;
                    switch (argument.Type)
                    {
                        case ReferenceType refType when paramType is ReferenceType && refType.ReferredType == paramType:
                            penalty = 0;
                            break;
                        case ReferenceType refType when refType.ReferredType == paramType:
                            penalty = dereferencePenalty;
                            break;
                        case ReferenceType _:
                            penalty = dereferencePenalty + coercionPenalty;
                            break;
                        default:
                            penalty = coercionPenalty;
                            break;
                    }
                }

                score -= penalty;
            }

            return score;
        }

        private static bool AreTypesAllCompatible(List<CcashType> paramTypes, IExpression[] arguments)
        {
            for (var i = 0; i < paramTypes.Count; i++)
            {
                var paramType = paramTypes[i];
                var argument = arguments[i];

                
                if (paramType.CanBeCoerced(CcashType.Float64))
                {
                    return true;
                }

                
                if (paramType != argument.Type && !argument.Type.CanBeCoerced(paramType))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

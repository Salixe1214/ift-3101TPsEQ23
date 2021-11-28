using System.Linq;
using Antlr4.Runtime;
using Ccash.SemanticAnalysis.Attributes;
using Ccash.SemanticAnalysis.Nodes.Expressions;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;
using FunctionCallContext = Ccash.Antlr.CcashParser.FunctionCallContext;
using ReassignmentContext = Ccash.Antlr.CcashParser.ReassignmentContext;

namespace Ccash.SemanticAnalysis.Nodes.Statements
{
    [SemanticRule("this.FullFunctionName = resolve(this.Arguments")]
    [SemanticRule("this.FunctionType = resolve(this.Arguments")]
    [SemanticRule("this.AlwaysReturns = false")]
    public class FunctionCallStatement : IStatement
    {
        public bool AlwaysReturns => false;

        public string FunctionName { get; }

        public IExpression[] Arguments { get; }

        public string FullFunctionName { get; }

        public FunctionType FunctionType { get; }

        public FunctionCallStatement(FunctionCallContext context, AbstractScope scope)
        {
            var (header, args) = FunctionCall.ResolveFunction(context, scope);
            Arguments = args;
            FunctionName = header.Name;
            FullFunctionName = header.FullName;
            FunctionType = header.FunctionType;
            
            if (FunctionType.ReturnType != CcashType.Void)
            {
                ErrorManager.AddWarning(context, "unused return value");
            }
        }

        public FunctionCallStatement(ReassignmentContext context, AbstractScope scope)
        {
            var operatorString = context.children[1].GetText();
            FunctionName = $"operator{operatorString}";
            FullFunctionName = FunctionName;
            Arguments = context.expression().Select(e => ExpressionFactory.Create(e, scope)).ToArray();

            if (!CheckReference(ref Arguments[0], context)
                || operatorString == ":=:" && !CheckReference(ref Arguments[1], context))
            {
                return;
            }

            FunctionType = new FunctionType(CcashType.Void, Arguments.Select(a => a.Type).ToArray());
            try
            {
                var (header, arguments) = FunctionOverloadResolver.Resolve(FunctionName, scope, Arguments);
                FullFunctionName = header.FullName;
                Arguments = arguments;
                FunctionType = header.FunctionType;
            }
            catch (FunctionNotFoundException)
            {
                ErrorManager.IdentifierNotInScope(context, FunctionName);
            }
            catch (NoValidOverloadsException)
            {
                ErrorManager.NoValidOverload(context, FunctionName, Arguments);
            }
            catch (AmbiguousCallException e)
            {
                ErrorManager.AmbiguousCall(context, e.Overloads, Arguments);
            }
        }

        public FunctionCallStatement(ParserRuleContext context, AbstractScope scope, IExpression[] args, string functionName)
        {
            var (header, resolvedArgs) = FunctionCall.ResolveFunction(context, scope, args, functionName);
            Arguments = resolvedArgs;
            FunctionName = header.Name;
            FullFunctionName = header.FullName;
            FunctionType = header.FunctionType;
        }

        /// Meant for manually creating function call statements. Be careful, all semantic checks are bypassed.
        public FunctionCallStatement(string functionName, AbstractScope scope, params IExpression[] arguments)
        {
            FunctionName = functionName;
            Arguments = arguments;
            FullFunctionName = FunctionNameMangler.Mangle(functionName, Arguments.Select(a => a.Type));
            FunctionType = (FunctionType) scope[FullFunctionName].Type;
        }

        private static bool CheckReference(ref IExpression expression, ParserRuleContext context)
        {
            if (expression is IdentifierExpression idExpression && idExpression.Type is ValueType valueType)
            {
                expression = new RefExpression(idExpression.Identifier, valueType, idExpression.IsMutable);
            }

            switch (expression.Type)
            {
                case ReferenceType refType:
                {
                    if (refType.IsMutable) 
                        return true;
                    
                    ErrorManager.MismatchedTypes(context, refType.AsMut, refType);
                    return false;

                }
                case ValueType type:
                    ErrorManager.MismatchedTypes(context, type.MutRef, expression.Type);
                    return false;
                default:
                    var operatorString = context.children[1].GetText();
                    ErrorManager.AddError(context,
                                          $"operator `{operatorString}` undefined on type `{expression.Type}`");
                    return false;
            }
        }
    }
}

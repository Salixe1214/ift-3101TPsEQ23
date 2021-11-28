using System.Linq;
using Antlr4.Runtime;
using Ccash.SemanticAnalysis.Attributes;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;
using FunctionCallContext = Ccash.Antlr.CcashParser.FunctionCallContext;

namespace Ccash.SemanticAnalysis.Nodes.Expressions
{
    [SemanticRule("this.FullFunctionName = resolve(this.Arguments")]
    [SemanticRule("this.FunctionType = resolve(this.Arguments")]
    public class FunctionCallExpression : IRvalueExpression
    {
        public string Text => $"{FunctionName}({string.Join(',', Arguments.Select(a => a.Text))})";

        public CcashType Type { get; protected set; }

        public string FunctionName { get; protected set; }

        public IExpression[] Arguments { get; protected set; }

        public string FullFunctionName { get; protected set; }

        public FunctionType FunctionType { get; protected set; }

        protected FunctionCallExpression()
        {
        }

        public FunctionCallExpression(FunctionCallContext context, AbstractScope scope)
        {
            var (header, args) = FunctionCall.ResolveFunction(context, scope);
            Arguments = args;
            FunctionName = header.Name;
            FullFunctionName = header.FullName;
            FunctionType = header.FunctionType;
            
            if (FunctionType == null)
            {
                throw new CannotDetermineTypeException();
            }

            Type = FunctionType.ReturnType;
        }

        public FunctionCallExpression(ParserRuleContext context, AbstractScope scope, IExpression[] args, string functionName)
        {
            var (header, resolvedArgs) = FunctionCall.ResolveFunction(context, scope, args, functionName);
            Arguments = resolvedArgs;
            FunctionName = header.Name;
            FullFunctionName = header.FullName;
            FunctionType = header.FunctionType;
        }

        /// Manually creates an equality operator call.
        public FunctionCallExpression(ParserRuleContext context,
                                      AbstractScope scope,
                                      IdentifierExpression identifier,
                                      IExpression expression)
        {
            FunctionName = "operator==";
            Arguments = new[] {identifier, expression};

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
                FullFunctionName = FunctionName;
                FunctionType = new FunctionType(CcashType.Boolean, Arguments.Select(a => a.Type).ToArray());
                Type = FunctionType.ReturnType;
            }
            catch (AmbiguousCallException e)
            {
                ErrorManager.AmbiguousCall(context, e.Overloads, Arguments);
                FullFunctionName = FunctionName;
                FunctionType = new FunctionType(CcashType.Boolean, Arguments.Select(a => a.Type).ToArray());
                Type = FunctionType.ReturnType;
            }
        }

        /// Meant for manually creating function call expressions. Doesn't swallow exceptions from the FunctionOverloadResolver.
        public FunctionCallExpression(string functionName, AbstractScope scope, params IExpression[] arguments)
        {
            FunctionName = functionName;
            var (header, coercedArgs) = FunctionOverloadResolver.Resolve(FunctionName, scope, arguments);
            FunctionType = header.FunctionType;
            FullFunctionName = header.FullName;
            Type = FunctionType.ReturnType;
            Arguments = coercedArgs;
        }

        /// Meant for manually creating constructor calls. Doesn't swallow exceptions from the FunctionOverloadResolver.
        public FunctionCallExpression(CcashType type, IExpression argument)
        {
            var (header, coercedArg) = FunctionOverloadResolver.ResolveConstructor(type, argument);
            FunctionName = header.Name;
            FunctionType = header.FunctionType;
            FullFunctionName = header.FullName;
            Type = FunctionType.ReturnType;
            Arguments = new[] {coercedArg};
        }
    }
}

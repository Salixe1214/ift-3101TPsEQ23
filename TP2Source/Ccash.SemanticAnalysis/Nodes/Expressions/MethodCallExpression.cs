using System.Linq;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;
using MethodCallExpressionContext = Ccash.Antlr.CcashParser.MethodCallExpressionContext;
using ExpressionContext = Ccash.Antlr.CcashParser.ExpressionContext;

namespace Ccash.SemanticAnalysis.Nodes.Expressions
{
    public class MethodCallExpression: IRvalueExpression
    {
        public CcashType Type { get; }
        
        public FunctionCallExpression FunctionCall { get; }
        
        public StructType StructType { get; }

        private IExpression ThisExpression { get; }

        public string Text { get; }
        
        public MethodCallExpression(MethodCallExpressionContext context, AbstractScope scope)
        {
            ThisExpression = ExpressionFactory.Create(context.expression(), scope);
            ThisExpression = ExpressionFactory.CoerceStructToRef(ThisExpression, scope);

            if (ThisExpression.Type is ReferenceType refType && refType.ReferredType is StructType type)
            {
                StructType = type;
            }
            else
            {
                ErrorManager.AddError(context,
                                      $"trying to call method on non struct or class type `{ThisExpression.Type}`");
            }
            
            var argContexts = context.functionArgs()?.expression() ?? Enumerable.Empty<ExpressionContext>();
            var arguments = new[] {ThisExpression}.Concat(argContexts.Select(e => ExpressionFactory.Create(e, scope))).ToArray();
            
            var methodName = context.Identifier().GetText();
            var functionName = $"{StructType}::{methodName}";

            FunctionCall = new FunctionCallExpression(context, StructType.ModuleScope, arguments, functionName);
            if (FunctionCall.FunctionType == null)
            {
                throw new CannotDetermineTypeException();
            }

            Type = FunctionCall.Type;
            Text = context.GetText();
        }
    }
}

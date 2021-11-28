using System.Linq;
using Ccash.Antlr;
using Ccash.SemanticAnalysis.Nodes.Expressions;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;
using MethodCallStatementContext = Ccash.Antlr.CcashParser.MethodCallStatementContext;

namespace Ccash.SemanticAnalysis.Nodes.Statements
{
    public class MethodCallStatement : IStatement
    {
        public bool AlwaysReturns => false;
        
        public FunctionCallStatement FunctionCall { get; }
        
        public StructType StructType { get; }

        private IExpression ThisExpression { get; }

        public MethodCallStatement(MethodCallStatementContext context, AbstractScope scope)
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
            
            var argContexts = context.functionArgs()?.expression() ?? Enumerable.Empty<CcashParser.ExpressionContext>();
            var arguments = new[] {ThisExpression}.Concat(argContexts.Select(e => ExpressionFactory.Create(e, scope))).ToArray();
            
            var methodName = context.Identifier().GetText();
            var functionName = $"{StructType}::{methodName}";
            
            FunctionCall = new FunctionCallStatement(context, StructType.ModuleScope, arguments, functionName);

            if (FunctionCall.FunctionType.ReturnType != CcashType.Void)
            {
                ErrorManager.AddWarning(context, "unused return value");
            }
        }
    }
}

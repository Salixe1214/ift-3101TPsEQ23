using System.Linq;
using Ccash.SemanticAnalysis.Attributes;
using Ccash.SemanticAnalysis.Nodes.Expressions;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;
using RepeatStatementContext = Ccash.Antlr.CcashParser.RepeatStatementContext;
using RepeatHeaderContext = Ccash.Antlr.CcashParser.RepeatHeaderContext;

namespace Ccash.SemanticAnalysis.Nodes.Statements.Loops
{
    [SemanticRule("this.ConditionBlock = codegenblock()")]
    [SemanticRule("this.NextBlock = codegenblock()")]
    [SemanticRule("Statements.inherited = {this.NextBlock, this.ConditionBlock, ..this.inherited}")]
    [SemanticRule("this.AlwaysReturns = false")]
    public class RepeatStatement : LoopStatement
    {
        public IStatement Declaration { get; }

        public IStatement Assignment { get; }

        public RepeatStatement(RepeatStatementContext context, AbstractScope parent, InheritedAttributes inheritedAttributes) :
            base(parent)
        {
            var expr = context.repeatHeader().expression();
            ConditionExpression = ExpressionFactory.Create(expr, this);
            
            if (context.repeatHeader().expression() != null && (ConditionExpression.Type.CanBeCoerced(CcashType.Uint64)))
            {
                ConditionExpression = ExpressionFactory.Coerce(ConditionExpression, CcashType.Uint64);
            }
            else
            {
                ErrorManager.MismatchedTypes(context.repeatHeader(), CcashType.Uint64, ConditionExpression.Type);
            }
            var childrenAttributes = inheritedAttributes.WithConditionBlock(ConditionBlock).WithNextBlock(NextBlock);
            Statements = context.loopBlock()
                                .statement()
                                .Select(s => StatementFactory.Create(s, this, childrenAttributes))
                                .ToList();
        }
    }
}

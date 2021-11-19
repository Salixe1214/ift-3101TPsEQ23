using System.Linq;
using Ccash.SemanticAnalysis.Attributes;
using Ccash.SemanticAnalysis.Nodes.Expressions;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;
using WhileStatementContext = Ccash.Antlr.CcashParser.WhileStatementContext;

namespace Ccash.SemanticAnalysis.Nodes.Statements.Loops
{
    [SemanticRule("this.ConditionBlock = codegenblock()")]
    [SemanticRule("this.NextBlock = codegenblock()")]
    [SemanticRule("Statements.inherited = {this.NextBlock, this.ConditionBlock, ..this.inherited}")]
    [SemanticRule("this.AlwaysReturns = false")]
    public class WhileStatement : LoopStatement
    {
        public WhileStatement(WhileStatementContext context, AbstractScope parent, InheritedAttributes inheritedAttributes) :
            base(parent)
        {
            ConditionExpression = ExpressionFactory.Create(context.whileHeader().expression(), this);
            if (ConditionExpression.Type.CanBeCoerced(CcashType.Boolean))
            {
                ConditionExpression = ExpressionFactory.Coerce(ConditionExpression, CcashType.Boolean);
            }
            else
            {
                ErrorManager.MismatchedTypes(context.whileHeader(), CcashType.Boolean, ConditionExpression.Type);
            }
            Block.Append(inheritedAttributes.NextBlock.Data);
            var childrenAttributes = inheritedAttributes.WithConditionBlock(ConditionBlock).WithNextBlock(NextBlock);
            Statements = context.loopBlock()
                                .statement()
                                .Select(s => StatementFactory.Create(s, this, childrenAttributes))
                                .ToList();
        }
    }
}

using System.Linq;
using Ccash.SemanticAnalysis.Attributes;
using Ccash.SemanticAnalysis.SymbolTable;
using BlockContext = Ccash.Antlr.CcashParser.BlockContext;

namespace Ccash.SemanticAnalysis.Nodes.Statements
{
    [SemanticRule("Statements.inherited = this.inherited")]
    [SemanticRule("this.AlwaysReturns = Statements.AlwaysReturns")]
    public class BlockStatement : BlockScope, IStatement
    {
        public bool AlwaysReturns { get; }

        public BlockStatement(BlockContext context, AbstractScope parent, InheritedAttributes inheritedAttributes) :
            base(parent)
        {
            Statements = context.statement()?.Select(s => StatementFactory.Create(s, this, inheritedAttributes)).ToList();

            AlwaysReturns = Statements.Any(s => s.AlwaysReturns);
            if (AlwaysReturns && Statements.First(s => s.AlwaysReturns) != Statements.Last())
            {
                ErrorManager.AddError(context, "Unreachable code after `return`");
            }
        }
    }
}

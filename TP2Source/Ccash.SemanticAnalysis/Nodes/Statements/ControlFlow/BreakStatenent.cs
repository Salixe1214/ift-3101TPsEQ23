using System.Linq;
using Ccash.SemanticAnalysis.Attributes;
using Ccash.SemanticAnalysis.Nodes.Expressions;
using Ccash.SemanticAnalysis.SymbolTable;
using BreakStatementContext = Ccash.Antlr.CcashParser.BreakStatementContext;

namespace Ccash.SemanticAnalysis.Nodes.Statements.ControlFlow
{
    [SemanticRule("this.AlwaysReturns = false")]
    public class BreakStatement : IStatement
    {
        public BreakStatementContext Context { get; }

        public bool AlwaysReturns => false;

        public BlockScope parent { get; }

        public InheritedAttributes BlockSuivant { get; }

        public BreakStatement()
        {
        }
        
        public BreakStatement(BreakStatementContext context, AbstractScope scope, InheritedAttributes attr)
        {
            
            var f = scope.Enclosing<Loops.LoopStatement>().Last().ia;
            BlockSuivant = attr;
        }
    }
}

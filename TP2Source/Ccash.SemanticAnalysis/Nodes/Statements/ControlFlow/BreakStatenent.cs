using System.Linq;
using Ccash.SemanticAnalysis.Attributes;
using Ccash.SemanticAnalysis.Nodes.Expressions;
using Ccash.SemanticAnalysis.SymbolTable;
using BreakStatementContext = Ccash.Antlr.CcashParser.BreakStatementContext;

namespace Ccash.SemanticAnalysis.Nodes.Statements.ControlFlow
{
    [SemanticRule("this.AlwaysReturns = true")]
    public class BreakStatement : IStatement
    {
        public BreakStatementContext Context { get; }

        public bool AlwaysReturns => true;

        public BreakStatement()
        {
        }
        
        public BreakStatement(BreakStatementContext context, AbstractScope scope)
        {
            Context = context;
        }
    }
}

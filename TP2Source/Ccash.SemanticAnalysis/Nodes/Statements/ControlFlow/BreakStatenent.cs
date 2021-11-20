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
        public bool AlwaysReturns => false;

        public CodeGeneratorAttribute NextBranch { get; } = new CodeGeneratorAttribute();

        public string label { get; } = null;

        public object f { get; }

        public BreakStatement()
        {
        }
        
        public BreakStatement(BreakStatementContext context, AbstractScope scope, InheritedAttributes inheritedAttributes)
        {
            NextBranch = inheritedAttributes.NextBlock;
            label = context.Identifier()?.GetText();
            f = scope.Enclosing<Loops.LoopStatement>();
        }
    }
}

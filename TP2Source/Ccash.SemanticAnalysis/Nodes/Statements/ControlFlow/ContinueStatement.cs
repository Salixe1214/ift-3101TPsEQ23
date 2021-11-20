using System.Linq;
using Ccash.SemanticAnalysis.Attributes;
using Ccash.SemanticAnalysis.Nodes.Expressions;
using Ccash.SemanticAnalysis.SymbolTable;
using ContinueStatementContext = Ccash.Antlr.CcashParser.ContinueStatementContext;

namespace Ccash.SemanticAnalysis.Nodes.Statements.ControlFlow
{
    [SemanticRule("this.AlwaysReturns = false")]
    public class ContinueStatement : IStatement
    {
        public bool AlwaysReturns => false;

        public CodeGeneratorAttribute CondBranch { get; } = new CodeGeneratorAttribute();

        public InheritedAttributes inher { get; }

        public string label { get; } = null;

        public object f { get; }

        public ContinueStatement()
        {
        }
        
        public ContinueStatement(ContinueStatementContext context, AbstractScope scope, InheritedAttributes inheritedAttributes)
        {
            CondBranch = inheritedAttributes.ConditionBlock;
            inher = inheritedAttributes;
            label = context.Identifier()?.GetText();
            f = scope.Enclosing<Loops.LoopStatement>();
        }
    }
}

using System.Linq;
using Ccash.SemanticAnalysis.Attributes;
using Ccash.SemanticAnalysis.Nodes.Expressions;
using Ccash.SemanticAnalysis.SymbolTable;
using FallthroughStatementContext = Ccash.Antlr.CcashParser.FallthroughStatementContext;

namespace Ccash.SemanticAnalysis.Nodes.Statements.ControlFlow
{
    [SemanticRule("this.AlwaysReturns = false")]
    public class FallthroughStatement : IStatement
    {
        public bool AlwaysReturns => false;

        public CodeGeneratorAttribute NextBranch { get; } = new CodeGeneratorAttribute();

        public object f { get; }

        public FallthroughStatement()
        {
        }
        
        public FallthroughStatement(FallthroughStatementContext context, AbstractScope scope, InheritedAttributes inheritedAttributes)
        {
            NextBranch.Data = inheritedAttributes.NextBlock.Data;
            f = scope.Enclosing<CaseStatement>();
        }
    }
}

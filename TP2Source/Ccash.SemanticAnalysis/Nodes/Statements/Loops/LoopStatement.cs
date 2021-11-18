using System.Linq;
using Antlr4.Runtime;
using Ccash.SemanticAnalysis.Attributes;
using Ccash.SemanticAnalysis.Nodes.Expressions;
using Ccash.SemanticAnalysis.SymbolTable;

namespace Ccash.SemanticAnalysis.Nodes.Statements.Loops
{
    public abstract class LoopStatement : BlockScope, IStatement
    {
        public bool AlwaysReturns => false;

        public IExpression ConditionExpression { get; protected set; }

        public CodeGeneratorAttribute ConditionBlock { get; } = new CodeGeneratorAttribute();

        public CodeGeneratorAttribute NextBlock { get; } = new CodeGeneratorAttribute();

        public CodeGeneratorAttribute BodyBlock { get; } = new CodeGeneratorAttribute();

        protected LoopStatement(AbstractScope parent) : base(parent)
        {
        }
    }
}

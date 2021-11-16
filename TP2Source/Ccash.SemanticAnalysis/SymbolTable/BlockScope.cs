using System.Collections.Generic;
using Ccash.SemanticAnalysis.Nodes.Statements;

namespace Ccash.SemanticAnalysis.SymbolTable
{
    public abstract class BlockScope : Scope
    {
        public List<IStatement> Statements { get; protected set; } = new List<IStatement>();

        protected BlockScope(AbstractScope parent) : base(parent)
        {
        }
    }
}

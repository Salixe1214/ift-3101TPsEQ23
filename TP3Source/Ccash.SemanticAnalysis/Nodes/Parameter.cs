using Ccash.SemanticAnalysis.Types;
using VariableContext = Ccash.Antlr.CcashParser.VariableContext;

namespace Ccash.SemanticAnalysis.Nodes
{
    public struct Parameter
    {
        public string Name { get; }
        public CcashType Type { get; }
        public bool IsMutable { get; }

        public Parameter(VariableContext context)
        {
            Name = context.Identifier().GetText();
            Type = CcashType.Create(context);
            IsMutable = context.variableType().Mut() != null;
        }

        public Parameter(string name, CcashType type, bool isMutable)
        {
            Name = name;
            Type = type;
            IsMutable = isMutable;
        }
    }
}

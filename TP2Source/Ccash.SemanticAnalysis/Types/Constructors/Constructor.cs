using Ccash.SemanticAnalysis.Nodes;

namespace Ccash.SemanticAnalysis.Types.Constructors
{
    public class Constructor : FunctionHeader
    {
        public Constructor(CcashType type, params CcashType[] paramTypes) : base(type.Name, type, paramTypes)
        {
        }
    }
}

namespace Ccash.SemanticAnalysis.Types.Constructors
{
    public class ImplicitConstructor: Constructor
    {
        public ImplicitConstructor(CcashType type, params CcashType[] paramTypes) : base(type, paramTypes)
        {
        }
    }
}

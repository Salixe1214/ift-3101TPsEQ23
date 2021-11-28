namespace Ccash.SemanticAnalysis.Types
{
    public abstract class ValueType: CcashType
    {
        public ReferenceType ConstRef => new ReferenceType(this, false);

        public ReferenceType MutRef => new ReferenceType(this, true);
    }
}

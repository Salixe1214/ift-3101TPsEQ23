using System.Collections.Generic;
using Ccash.SemanticAnalysis.Types.Constructors;

namespace Ccash.SemanticAnalysis.Types
{
    public class ReferenceType : CcashType
    {
        public ValueType ReferredType { get; }

        public bool IsMutable { get; }

        public override string Name => $"{(IsMutable ? "mut" : "const")} ref {ReferredType.Name}";

        public override List<Constructor> Constructors => new List<Constructor>();
        
        public ReferenceType AsConst => new ReferenceType(ReferredType, false);
        
        public ReferenceType AsMut => new ReferenceType(ReferredType, true);

        public ReferenceType(ValueType referredType, bool isMutable)
        {
            ReferredType = referredType;
            IsMutable = isMutable;
        }

        public bool CanBeCoercedInMutability(ReferenceType referenceType)
        {
            return IsMutable == referenceType.IsMutable || IsMutable;
        }

        public override bool CanBeCoerced(CcashType destination)
        {
            if (destination is ReferenceType otherRefType && ReferredType == otherRefType.ReferredType)
            {
                return CanBeCoercedInMutability(otherRefType);
            }
            
            return ReferredType.CanBeCoerced(destination);
        }
        
        public override bool Equals(CcashType other)
        {
            return base.Equals(other)
                   && other is ReferenceType otherRef
                   && ReferredType == otherRef.ReferredType
                   && IsMutable == otherRef.IsMutable;
        }
    }
}

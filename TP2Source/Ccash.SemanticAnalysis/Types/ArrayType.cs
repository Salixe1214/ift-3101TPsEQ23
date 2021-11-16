using System.Collections.Generic;
using Ccash.SemanticAnalysis.Types.Constructors;

namespace Ccash.SemanticAnalysis.Types
{
    public class ArrayType : StructType
    {
        public CcashType ContainedType { get; }

        public bool IsMutable { get; }

        public override string Name => $"{(IsMutable ? "mut" : "const")} []{StripMutabilityQualifiers(ContainedType)}";

        public override List<Constructor> Constructors => new List<Constructor>();

        public ArrayType(CcashType containedType, bool isMutable)
        {
            ContainedType = containedType;
            IsMutable = isMutable;
            Fields.Add(new Field("length", Uint32));
        }

        public override bool CanBeCoerced(CcashType destination)
        {
            if (!(destination is ArrayType))
                return false;

            var destinationArrayType = (ArrayType) destination;
            if (IsMutable != destinationArrayType.IsMutable && !IsMutable)
            {
                return false;
            }
            
            if (ContainedType is ArrayType && destinationArrayType.ContainedType is ArrayType)
            {
                return ContainedType.CanBeCoerced(destinationArrayType.ContainedType);
            }

            if (ContainedType is ReferenceType containedRef && destinationArrayType.ContainedType is ReferenceType destinationRef)
            {
                return containedRef.CanBeCoercedInMutability(destinationRef);
            }

            return ContainedType == destinationArrayType.ContainedType;
        }

        public override bool Equals(CcashType other)
        {
            return base.Equals(other)
                   && other is ArrayType otherArray
                   && ContainedType == otherArray.ContainedType
                   && IsMutable == otherArray.IsMutable;
        }

        private static string StripMutabilityQualifiers(CcashType type)
        {
            switch (type)
            {
                case ReferenceType referenceType:
                    return $"ref {referenceType.ReferredType}";
                case ArrayType arrayType:
                    return $"[]{StripMutabilityQualifiers(arrayType.ContainedType)}";
                default:
                    return type.Name;
            }
        }
    }
}

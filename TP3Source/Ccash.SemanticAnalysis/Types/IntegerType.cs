using System.Collections.Generic;
using Ccash.SemanticAnalysis.Types.Constructors;

namespace Ccash.SemanticAnalysis.Types
{
    public class IntegerType : ValueType
    {
        public override string Name => $"{(IsSigned ? "" : "u")}int{Size}";

        private List<Constructor> _constructors;
        
        public override List<Constructor> Constructors
        {
            get
            {
                if (_constructors == null)
                {
                    _constructors = GenerateConstructors();
                }

                return _constructors;
            }
        }

        public uint Size { get; }

        public bool IsSigned { get; }

        public IntegerType(string name)
        {
            IsSigned = !name.StartsWith('u');
            Size = uint.TryParse(name.Substring(name.Length - 2), out var size) ? size : 8;
        }

        public IntegerType(uint size, bool isSigned)
        {
            Size = size;
            IsSigned = isSigned;
        }

        public override bool Equals(CcashType other)
        {
            return base.Equals(other)
                   && other is IntegerType otherInt
                   && Size == otherInt.Size
                   && IsSigned == otherInt.IsSigned;
        }

        private List<Constructor> GenerateConstructors()
        {
            var constructors = new List<Constructor>();
            foreach (var primitive in AllPrimitives)
            {
                Constructor ctor;
                if (primitive is IntegerType intArgument)
                {
                    ctor = NewIntConstructor(intArgument);
                }
                else
                {
                    ctor = new Constructor(this, primitive);
                }

                constructors.Add(ctor);
            }

            return constructors;
        }

        private Constructor NewIntConstructor(IntegerType intArgument)
        {
            if (IsSigned == intArgument.IsSigned && Size >= intArgument.Size)
            {
                return new ImplicitConstructor(this, intArgument);
            }

            if (IsSigned && !intArgument.IsSigned && Size > intArgument.Size)
            {
                return new ImplicitConstructor(this, intArgument);
            }

            return new Constructor(this, intArgument);
        }
    }
}

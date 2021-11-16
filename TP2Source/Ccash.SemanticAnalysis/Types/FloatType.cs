using System.Collections.Generic;
using Ccash.SemanticAnalysis.Types.Constructors;

namespace Ccash.SemanticAnalysis.Types
{
    public class FloatType : ValueType
    {
        public override string Name => $"float{Size}";

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

        public FloatType(string name)
        {
            Size = uint.TryParse(name.Substring(name.Length - 2), out var size) ? size : 32;
        }

        public FloatType(uint size)
        {
            Size = size;
        }

        public override bool Equals(CcashType other)
        {
            return base.Equals(other)
                   && other is FloatType otherFloat
                   && Size == otherFloat.Size;
        }

        private List<Constructor> GenerateConstructors()
        {
            var constructors = new List<Constructor>();
            foreach (var primitive in AllPrimitives)
            {
                Constructor ctor;
                switch (primitive)
                {
                    case FloatType floatArgument:
                        ctor = NewFloatConstructor(floatArgument);
                        break;
                    default:
                        ctor = new Constructor(this, primitive);
                        break;
                }

                constructors.Add(ctor);
            }

            return constructors;
        }

        private Constructor NewFloatConstructor(FloatType floatArgument)
        {
            if (Size >= floatArgument.Size)
            {
                return new ImplicitConstructor(this, floatArgument);
            }

            return new Constructor(this, floatArgument);
        }
    }
}

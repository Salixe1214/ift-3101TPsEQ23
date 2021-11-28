using System.Collections.Generic;
using Ccash.SemanticAnalysis.Nodes.Expressions.Literals;
using Ccash.SemanticAnalysis.Types.Constructors;

namespace Ccash.SemanticAnalysis.Types
{
    public class BooleanType : ValueType
    {
        public static BooleanLiteralExpression True => new BooleanLiteralExpression(true);
        public static BooleanLiteralExpression False => new BooleanLiteralExpression(false);
        
        public override string Name => "bool";

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

        private List<Constructor> GenerateConstructors()
        {
            var constructors = new List<Constructor>();
            foreach (var primitive in AllPrimitives)
            {
                Constructor ctor;
                if (primitive is BooleanType booleanType)
                {
                    ctor = new ImplicitConstructor(this, booleanType);
                }
                else
                {
                    ctor = new Constructor(this, primitive);
                }

                constructors.Add(ctor);
            }

            return constructors;
        }
    }
}

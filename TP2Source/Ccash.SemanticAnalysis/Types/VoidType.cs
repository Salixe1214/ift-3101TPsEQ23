using System.Collections.Generic;
using Ccash.SemanticAnalysis.Types.Constructors;

namespace Ccash.SemanticAnalysis.Types
{
    public class VoidType : ValueType
    {
        public override string Name => "void";

        public override List<Constructor> Constructors => new List<Constructor>();
    }
}

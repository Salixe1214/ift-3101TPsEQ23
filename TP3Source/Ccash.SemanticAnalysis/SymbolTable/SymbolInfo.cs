using Ccash.SemanticAnalysis.Attributes;
using Ccash.SemanticAnalysis.Types;

namespace Ccash.SemanticAnalysis.SymbolTable
{
    public class SymbolInfo
    {
        public CcashType Type { get; }
        
        public bool IsMutable { get; }
        
        public bool IsStructField { get; }

        public CodeGeneratorAttribute CodeGeneratorAttribute { get; } = new CodeGeneratorAttribute();

        public SymbolInfo(CcashType type, bool isMutable = false, bool isStructField = false)
        {
            Type = type;
            IsMutable = isMutable;
            IsStructField = isStructField;
        }
    }
}

using Ccash.SemanticAnalysis.Types;
using LLVMSharp;

namespace Ccash.CodeGeneration.LLVMGenerator.Intrinsics
{
    public struct Arg
    {
        public LLVMValueRef Value { get; }
        public CcashType Type { get; }

        public Arg(LLVMValueRef value, CcashType type)
        {
            Value = value;
            Type = type;
        }
    }
}

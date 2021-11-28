using Ccash.SemanticAnalysis.Types;
using FunctionHeaderContext = Ccash.Antlr.CcashParser.FunctionHeaderContext;
using MethodHeaderContext = Ccash.Antlr.CcashParser.MethodHeaderContext;

namespace Ccash.SemanticAnalysis.Nodes
{
    public class FunctionHeader
    {
        public string Name { get; }

        public string FullName => FunctionNameMangler.Mangle(Name, FunctionType);

        public FunctionType FunctionType { get; }

        public FunctionHeader(FunctionHeaderContext context)
        {
            Name = context.Identifier().GetText();
            FunctionType = new FunctionType(context);
        }

        public FunctionHeader(StructType ownerType, MethodHeaderContext context)
        {
            Name = $"{ownerType}::{context.Identifier().GetText()}";
            FunctionType = new FunctionType(ownerType, context);
        }

        public FunctionHeader(string name, CcashType returnType, params CcashType[] paramTypes)
        {
            Name = name;
            FunctionType = new FunctionType(returnType, paramTypes);
        }
    }
}

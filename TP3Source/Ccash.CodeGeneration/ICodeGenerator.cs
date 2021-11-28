using Ccash.SemanticAnalysis.Nodes;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;

namespace Ccash.CodeGeneration
{
    public interface ICodeGenerator
    {
        void WriteToFile(string fileName);

        void Generate(FunctionHeader functionHeader, AbstractScope moduleScope);

        void Generate(FunctionDeclaration functionDeclaration, AbstractScope moduleScope);

        void RegisterStructName(string structName);

        void Generate(StructType structType);
    }
}
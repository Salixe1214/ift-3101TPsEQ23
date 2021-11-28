using System.Collections.Generic;
using System.Linq;
using Ccash.Antlr;
using Ccash.CodeGeneration;
using Ccash.SemanticAnalysis;
using Ccash.SemanticAnalysis.Nodes;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;
using FunctionHeaderContext = Ccash.Antlr.CcashParser.FunctionHeaderContext;
using CompilationUnitContext = Ccash.Antlr.CcashParser.CompilationUnitContext;

namespace Ccash
{
    public class GlobalModuleListener<T> : CcashBaseListener where T : ICodeGenerator, new()
    {
        public ModuleScope GlobalModuleScope { get; }

        public T CodeGenerator { get; }
        
        public GlobalModuleListener()
        {
            CodeGenerator = new T();
            GlobalModuleScope = new ModuleScope();
            
            foreach (var type in CcashType.AllPrimitives)
            {
                type.Constructors.ForEach(GlobalModuleScope.AddFunction);
            }
            
            foreach (var op in CcashType.NativeOperators)
            {
                GlobalModuleScope.AddFunction(op);
            }
        }

        public override void EnterCompilationUnit(CompilationUnitContext context)
        {
            var structCounts = new Dictionary<string, uint>();
            foreach (var structDeclaration in context.structDeclaration())
            {
                var structType = new StructType(structDeclaration.Identifier().GetText(), GlobalModuleScope);
                if (CcashType.Structs.ContainsKey(structType.Name))
                {
                    structCounts[structType.Name]++;
                    ErrorManager.AddError(structDeclaration, "duplicate type definition");
                }
                else
                {
                    structCounts.Add(structType.Name, 0);
                    CcashType.Structs.Add(structType.Name, structType);
                    CodeGenerator.RegisterStructName(structType.Name);
                }
            }
            
            foreach (var structDeclaration in context.structDeclaration().Reverse())
            {
                var structType = CcashType.Structs[structDeclaration.Identifier().GetText()];
                if (structCounts[structType.Name] > 1)
                {
                    structCounts[structType.Name]--;
                }
                else
                {
                    structType.AddMembers(structDeclaration);
                    CodeGenerator.Generate(structType);
                }
            }
        }

        public override void EnterFunctionHeader(FunctionHeaderContext context)
        {
            var header = new FunctionHeader(context);
            try
            {
                GlobalModuleScope.AddFunction(header);
                CodeGenerator.Generate(header, GlobalModuleScope);
            }
            catch (DuplicateSymbolException)
            {
                var details = $"with parameters ({string.Join(", ", header.FunctionType.ParameterTypes)})";
                ErrorManager.AddError(context, "duplicate function definition", details);
            }
        }
    }
}

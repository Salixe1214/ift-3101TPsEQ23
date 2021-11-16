using System.Linq;
using Ccash.Antlr;
using Ccash.CodeGeneration;
using Ccash.SemanticAnalysis;
using Ccash.SemanticAnalysis.Nodes;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;
using FunctionDeclarationContext = Ccash.Antlr.CcashParser.FunctionDeclarationContext;
using CompilationUnitContext = Ccash.Antlr.CcashParser.CompilationUnitContext;

namespace Ccash
{
    public class CompilationListener : CcashBaseListener
    {
        private ModuleScope GlobalModuleScope { get; }

        private ICodeGenerator CodeGenerator { get; }

        public CompilationListener(ModuleScope globalModuleScope, ICodeGenerator codeGenerator)
        {
            GlobalModuleScope = globalModuleScope;
            CodeGenerator = codeGenerator;
        }

        public override void ExitFunctionDeclaration(FunctionDeclarationContext context)
        {
            var functionDeclaration = new FunctionDeclaration(context, GlobalModuleScope);
            if (!ErrorManager.HasErrors)
            {
                CodeGenerator.Generate(functionDeclaration, GlobalModuleScope);
            }
        }
        
        public override void ExitCompilationUnit(CompilationUnitContext context)
        {
            foreach (var structDeclaration in context.structDeclaration().Reverse())
            {
                var structType = CcashType.Structs[structDeclaration.Identifier().GetText()];
                foreach (var methodContext in structDeclaration.method())
                {
                    FunctionDeclaration functionDeclaration;
                    if (methodContext.methodHeader().Static() == null)
                    {
                        functionDeclaration = new FunctionDeclaration(structType, methodContext);
                    }
                    else
                    {
                        functionDeclaration = new FunctionDeclaration(structType, methodContext, GlobalModuleScope);
                    }
                    
                    if (!ErrorManager.HasErrors)
                    {
                        CodeGenerator.Generate(functionDeclaration, structType.ModuleScope);
                    }
                }
            }
        }

    }
}

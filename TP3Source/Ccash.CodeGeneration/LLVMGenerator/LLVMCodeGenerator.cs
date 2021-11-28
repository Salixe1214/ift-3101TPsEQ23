using System;
using System.IO;
using System.Linq;
using System.Text;
using Ccash.SemanticAnalysis.Nodes;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;
using LLVMSharp;

namespace Ccash.CodeGeneration.LLVMGenerator
{
    public class LLVMCodeGenerator : ICodeGenerator
    {
        private LLVMModuleRef Module { get; } = LLVM.ModuleCreateWithName("global_module");

        private IRBuilder Builder { get; } = new IRBuilder();

        private StatementGenerator StatementGenerator { get; }

        public LLVMCodeGenerator()
        {
            var charPointerType = LLVM.PointerType(LLVMTypeRef.Int8Type(), 0);
            var printfType = LLVM.FunctionType(LLVM.Int32Type(), new[] {charPointerType}, true);
            var printf = LLVM.AddFunction(Module, "printf", printfType);
            LLVM.SetLinkage(printf, LLVMLinkage.LLVMExternalLinkage);

            StatementGenerator = new StatementGenerator(Builder, printf);
        }

        public void WriteToFile(string fileName)
        {
            LLVM.PrintModuleToFile(Module, fileName + ".ll", out _);
            LLVM.WriteBitcodeToFile(Module, fileName + ".bc");
        }

        public void RegisterStructName(string structName)
        {
            LLVMContextRef context = LLVM.GetModuleContext(Module);
            LLVMTypeRef structType = LLVM.StructCreateNamed(context, structName);
            TypeResolver.RegisterStruct(structName, structType);
        }

        public void Generate(StructType structType)
        {
            LLVMTypeRef llvmStruct = TypeResolver.Resolve(structType);
            LLVMTypeRef[] memberTypes = structType.Fields.Select(f => TypeResolver.Resolve(f.Type)).ToArray();
            llvmStruct.StructSetBody(memberTypes, false);
            
            foreach (var structMethod in structType.Methods)
            {
                var firstArg = structMethod.FunctionType.ParameterTypes.FirstOrDefault();
                if (firstArg != null && firstArg is ReferenceType refType && refType.ReferredType == structType)
                {
                    Generate(structMethod, structType.ModuleScope);
                }
                else
                {
                    Generate(structMethod, structType.ModuleScope.Parent);
                }
            }
        }

        public void Generate(FunctionHeader functionHeader, AbstractScope moduleScope)
        {
            var type = functionHeader.FunctionType;
            var returnType = TypeResolver.Resolve(type.ReturnType);
            var parameterTypes = type.ParameterTypes.Select(TypeResolver.Resolve).ToArray();

            var functionType = LLVM.FunctionType(returnType, parameterTypes, false);
            var function = LLVM.AddFunction(Module, functionHeader.FullName, functionType);

            moduleScope[functionHeader.FullName].CodeGeneratorAttribute.Data = function;
        }

        public void Generate(FunctionDeclaration functionDeclaration, AbstractScope moduleScope)
        {
            var functionRef = moduleScope.GetCodeGeneratorData<LLVMValueRef>(functionDeclaration.Header.FullName);
            var entry = LLVM.AppendBasicBlock(functionRef, "entry");
            Builder.PositionAtEnd(entry);

            for (var i = 0; i < functionDeclaration.Parameters.Count; i++)
            {
                var parameter = functionDeclaration.Parameters[i];

                LLVMTypeRef type = TypeResolver.Resolve(parameter.Type);
                LLVMValueRef llvmParam = LLVM.GetParam(functionRef, (uint) i);
                
                if (parameter.Type is ReferenceType)
                {
                    llvmParam.SetValueName(parameter.Name);
                    functionDeclaration.GetSymbolInfo(parameter.Name).CodeGeneratorAttribute.Data = llvmParam;
                }
                else
                {
                    LLVMValueRef paramVariable = Builder.AllocateVariable(type, parameter.Name);
                    LLVM.SetValueName(paramVariable, parameter.Name);
                    Builder.Store(llvmParam, paramVariable);
                    functionDeclaration.GetSymbolInfo(parameter.Name).CodeGeneratorAttribute.Data = paramVariable;
                }
            }

            foreach (var statement in functionDeclaration.Statements)
            {
                StatementGenerator.Generate(statement, functionDeclaration);
            }

            var result = LLVM.VerifyFunction(functionRef, LLVMVerifierFailureAction.LLVMPrintMessageAction);
            if (result.Value != 0)
            {
                var fileName = new StringBuilder($"fn_{functionDeclaration.Header.FullName}.ll");
                fileName.Replace('>', '_');
                fileName.Replace("::<", "_");
                File.WriteAllText(fileName.ToString(), functionRef.ToString());
                Console.WriteLine($"\n  In function {functionDeclaration.Header.FullName}, IR dumped in {fileName}");
            }

            StatementGenerator.ResetCounters();
        }
    }
}

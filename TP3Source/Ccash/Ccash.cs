using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Ccash.Antlr;
using Ccash.CodeGeneration.LLVMGenerator;
using Ccash.SemanticAnalysis;
using Ccash.SemanticAnalysis.Nodes.Expressions;

namespace Ccash
{
    public static class Ccash
    {
        public static void Main(string[] args)
        {
            var syntaxErrorListener = new SyntaxErrorListener();
            
            var inputFilePath = args[0];
            ErrorManager.CurrentFileName = inputFilePath;

            var file = new AntlrFileStream(inputFilePath);
            var lexer = new CcashLexer(file);
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(syntaxErrorListener);
            
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new CcashParser(tokenStream);
            parser.RemoveErrorListeners();
            parser.AddErrorListener(syntaxErrorListener);
            
            var compilationUnitContext = parser.compilationUnit();
            if (ErrorManager.HasErrors)
            {
                ErrorManager.PrintErrors();
                return;
            }

            var treeWalker = new ParseTreeWalker();
            var globalSymbolsListener = new GlobalModuleListener<LLVMCodeGenerator>();
            treeWalker.Walk(globalSymbolsListener, compilationUnitContext);

            var generator = globalSymbolsListener.CodeGenerator;
            var globalScope = globalSymbolsListener.GlobalModuleScope;

            try
            {
                treeWalker.Walk(new CompilationListener(globalScope, generator), compilationUnitContext);
            }
            catch (CannotDetermineTypeException)
            {
                // Unrecoverable error during semantic analysis, details in the ErrorManager
            }
            finally
            {
                if (ErrorManager.HasErrors)
                    ErrorManager.PrintErrors();
                else
                    generator.WriteToFile(Path.GetFileNameWithoutExtension(inputFilePath));

                ErrorManager.PrintWarnings();
            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Ccash.SemanticAnalysis.Attributes;
using Ccash.SemanticAnalysis.Nodes.Statements;
using Ccash.SemanticAnalysis.Nodes.Statements.ControlFlow;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;
using FunctionDeclarationContext = Ccash.Antlr.CcashParser.FunctionDeclarationContext;
using VariableContext = Ccash.Antlr.CcashParser.VariableContext;
using MethodContext = Ccash.Antlr.CcashParser.MethodContext;
using BlockContext = Ccash.Antlr.CcashParser.BlockContext;

namespace Ccash.SemanticAnalysis.Nodes
{
    public class FunctionDeclaration : BlockScope
    {
        public FunctionHeader Header { get; }

        public List<Parameter> Parameters { get; } = new List<Parameter>();

        public FunctionDeclaration(FunctionDeclarationContext context, AbstractScope parent) : base(parent)
        {
            Header = new FunctionHeader(context.functionHeader());

            var parameterContexts =
                context.functionHeader().functionParameters()?.variable() ?? new VariableContext[] { };

            foreach (var parameterContext in parameterContexts)
            {
                var parameter = new Parameter(parameterContext);
                Parameters.Add(parameter);
                if (!Symbols.TryAdd(parameter.Name, new SymbolInfo(parameter.Type, parameter.IsMutable)))
                {
                    ErrorManager.DuplicateIdentifier(context.functionHeader(), parameter.Name);
                }
            }

            ParseBlock(context.block());
        }

        /// For handling static methods
        public FunctionDeclaration(StructType structType, MethodContext context, AbstractScope parent) : base(parent)
        {
            Header = new FunctionHeader(structType, context.methodHeader());
            
            var parameterContexts =
                context.methodHeader().functionParameters()?.variable() ?? new VariableContext[] { };

            foreach (var parameterContext in parameterContexts)
            {
                var parameter = new Parameter(parameterContext);
                Parameters.Add(parameter);
                if (!Symbols.TryAdd(parameter.Name, new SymbolInfo(parameter.Type, parameter.IsMutable)))
                {
                    ErrorManager.DuplicateIdentifier(context.methodHeader(), parameter.Name);
                }
            }

            ParseBlock(context.block());
        }
        
        public FunctionDeclaration(StructType structType, MethodContext context) : base(structType.ModuleScope)
        {
            Header = new FunctionHeader(structType, context.methodHeader());

            var parameterContexts =
                context.methodHeader().functionParameters()?.variable() ?? new VariableContext[] { };

            var thisType = context.methodHeader().Mut() == null ? structType.ConstRef : structType.MutRef;
            Parameters.Add(new Parameter("__this__", thisType, thisType.IsMutable));
            Symbols.Add("__this__", new SymbolInfo(thisType, thisType.IsMutable));

            foreach (var parameterContext in parameterContexts)
            {
                var parameter = new Parameter(parameterContext);
                Parameters.Add(parameter);
                if (!Symbols.TryAdd(parameter.Name, new SymbolInfo(parameter.Type, parameter.IsMutable)))
                {
                    ErrorManager.DuplicateIdentifier(context.methodHeader(), parameter.Name);
                }
            }

            ParseBlock(context.block());
        }

        private void ParseBlock(BlockContext context)
        {
            var inheritedAttributes = new InheritedAttributes();
            Statements = context.statement()
                                .Select(s => StatementFactory.Create(s, this, inheritedAttributes))
                                .ToList();

            if (Statements.Any(s => s.AlwaysReturns))
            {
                if (Statements.First(s => s.AlwaysReturns) != Statements.Last())
                {
                    ErrorManager.AddError(context, "Unreachable code after return");
                }
            }
            else
            {
                if (Header.FunctionType.ReturnType == CcashType.Void)
                {
                    Statements.Add(new ReturnStatement());
                }
                else
                {
                    var lastContext = context.statement().Any()
                                          ? context.statement().Last()
                                          : (ParserRuleContext) context;
                    const string hint = "did you forget a `return`?";
                    ErrorManager.MismatchedTypes(lastContext, Header.FunctionType.ReturnType, CcashType.Void, hint);
                }
            }
        }

        public override bool Contains(string symbol)
        {
            return Parameters.Any(p => p.Name == symbol) || base.Contains(symbol);
        }
    }
}

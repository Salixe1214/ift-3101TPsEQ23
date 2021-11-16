using System;
using System.Collections.Generic;
using Ccash.SemanticAnalysis.Nodes;
using Ccash.SemanticAnalysis.Types;

namespace Ccash.SemanticAnalysis.SymbolTable
{
    public class DuplicateSymbolException : Exception
    {
    }

    public abstract class Scope : AbstractScope
    {
        public override AbstractScope Parent { get; }

        protected Dictionary<string, SymbolInfo> Symbols { get; } = new Dictionary<string, SymbolInfo>();
        
        protected readonly Dictionary<string, List<FunctionHeader>> Functions = new Dictionary<string, List<FunctionHeader>>();

        protected Scope()
        {
        }

        protected Scope(AbstractScope parent)
        {
            Parent = parent;
        }

        public override void AddSymbol(string symbol, CcashType type, bool isMutable = false)
        {
            if (!Symbols.TryAdd(symbol, new SymbolInfo(type, isMutable)))
            {
                throw new DuplicateSymbolException();
            }
        }

        public void AddStructField(string name, CcashType type)
        {
            if (!Symbols.TryAdd(name, new SymbolInfo(type, isStructField: true)))
            {
                throw new DuplicateSymbolException();
            }
        }

        public override void AddFunction(FunctionHeader header)
        {
            AddSymbol(header.FullName, header.FunctionType);
            
            if (Functions.ContainsKey(header.Name))
            {
                Functions[header.Name].Add(header);
            }
            else
            {
                Functions[header.Name] = new List<FunctionHeader>{header};
            }
        }

        public override bool Contains(string symbol)
        {
            return Symbols.ContainsKey(symbol) || (Parent?.Contains(symbol) ?? false);
        }

        public override T GetCodeGeneratorData<T>(string symbol)
        {
            var codeGeneratorData = GetSymbolInfo(symbol).CodeGeneratorAttribute;
            if (codeGeneratorData == null)
            {
                throw new NullReferenceException($"did you forget to set the data for {symbol}?");
            }

            return (T) codeGeneratorData.Data;
        }

        public SymbolInfo this[string symbol] => GetSymbolInfo(symbol);

        public override SymbolInfo GetSymbolInfo(string symbol)
        {
            return Symbols.TryGetValue(symbol, out var info) ? info : Parent.GetSymbolInfo(symbol);
        }

        public override List<FunctionHeader> GetFunctionOverloads(string identifier)
        {
            return Parent.GetFunctionOverloads(identifier);
        }
    }
}

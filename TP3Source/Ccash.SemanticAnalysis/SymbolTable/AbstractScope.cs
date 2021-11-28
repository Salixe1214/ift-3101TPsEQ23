using System.Collections.Generic;
using Ccash.SemanticAnalysis.Nodes;
using Ccash.SemanticAnalysis.Types;

namespace Ccash.SemanticAnalysis.SymbolTable
{
    public abstract class AbstractScope
    {
        public abstract AbstractScope Parent { get; }

        public abstract void AddSymbol(string symbol, CcashType type, bool isMutable = false);

        public abstract void AddFunction(FunctionHeader header);

        public abstract bool Contains(string symbol);

        public abstract T GetCodeGeneratorData<T>(string symbol);

        public SymbolInfo this[string symbol] => GetSymbolInfo(symbol);

        public abstract SymbolInfo GetSymbolInfo(string symbol);

        public abstract List<FunctionHeader> GetFunctionOverloads(string identifier);

        /// Enumerates all the parent scopes that are of type T
        public IEnumerable<T> Enclosing<T>() where T: AbstractScope
        {
            var current = this;
            while (current != null)
            {
                if (current is T t)
                {
                    yield return t;
                }
                
                current = current.Parent;
            }
        }
    }
}

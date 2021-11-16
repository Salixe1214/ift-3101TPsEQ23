using System;
using System.Collections.Generic;
using Ccash.SemanticAnalysis.Nodes;

namespace Ccash.SemanticAnalysis.SymbolTable
{
    public class FunctionNotFoundException : Exception
    {
    }
    
    public class ModuleScope : Scope
    {
        public ModuleScope()
        {
        }

        public ModuleScope(AbstractScope parent): base(parent)
        {
        }

        public override List<FunctionHeader> GetFunctionOverloads(string identifier)
        {
            if (Functions.ContainsKey(identifier))
            {
                return Functions[identifier];
            }

            if (Parent != null)
            {
                return Parent.GetFunctionOverloads(identifier);
            }
            
            throw new FunctionNotFoundException();
        }
    }
}

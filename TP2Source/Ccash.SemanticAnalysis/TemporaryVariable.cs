using Antlr4.Runtime;
using Ccash.SemanticAnalysis.Nodes.Expressions;
using Ccash.SemanticAnalysis.Nodes.Statements;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;

namespace Ccash.SemanticAnalysis
{
    public static class TemporaryVariable
    {
        private static uint _count;

        public static VariableDeclaration New(ParserRuleContext context,
                                              CcashType type,
                                              IExpression expression,
                                              bool isMutable,
                                              AbstractScope scope)
        {
            var name = $"__temp{_count++}__";
            return new VariableDeclaration(context, name, type, expression, isMutable, scope);
        }
        
        public static VariableDeclaration New(IExpression expression, bool isMutable, AbstractScope scope)
        {
            var name = $"__temp{_count++}__";
            return new VariableDeclaration(name, expression, isMutable, scope);
        }
    }
}

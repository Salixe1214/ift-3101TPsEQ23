using System.Linq;
using Antlr4.Runtime;
using Ccash.SemanticAnalysis.Attributes;
using Ccash.SemanticAnalysis.Nodes.Expressions;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;
using VariableDeclarationContext = Ccash.Antlr.CcashParser.VariableDeclarationContext;

namespace Ccash.SemanticAnalysis.Nodes.Statements
{
    [SemanticRule("this.AlwaysReturns = false")]
    public class VariableDeclaration : IStatement
    {
        public bool AlwaysReturns => false;

        public string Name { get; }

        public CcashType Type { get; }

        public IExpression Expression { get; }

        public VariableDeclaration(VariableDeclarationContext context, AbstractScope scope)
        {
            Name = context.variable().Identifier().GetText();
            Type = CcashType.Create(context.variable());

            var expression = ExpressionFactory.Create(context.expression(), scope);
            Expression = CallConstructor(Type, expression, context) ?? expression;

            try
            {
                scope.AddSymbol(Name, Type, context.variable().variableType().Mut() != null);
            }
            catch (DuplicateSymbolException e)
            {
                ErrorManager.DuplicateIdentifier(context, Name);
            }
        }

        ///  Manually creates a variable of the specified type
        public VariableDeclaration(ParserRuleContext context,
                                   string name,
                                   CcashType type,
                                   IExpression expression,
                                   bool isMutable,
                                   AbstractScope scope)
        {
            Name = name;
            Type = type;
            Expression = CallConstructor(type, expression, context) ?? expression;

            try
            {
                scope.AddSymbol(Name, Type, isMutable);
            }
            catch (DuplicateSymbolException e)
            {
                ErrorManager.DuplicateIdentifier(context, Name);
            }
        }

        ///  Manually creates a type-inferred variable, bypassing all semantic checks
        public VariableDeclaration(string name, IExpression expression, bool isMutable, AbstractScope scope)
        {
            Name = name;
            Expression = expression;
            Type = Expression.Type;

            scope.AddSymbol(Name, Type, isMutable);
        }

        private static IExpression CallConstructor(CcashType type, IExpression argument, ParserRuleContext context)
        {
            try
            {
                switch (type)
                {
                    case ReferenceType refType when argument.Type is ReferenceType argRefType &&
                                                    argRefType.CanBeCoercedInMutability(refType):
                        return argument;
                    
                    case ReferenceType _:
                        ErrorManager.MismatchedTypes(context, type, argument.Type);
                        return null;
                    
                    case ArrayType arrayType when argument.Type is ArrayType argArrayType &&
                                                  argArrayType.CanBeCoerced(arrayType):
                        return argument;
                    
                    case ArrayType _:
                        ErrorManager.MismatchedTypes(context, type, argument.Type);
                        return null;
                    
                    case StructType _:
                        if (argument.Type == type || argument.Type.CanBeCoerced(type))
                            return ExpressionFactory.Coerce(argument, type);
                        
                        ErrorManager.MismatchedTypes(context, type, argument.Type);
                        return null;
                    
                    default:
                        if (argument.Type == type || argument.Type.CanBeCoerced(type))
                            return type.CallConstructor(argument);
                        
                        ErrorManager.MismatchedTypes(context, type, argument.Type);
                        return null;

                }
            }
            catch (NoValidOverloadsException)
            {
                ErrorManager.NoValidOverload(context, type.Name, argument);
            }
            catch (AmbiguousCallException e)
            {
                ErrorManager.AmbiguousCall(context, e.Overloads, argument);
            }
            catch (WrongArityException e)
            {
                ErrorManager.WrongArity(context, type.Name, e.Overload.FunctionType.ParameterTypes.Count, 1);
            }
            catch (ArgumentTypeMismatchException e)
            {
                ErrorManager.MismatchedTypes(context, e.Overload.FunctionType.ParameterTypes.First(), argument.Type);
            }

            return null;
        }
    }
}

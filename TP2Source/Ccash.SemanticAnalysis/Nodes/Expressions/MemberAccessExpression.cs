using Antlr4.Runtime;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;
using MemberAccessExpressionContext = Ccash.Antlr.CcashParser.MemberAccessExpressionContext;
using ValueType = Ccash.SemanticAnalysis.Types.ValueType;

namespace Ccash.SemanticAnalysis.Nodes.Expressions
{
    public struct Member
    {
        public string Name { get; }

        public CcashType Type { get; }

        public Member(string name, CcashType type)
        {
            Name = name;
            Type = type;
        }
    }

    public class MemberAccessExpression : IExpression
    {
        public CcashType Type { get; }

        public string Text { get; }

        public IExpression Expression { get; }

        public Member Member { get; }

        public uint Offset { get; }

        public MemberAccessExpression(MemberAccessExpressionContext context, AbstractScope scope)
            : this(context, ExpressionFactory.Create(context.expression(), scope), context.Identifier().GetText(), scope)
        {
        }

        public MemberAccessExpression(ParserRuleContext context,
                                      IExpression expression,
                                      string memberName,
                                      AbstractScope scope)
        {
            Text = context.GetText();
            
            Expression = ExpressionFactory.CoerceStructToRef(expression, scope);
            switch (Expression.Type)
            {
                case ArrayType arrayType:
                {
                    if (memberName == "length")
                    {
                        Offset = 0;
                        Type = CcashType.Uint32.ConstRef;
                    }
                    else
                    {
                        Type = CcashType.Void;
                        ErrorManager.AddError(context, $"type `{arrayType}` has no member `{memberName}`");
                    }

                    break;
                }
                case ReferenceType refType when refType.ReferredType is StructType structType:
                {
                    for (var i = 0; i < structType.Fields.Count; i++)
                    {
                        var field = structType.Fields[i];
                        if (field.Name == memberName)
                        {
                            Offset = (uint) i;

                            if (field.Type is ValueType fieldType)
                            {
                                Type = refType.IsMutable ? fieldType.MutRef : fieldType.ConstRef;
                            }
                            else
                            {
                                Type = field.Type;
                            }

                            Member = new Member(memberName, field.Type);

                            return;
                        }
                    }

                    Type = CcashType.Void;
                    ErrorManager.AddError(context, $"type `{structType}` has no member `{memberName}`");
                    break;
                }
                default:
                    ErrorManager.AddError(context, $"Member access on type `{Expression.Type}` is unsupported");
                    throw new CannotDetermineTypeException();
            }
        }
    }
}

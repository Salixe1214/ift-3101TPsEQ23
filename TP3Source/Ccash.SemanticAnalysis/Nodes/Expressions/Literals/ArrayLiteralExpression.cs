using System.Collections.Generic;
using System.Linq;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;
using ArrayLiteralExpressionContext = Ccash.Antlr.CcashParser.ArrayLiteralExpressionContext;

namespace Ccash.SemanticAnalysis.Nodes.Expressions.Literals
{
    public class ArrayLiteralExpression : IExpression
    {
        public CcashType Type { get; }

        public ArrayType ArrayType => (ArrayType) Type;

        public string Text { get; }

        public List<IExpression> Elements { get; }
        
        public ArrayLiteralExpression(ArrayLiteralExpressionContext context, AbstractScope scope)
        {
            Text = context.GetText();
            
            Elements = context.expression().Select(e => ExpressionFactory.Create(e, scope)).ToList();

            var containedType = Elements.First().Type;
            foreach (var element in Elements)
            {
                if (containedType == element.Type)
                    continue;

                if (element.Type.CanBeCoerced(containedType))
                    continue;
                
                if (containedType.CanBeCoerced(element.Type))
                {
                    containedType = element.Type;
                }
                else
                {
                    ErrorManager.MismatchedTypes(context, containedType, element.Type);
                }
            }

            bool isMutable;
            switch (containedType)
            {
                case ReferenceType refType:
                    isMutable = refType.IsMutable;
                    break;
                case ArrayType type:
                    isMutable = type.IsMutable;
                    break;
                default:
                    isMutable = true;
                    break;
            }

            var arrayType = new ArrayType(containedType, isMutable);

            for (var i = 0; i < Elements.Count; i++)
            {
                if (Elements[i].Type.CanBeCoerced(arrayType.ContainedType))
                {
                    Elements[i] = ExpressionFactory.Coerce(Elements[i], arrayType.ContainedType);
                }
            }

            Type = arrayType;
        }
    }
}

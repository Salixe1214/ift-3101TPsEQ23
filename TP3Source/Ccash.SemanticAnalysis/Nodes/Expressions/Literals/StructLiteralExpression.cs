using System.Collections.Generic;
using System.Linq;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;
using StructLiteralExpressionContext = Ccash.Antlr.CcashParser.StructLiteralExpressionContext;

namespace Ccash.SemanticAnalysis.Nodes.Expressions.Literals
{
    public class StructLiteralExpression : IRvalueExpression
    {
        public CcashType Type { get; }

        public StructType StructType => (StructType) Type;

        public string Text { get; }

        public Dictionary<string, IExpression> Fields { get; } = new Dictionary<string, IExpression>();

        public StructLiteralExpression(StructLiteralExpressionContext context, AbstractScope scope)
        {
            Text = context.GetText();

            var structName = context.Identifier().GetText();
            if (CcashType.Structs.ContainsKey(structName))
            {
                Type = CcashType.Structs[structName];

                var uninitializedStructFields = StructType.Fields.Select(f => f.Name).ToHashSet();

                foreach (var fieldInitializer in context.fieldInitializer())
                {
                    var fieldName = fieldInitializer.Identifier().GetText();
                    if (Fields.ContainsKey(fieldName))
                    {
                        ErrorManager.AddError(fieldInitializer, $"duplicate field in struct initialization: `{fieldName}`");
                    }
                    else if (StructType.Fields.Any(f => f.Name == fieldName))
                    {
                        var targetType = StructType.Fields.First(f => f.Name == fieldName).Type;
                        var expression = ExpressionFactory.Create(fieldInitializer.expression(), scope);

                        if (expression.Type.CanBeCoerced(targetType))
                        {
                            expression = ExpressionFactory.Coerce(expression, targetType);
                        }
                        else
                        {
                            ErrorManager.MismatchedTypes(fieldInitializer, targetType, expression.Type);
                        }

                        Fields.Add(fieldName, expression);
                        uninitializedStructFields.Remove(fieldName);
                    }
                    else
                    {
                        ErrorManager.AddError(fieldInitializer,
                                              $"type `{StructType}` does not contain a field `{fieldName}`");
                    }
                }

                if (uninitializedStructFields.Any())
                {
                    var missingMembers = string.Join(", ", uninitializedStructFields.Select(m => $"`{m}`"));
                    ErrorManager.AddError(context,
                                          $"missing field{(missingMembers.Length > 1 ? "s" : "")} in struct initialization: {missingMembers}");
                }
            }
            else
            {
                ErrorManager.AddError(context, $"`{structName}` is not a type");
                throw new CannotDetermineTypeException();
            }
        }
    }
}

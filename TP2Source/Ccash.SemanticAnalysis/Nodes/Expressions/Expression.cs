using System;
using Ccash.SemanticAnalysis.Nodes.Expressions.Literals;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;
using ExpressionContext = Ccash.Antlr.CcashParser.ExpressionContext;
using IntegerLiteralExpressionContext = Ccash.Antlr.CcashParser.IntegerLiteralExpressionContext;
using FloatLiteralExpressionContext = Ccash.Antlr.CcashParser.FloatLiteralExpressionContext;
using Float32LiteralExpressionContext = Ccash.Antlr.CcashParser.Float32LiteralExpressionContext;
using StringLiteralExpressionContext = Ccash.Antlr.CcashParser.StringLiteralExpressionContext;
using BooleanLiteralExpressionContext = Ccash.Antlr.CcashParser.BooleanLiteralExpressionContext;
using IdentifierExpressionContext = Ccash.Antlr.CcashParser.IdentifierExpressionContext;
using FunctionCallExpressionContext = Ccash.Antlr.CcashParser.FunctionCallExpressionContext;
using BinaryOperatorExpressionContext = Ccash.Antlr.CcashParser.BinaryOperatorExpressionContext;
using UnaryOperatorExpressionContext = Ccash.Antlr.CcashParser.UnaryOperatorExpressionContext;
using RefExpressionContext = Ccash.Antlr.CcashParser.RefExpressionContext;
using ParenthesisExpressionContext = Ccash.Antlr.CcashParser.ParenthesisExpressionContext;
using TernaryExpressionContext = Ccash.Antlr.CcashParser.TernaryExpressionContext;
using ArrayLiteralExpressionContext = Ccash.Antlr.CcashParser.ArrayLiteralExpressionContext;
using IndexOperatorExpressionContext = Ccash.Antlr.CcashParser.IndexOperatorExpressionContext;
using MemberAccessExpressionContext = Ccash.Antlr.CcashParser.MemberAccessExpressionContext;
using StructLiteralExpressionContext = Ccash.Antlr.CcashParser.StructLiteralExpressionContext;
using MethodCallExpressionContext = Ccash.Antlr.CcashParser.MethodCallExpressionContext;

namespace Ccash.SemanticAnalysis.Nodes.Expressions
{
    public class CannotDetermineTypeException : Exception
    {
    }

    public interface IExpression
    {
        CcashType Type { get; }

        string Text { get; }
    }
    
    public static class ExpressionFactory
    {
        public static IExpression Coerce(IExpression expression, CcashType type)
        {
            if (expression.Type == type)
            {
                return expression;
            }

            if (expression.Type is ReferenceType)
            {
                return type is ReferenceType ? expression : Coerce(new AutoDerefExpression(expression), type);
            }

            if (expression.Type is ArrayType)
            {
                return expression;
            }

            return type.CallConstructor(expression);
        }

        public static IExpression CoerceStructToRef(IExpression expression, AbstractScope scope)
        {
            if (expression is IRvalueExpression rvalueExpr && !(rvalueExpr.Type is ReferenceType))
            {
                return new RvalueRefBindExpression(rvalueExpr, scope);
            }

            if (expression.Type is StructType && expression is IdentifierExpression idExpr)
            {
                return new RefExpression(idExpr);
            }

            return expression;
        }
        
        public static IExpression Create(ExpressionContext context, AbstractScope scope)
        {
            switch (context)
            {
                case IntegerLiteralExpressionContext integerLiteralContext:
                    return new IntegerLiteralExpression(integerLiteralContext);

                case Float32LiteralExpressionContext float32LiteralContext:
                    return new FloatLiteralExpression(float32LiteralContext);

                case FloatLiteralExpressionContext floatLiteralContext:
                    return new FloatLiteralExpression(floatLiteralContext);

                case StringLiteralExpressionContext stringLiteralContext:
                    return new StringLiteralExpression(stringLiteralContext);

                case BooleanLiteralExpressionContext booleanLiteralContext:
                    return new BooleanLiteralExpression(booleanLiteralContext);
                
                case StructLiteralExpressionContext structLiteralContext:
                    return new StructLiteralExpression(structLiteralContext, scope);

                case IdentifierExpressionContext identifierContext:
                    return IdentifierExpression.Create(identifierContext, scope);

                case FunctionCallExpressionContext functionCallContext:
                    return new FunctionCallExpression(functionCallContext.functionCall(), scope);

                case BinaryOperatorExpressionContext binaryOperatorContext:
                    return new BinaryOperatorExpression(binaryOperatorContext, scope);

                case UnaryOperatorExpressionContext unaryOperatorContext:
                    return new UnaryOperatorExpression(unaryOperatorContext, scope);

                case ParenthesisExpressionContext parenthesisContext:
                    return Create(parenthesisContext.expression(), scope);

                case TernaryExpressionContext ternaryContext:
                    return new TernaryExpression(ternaryContext, scope);
                
                case RefExpressionContext refContext:
                    return new RefExpression(refContext, scope);
                
                case ArrayLiteralExpressionContext arrayLiteralContext:
                    return new ArrayLiteralExpression(arrayLiteralContext, scope);
                
                case IndexOperatorExpressionContext indexOperatorExpressionContext:
                    return new IndexOperatorExpression(indexOperatorExpressionContext, scope);
                
                case MemberAccessExpressionContext memberAccessContext:
                    return new MemberAccessExpression(memberAccessContext, scope);
                
                case MethodCallExpressionContext methodCallContext:
                    return new MethodCallExpression(methodCallContext, scope);
                
                default:
                    throw new NotImplementedException($"{context.GetType()} is not yet implemented");
            }
        }
    }
}

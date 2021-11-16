using System;
using System.Collections.Generic;
using System.Linq;
using Ccash.SemanticAnalysis.Nodes.Expressions;
using Ccash.SemanticAnalysis.Types;
using LLVMSharp;

namespace Ccash.CodeGeneration.LLVMGenerator.Intrinsics
{
    public static class IntrinsicExpressions
    {
        private static Dictionary<string, string> Functions { get; }

        static IntrinsicExpressions()
        {
            Functions = new Dictionary<string, string>();

            foreach (var type in CcashType.AllPrimitives)
            {
                foreach (var constructor in type.Constructors)
                {
                    Functions.Add(constructor.FullName, type.Name);
                }
            }

            foreach (var op in CcashType.NativeOperators)
            {
                var operatorSymbol = op.Name.Substring("operator".Length);
                Functions.Add(op.FullName, operatorSymbol);
            }
        }

        public static bool IsIntrinsic(this FunctionCallExpression functionCall) =>
            Functions.ContainsKey(functionCall.FullFunctionName)
            || functionCall.FunctionName == "operator[]" && functionCall.Arguments[0].Type is ReferenceType refType
            && refType.ReferredType is ArrayType;

        public static LLVMValueRef IntrinsicExpression(this IRBuilder builder, string functionName, Arg[] args)
        {
            if (functionName.StartsWith("operator[]"))
            {
                LLVMValueRef elementsPointerField = builder.GetElementPointer(args[0].Value, 0, 1);
                LLVMValueRef elementsPointer = builder.Load(elementsPointerField);
                LLVMValueRef elementPointer = builder.GetElementPointer(elementsPointer, args[1].Value);
                var arrayType = (ArrayType) ((ReferenceType) args[0].Type).ReferredType;
                return arrayType.ContainedType is ReferenceType ? builder.Load(elementPointer) : elementPointer;
            }

            var symbol = Functions[functionName];

            if (args.Length == 1)
            {
                return builder.UnaryIntrinsicCall(symbol, args.First());
            }

            Func<IRBuilder, Arg, Arg, LLVMValueRef> buildFunction;
            switch (symbol)
            {
                case "+":
                    buildFunction = BuildAddition;
                    break;
                case "-":
                    buildFunction = BuildSubtraction;
                    break;
                case "*":
                    buildFunction = BuildMultiplication;
                    break;
                case "/":
                    buildFunction = BuildDivision;
                    break;
                case "%":
                    buildFunction = BuildModulo;
                    break;
                case "^":
                    buildFunction = (b, left, right) => b.Xor(left.Value, right.Value);
                    break;
                case "||":
                    buildFunction = (b, left, right) => b.Or(left.Value, right.Value);
                    break;
                case "|":
                    buildFunction = (b, left, right) => b.Or(left.Value, right.Value);
                    break;
                case "&&":
                    buildFunction = (b, left, right) => b.And(left.Value, right.Value);
                    break;
                case "&":
                    buildFunction = (b, left, right) => b.And(left.Value, right.Value);
                    break;
                case "<":
                    buildFunction = BuildLessThan;
                    break;
                case ">":
                    buildFunction = BuildGreaterThan;
                    break;
                case "<=":
                    buildFunction = BuildLessOrEqual;
                    break;
                case ">=":
                    buildFunction = BuildGreaterOrEqual;
                    break;
                case "==":
                    buildFunction = BuildEqual;
                    break;
                case "!=":
                    buildFunction = BuildNotEqual;
                    break;
                default:
                    throw new NotImplementedException($"{symbol} is not yet supported");
            }

            return buildFunction(builder, args[0], args[1]);
        }

        private static LLVMValueRef BuildAddition(IRBuilder builder, Arg leftArgument, Arg rightArgument)
        {
            if (leftArgument.Type != rightArgument.Type)
            {
                // case int + float, need to cast the int to float
                leftArgument = new Arg(builder.UnaryIntrinsicCall(rightArgument.Type.Name, leftArgument),
                                       rightArgument.Type);
            }

            return rightArgument.Type is IntegerType
                       ? builder.Add(leftArgument.Value, rightArgument.Value)
                       : builder.FAdd(leftArgument.Value, rightArgument.Value);
        }

        private static LLVMValueRef BuildSubtraction(IRBuilder builder, Arg leftArgument, Arg rightArgument)
        {
            if (leftArgument.Type != rightArgument.Type)
            {
                // case int - float, need to cast the int to float
                leftArgument = new Arg(builder.UnaryIntrinsicCall(rightArgument.Type.Name, leftArgument),
                                       rightArgument.Type);
            }

            return rightArgument.Type is IntegerType
                       ? builder.Sub(leftArgument.Value, rightArgument.Value)
                       : builder.FSub(leftArgument.Value, rightArgument.Value);
        }

        private static LLVMValueRef BuildMultiplication(IRBuilder builder, Arg leftArgument, Arg rightArgument)
        {
            if (leftArgument.Type != rightArgument.Type)
            {
                // case int * float, need to cast the int to float
                leftArgument = new Arg(builder.UnaryIntrinsicCall(rightArgument.Type.Name, leftArgument),
                                       rightArgument.Type);
            }

            return rightArgument.Type is IntegerType
                       ? builder.Mul(leftArgument.Value, rightArgument.Value)
                       : builder.FMul(leftArgument.Value, rightArgument.Value);
        }

        private static LLVMValueRef BuildDivision(IRBuilder builder, Arg leftArgument, Arg rightArgument)
        {
            if (leftArgument.Type != rightArgument.Type)
            {
                // case int / float, need to cast the int to float
                leftArgument = new Arg(builder.UnaryIntrinsicCall(rightArgument.Type.Name, leftArgument),
                                       rightArgument.Type);
            }

            Func<LLVMValueRef, LLVMValueRef, LLVMValueRef> divFunc;
            if (rightArgument.Type is IntegerType)
            {
                var leftArgumentType = (IntegerType) leftArgument.Type;
                if (leftArgumentType.IsSigned)
                {
                    divFunc = builder.SignedDiv;
                }
                else
                {
                    divFunc = builder.UnsignedDiv;
                }
            }
            else
            {
                divFunc = builder.FDiv;
            }

            return divFunc(leftArgument.Value, rightArgument.Value);
        }

        private static LLVMValueRef BuildModulo(IRBuilder builder, Arg leftArgument, Arg rightArgument)
        {
            return ((IntegerType) leftArgument.Type).IsSigned
                       ? builder.SignedRem(leftArgument.Value, rightArgument.Value)
                       : builder.UnsignedRem(leftArgument.Value, rightArgument.Value);
        }

        private static LLVMValueRef BuildLessThan(IRBuilder builder, Arg leftArgument, Arg rightArgument)
        {
            if (leftArgument.Type is IntegerType intType)
            {
                return intType.IsSigned
                           ? builder.SignedLT(leftArgument.Value, rightArgument.Value)
                           : builder.UnsignedLT(leftArgument.Value, rightArgument.Value);
            }

            return builder.FLT(leftArgument.Value, rightArgument.Value);
        }

        private static LLVMValueRef BuildLessOrEqual(IRBuilder builder, Arg leftArgument, Arg rightArgument)
        {
            if (leftArgument.Type is IntegerType intType)
            {
                return intType.IsSigned
                           ? builder.SignedLE(leftArgument.Value, rightArgument.Value)
                           : builder.UnsignedLE(leftArgument.Value, rightArgument.Value);
            }

            return builder.FLE(leftArgument.Value, rightArgument.Value);
        }

        private static LLVMValueRef BuildGreaterThan(IRBuilder builder, Arg leftArgument, Arg rightArgument)
        {
            if (leftArgument.Type is IntegerType intType)
            {
                return intType.IsSigned
                           ? builder.SignedGT(leftArgument.Value, rightArgument.Value)
                           : builder.UnsignedGT(leftArgument.Value, rightArgument.Value);
            }

            return builder.FGT(leftArgument.Value, rightArgument.Value);
        }

        private static LLVMValueRef BuildGreaterOrEqual(IRBuilder builder, Arg leftArgument, Arg rightArgument)
        {
            if (leftArgument.Type is IntegerType intType)
            {
                return intType.IsSigned
                           ? builder.SignedGE(leftArgument.Value, rightArgument.Value)
                           : builder.UnsignedGE(leftArgument.Value, rightArgument.Value);
            }

            return builder.FGE(leftArgument.Value, rightArgument.Value);
        }

        private static LLVMValueRef BuildEqual(IRBuilder builder, Arg leftArgument, Arg rightArgument)
        {
            if (leftArgument.Type is IntegerType || leftArgument.Type is BooleanType)
            {
                return builder.EQ(leftArgument.Value, rightArgument.Value);
            }

            return builder.FEQ(leftArgument.Value, rightArgument.Value);
        }

        private static LLVMValueRef BuildNotEqual(IRBuilder builder, Arg leftArgument, Arg rightArgument)
        {
            if (leftArgument.Type is IntegerType || leftArgument.Type is BooleanType)
            {
                return builder.NE(leftArgument.Value, rightArgument.Value);
            }

            return builder.FNE(leftArgument.Value, rightArgument.Value);
        }
    }
}

using System;
using System.Collections.Generic;
using Ccash.SemanticAnalysis.Nodes.Statements;
using Ccash.SemanticAnalysis.Types;
using LLVMSharp;

namespace Ccash.CodeGeneration.LLVMGenerator.Intrinsics
{
    public static class IntrinsicStatementGenerator
    {
        public static bool IsIntrinsic(this FunctionCallStatement functionCall) =>
            Functions.ContainsKey(functionCall.FullFunctionName);

        private static Dictionary<string, string> Functions { get; }

        static IntrinsicStatementGenerator()
        {
            Functions = new Dictionary<string, string>();
            foreach (var op in CcashType.NativeOperators)
            {
                var operatorSymbol = op.Name.Substring("operator".Length);
                Functions.Add(op.FullName, operatorSymbol);
            }
        }

        public static void IntrinsicAssignmentCall(this IRBuilder builder, string functionName, Arg[] args)
        {
            var variable = args[0];
            var symbol = Functions[functionName];

            switch (symbol)
            {
                case "++":
                    builder.Inc(variable);
                    return;
                case "--":
                    builder.Dec(variable);
                    return;
            }

            LLVMValueRef newValue;

            var operand = args[1];
            switch (symbol)
            {
                case ":=":
                    newValue = operand.Value;
                    break;
                case ":=:":
                {
                    LLVMValueRef oldValue = builder.Load(variable.Value);
                    LLVMValueRef operandValue = builder.Load(operand.Value);
                    builder.Store(oldValue, operand.Value);
                    newValue = operandValue;
                    break;
                }
                case "+=":
                {
                    LLVMValueRef oldValue = builder.Load(variable.Value);
                    newValue = ((ReferenceType) variable.Type).ReferredType is IntegerType
                                   ? builder.Add(oldValue, operand.Value)
                                   : builder.FAdd(oldValue, operand.Value);
                    break;
                }
                case "-=":
                {
                    LLVMValueRef oldValue = builder.Load(variable.Value);
                    newValue = variable.Type is IntegerType
                                   ? builder.Sub(oldValue, operand.Value)
                                   : builder.FSub(oldValue, operand.Value);
                    break;
                }
                case "*=":
                {
                    LLVMValueRef oldValue = builder.Load(variable.Value);
                    newValue = ((ReferenceType) variable.Type).ReferredType is IntegerType
                                   ? builder.Mul(oldValue, operand.Value)
                                   : builder.FMul(oldValue, operand.Value);
                    break;
                }
                case "/=":
                {
                    LLVMValueRef oldValue = builder.Load(variable.Value);
                    if (((ReferenceType) variable.Type).ReferredType is IntegerType intType)
                    {
                        newValue = intType.IsSigned
                                       ? builder.SignedDiv(oldValue, operand.Value)
                                       : builder.UnsignedDiv(oldValue, operand.Value);
                        break;
                    }

                    newValue = builder.FDiv(oldValue, operand.Value);
                    break;
                }
                case "%=":
                {
                    LLVMValueRef oldValue = builder.Load(variable.Value);
                    var intType = (IntegerType) ((ReferenceType) variable.Type).ReferredType;
                    newValue = intType.IsSigned
                                   ? builder.SignedRem(oldValue, operand.Value)
                                   : builder.UnsignedRem(oldValue, operand.Value);
                    break;
                }
                case "|=":
                {
                    LLVMValueRef oldValue = builder.Load(variable.Value);
                    newValue = builder.Or(oldValue, operand.Value);
                    break;
                }
                case "&=":
                {
                    LLVMValueRef oldValue = builder.Load(variable.Value);
                    newValue = builder.And(oldValue, operand.Value);
                    break;
                }
                case "^=":
                {
                    LLVMValueRef oldValue = builder.Load(variable.Value);
                    newValue = builder.Xor(oldValue, operand.Value);
                    break;
                }
                default:
                    throw new NotImplementedException($"{symbol} is not yet supported");
            }

            builder.Store(newValue, variable.Value);
        }

        private static void Inc(this IRBuilder builder, Arg arg)
        {
            var intType = (IntegerType) ((ReferenceType) arg.Type).ReferredType;
            var one = LLVM.ConstInt(TypeResolver.Resolve(intType), 1, intType.IsSigned);

            LLVMValueRef oldValue = builder.Load(arg.Value);
            oldValue.SetValueName("__rvalue__");
            LLVMValueRef newValue = builder.Add(oldValue, one);
            builder.Store(newValue, arg.Value);
        }

        private static void Dec(this IRBuilder builder, Arg arg)
        {
            var intType = (IntegerType) ((ReferenceType) arg.Type).ReferredType;
            var one = LLVM.ConstInt(TypeResolver.Resolve(intType), 1, intType.IsSigned);

            LLVMValueRef oldValue = builder.Load(arg.Value);
            oldValue.SetValueName("__rvalue__");
            LLVMValueRef newValue = builder.Sub(oldValue, one);
            builder.Store(newValue, arg.Value);
        }
    }
}

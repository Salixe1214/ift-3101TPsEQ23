using System;
using System.Linq;
using Ccash.CodeGeneration.LLVMGenerator.Intrinsics;
using Ccash.SemanticAnalysis.Nodes.Expressions;
using Ccash.SemanticAnalysis.Nodes.Expressions.Literals;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;
using LLVMSharp;

namespace Ccash.CodeGeneration.LLVMGenerator
{
    public static class ExpressionBuilderExtensions
    {
        public static LLVMValueRef Expression(this IRBuilder builder, IExpression expression, AbstractScope scope)
        {
            switch (expression)
            {
                case IntegerLiteralExpression intLiteral:
                {
                    LLVMTypeRef type = TypeResolver.Resolve(intLiteral.Type);
                    return LLVM.ConstInt(type, intLiteral.UnsignedValue, intLiteral.IntegerType.IsSigned);
                }

                case FloatLiteralExpression floatLiteral:
                {
                    LLVMTypeRef type = TypeResolver.Resolve(floatLiteral.Type);
                    return LLVM.ConstReal(type, floatLiteral.Value);
                }

                case BooleanLiteralExpression boolLiteral:
                {
                    LLVMTypeRef type = TypeResolver.Resolve(boolLiteral.Type);
                    return LLVM.ConstInt(type, Convert.ToByte(boolLiteral.Value), false);
                }

                case StringLiteralExpression strLiteral:
                {
                    LLVMValueRef str = builder.AddGlobalString("str", strLiteral.Value);
                    LLVMValueRef pointer = builder.ReinterpretCast(str, LLVM.PointerType(LLVMTypeRef.Int8Type(), 0));
                    LLVMValueRef length = LLVM.ConstInt(LLVM.Int32Type(), (ulong) strLiteral.Value.Length, false);

                    LLVMValueRef llvmStruct = builder.AllocateStruct(length, pointer);
                    return builder.Load(llvmStruct);
                }

                case ArrayLiteralExpression arrayExpression:
                {
                    LLVMValueRef[] elements =
                        arrayExpression.Elements.Select(e => builder.Expression(e, scope)).ToArray();

                    LLVMValueRef arrayVariable = builder.AllocateArray(elements);

                    LLVMValueRef arrayPointer = builder.GetElementPointerInBounds(arrayVariable, 0, 0);

                    LLVMValueRef length =
                        LLVM.ConstInt(LLVM.Int32Type(), (ulong) arrayExpression.Elements.Count, false);

                    LLVMValueRef llvmStruct = builder.AllocateStruct(length, arrayPointer);
                    return builder.Load(llvmStruct);
                }

                case StructLiteralExpression structLiteralExpression:
                {
                    LLVMValueRef[] members = structLiteralExpression
                                             .Fields.Values
                                             .Select(e => builder.Expression(e, scope)).ToArray();

                    LLVMTypeRef structType = TypeResolver.Resolve(structLiteralExpression.Type);

                    LLVMValueRef llvmStruct = builder.AllocateStruct(structType, members);
                    return builder.Load(llvmStruct);
                }

                case IdentifierExpression identifierExpression:
                {
                    var identifier = identifierExpression.Identifier;
                    var variable = scope.GetCodeGeneratorData<LLVMValueRef>(identifier);
                    return identifierExpression.Type is ReferenceType ? variable : builder.Load(variable);
                }

                case RefExpression referenceExpression:
                {
                    var identifier = referenceExpression.Identifier;
                    return scope.GetCodeGeneratorData<LLVMValueRef>(identifier);
                }

                case AutoDerefExpression autoDerefExpression:
                {
                    LLVMValueRef reference = builder.Expression(autoDerefExpression.Expression, scope);
                    LLVMValueRef dereferenceValue = builder.Load(reference);
                    dereferenceValue.SetValueName(autoDerefExpression.Text);
                    return dereferenceValue;
                }

                case MethodCallExpression methodCall:
                {
                    var args = methodCall.FunctionCall.Arguments
                                                  .Select(a => new Arg(builder.Expression(a, scope), a.Type))
                                                  .ToArray();

                    var functionName = methodCall.FunctionCall.FullFunctionName;
                    var function = methodCall.StructType.ModuleScope.GetCodeGeneratorData<LLVMValueRef>(functionName);

                    return builder.CallFunction(function, args);
                }

                case FunctionCallExpression functionCall:
                {
                    var args = functionCall.Arguments
                                           .Select(a => new Arg(builder.Expression(a, scope), a.Type))
                                           .ToArray();

                    if (functionCall.IsIntrinsic())
                    {
                        return builder.IntrinsicExpression(functionCall.FullFunctionName, args);
                    }

                    var functionName = functionCall.FullFunctionName;
                    var function = scope.GetCodeGeneratorData<LLVMValueRef>(functionName);

                    return builder.CallFunction(function, args);
                }

                case TernaryExpression ternaryExpression:
                {
                    LLVMValueRef condition = builder.Expression(ternaryExpression.ConditionExpression, scope);
                    LLVMValueRef trueValue = builder.Expression(ternaryExpression.TrueExpression, scope);
                    LLVMValueRef falseValue = builder.Expression(ternaryExpression.FalseExpression, scope);

                    return builder.Ternary(condition, trueValue, falseValue);
                }

                case MemberAccessExpression memberAccessExpression:
                {
                    LLVMValueRef structVariable = builder.Expression(memberAccessExpression.Expression, scope);
                    LLVMValueRef field = builder.StructField(structVariable, memberAccessExpression.Offset);

                    // GetElementPointer will return a pointer, which would be a pointer to pointer in the case of a reference
                    return memberAccessExpression.Member.Type is ReferenceType ? builder.Load(field) : field;
                }

                case RvalueRefBindExpression rvalueRefBind:
                {
                    LLVMTypeRef type = TypeResolver.Resolve(rvalueRefBind.TempVariable.Type);
                    LLVMValueRef variable = builder.AllocateVariable(type, rvalueRefBind.TempVariable.Name);
                    LLVMValueRef initExpression = builder.Expression(rvalueRefBind.TempVariable.Expression, scope);
                    builder.Store(initExpression, variable);

                    scope[rvalueRefBind.TempVariable.Name].CodeGeneratorAttribute.Data = variable;
                    return variable;
                }

                default:
                    throw new NotImplementedException($"{expression.GetType()} is not yet supported");
            }
        }
    }
}

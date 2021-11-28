using System;
using Ccash.SemanticAnalysis.Types;
using LLVMSharp;

namespace Ccash.CodeGeneration.LLVMGenerator.Intrinsics
{
    public static class UnaryIntrinsicExpressions
    {
        public static LLVMValueRef UnaryIntrinsicCall(this IRBuilder builder, string functionName, Arg arg)
        {
            switch (functionName)
            {
                case "!":
                    return builder.BitNot(arg.Value);
                case "-":
                    return arg.Type is IntegerType ? builder.Neg(arg.Value) : builder.FNeg(arg.Value);
                case "bool":
                    return builder.BoolConstructor(arg);
                case "int8":
                    return builder.IntConstructor(CcashType.Int8, arg);
                case "uint8":
                    return builder.IntConstructor(CcashType.Uint8, arg);
                case "int16":
                    return builder.IntConstructor(CcashType.Int16, arg);
                case "uint16":
                    return builder.IntConstructor(CcashType.Uint16, arg);
                case "int32":
                    return builder.IntConstructor(CcashType.Int32, arg);
                case "uint32":
                    return builder.IntConstructor(CcashType.Uint32, arg);
                case "int64":
                    return builder.IntConstructor(CcashType.Int64, arg);
                case "uint64":
                    return builder.IntConstructor(CcashType.Uint64, arg);
                case "float32":
                    return builder.FloatConstructor(CcashType.Float32, arg);
                case "float64":
                    return builder.FloatConstructor(CcashType.Float64, arg);
                default:
                    throw new NotImplementedException($"{functionName} is not yet supported");
            }
        }

        private static LLVMValueRef BoolConstructor(this IRBuilder builder, Arg arg)
        {
            switch (arg.Type)
            {
                case BooleanType _:
                    return arg.Value;
                case IntegerType _:
                {
                    var zeroInt = LLVM.ConstInt(TypeResolver.Resolve(arg.Type), 0, true);
                    return builder.NE(arg.Value, zeroInt);
                }
                default:
                {
                    var zeroFloat = LLVM.ConstReal(TypeResolver.Resolve(arg.Type), 0);
                    return builder.FNE(arg.Value, zeroFloat);
                }
            }
        }

        private static LLVMValueRef IntConstructor(this IRBuilder builder, IntegerType intType, Arg arg)
        {
            var llvmDestinationType = TypeResolver.Resolve(intType);

            switch (arg.Type)
            {
                case IntegerType sourceIntType when intType.Size > sourceIntType.Size:
                    return sourceIntType.IsSigned
                               ? builder.SignExtendOrCastInt(arg.Value, llvmDestinationType)
                               : builder.ZeroExtendOrCastInt(arg.Value, llvmDestinationType);
                case IntegerType _:
                    return builder.TruncateOrCastInt(arg.Value, llvmDestinationType);
                case BooleanType _:
                    return builder.CastInt(arg.Value, llvmDestinationType);
                case FloatType _:
                    return intType.IsSigned
                               ? builder.FloatToSigned(arg.Value, llvmDestinationType)
                               : builder.FloatToUnsigned(arg.Value, llvmDestinationType);
                default:
                    throw new NotImplementedException();
            }
        }

        private static LLVMValueRef FloatConstructor(this IRBuilder builder, FloatType floatType, Arg arg)
        {
            var llvmDestinationType = TypeResolver.Resolve(floatType);

            switch (arg.Type)
            {
                case IntegerType sourceIntType:
                    return sourceIntType.IsSigned
                               ? builder.SignedToFloat(arg.Value, llvmDestinationType)
                               : builder.UnsignedToFloat(arg.Value, llvmDestinationType);
                case BooleanType _:
                    return builder.UnsignedToFloat(arg.Value, llvmDestinationType);
                case FloatType sourceFloatType:
                    return floatType.Size > sourceFloatType.Size
                               ? builder.ExtendFloat(arg.Value, llvmDestinationType)
                               : builder.TruncateFloat(arg.Value, llvmDestinationType);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}

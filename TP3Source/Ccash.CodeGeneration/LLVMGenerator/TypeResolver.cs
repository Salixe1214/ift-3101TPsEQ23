using System;
using System.Collections.Generic;
using Ccash.SemanticAnalysis.Types;
using LLVMSharp;

namespace Ccash.CodeGeneration.LLVMGenerator
{
    public static class TypeResolver
    {
        private static readonly Dictionary<string, LLVMTypeRef> TypeTable = new Dictionary<string, LLVMTypeRef>
        {
            {"int8", LLVM.Int8Type()},
            {"uint8", LLVM.Int8Type()},
            {"int16", LLVM.Int16Type()},
            {"uint16", LLVM.Int16Type()},
            {"int32", LLVM.Int32Type()},
            {"uint32", LLVM.Int32Type()},
            {"int64", LLVM.Int64Type()},
            {"uint64", LLVM.Int64Type()},
            {"float32", LLVM.FloatType()},
            {"float64", LLVM.DoubleType()},
            {"bool", LLVM.Int1Type()}
        };

        public static void RegisterStruct(string name, LLVMTypeRef structType)
        {
            TypeTable.Add(name, structType);
        }

        public static LLVMTypeRef Resolve(CcashType type)
        {
            if (TypeTable.TryGetValue(type.Name, out var typeRef))
            {
                return typeRef;
            }

            switch (type)
            {
                case VoidType _:
                    return LLVM.VoidType();
                case ReferenceType referenceType:
                    return LLVM.PointerType(Resolve(referenceType.ReferredType), 0);
                case ArrayType arrayType:
                    return
                        LLVM.StructType(new[] {LLVM.Int32Type(), LLVM.PointerType(Resolve(arrayType.ContainedType), 0)},
                                        false);
                default:
                    throw new NotImplementedException($"Unknown type: {type.Name}");
            }
        }
    }
}

using System.Linq;
using Ccash.CodeGeneration.LLVMGenerator.Intrinsics;
using LLVMSharp;

namespace Ccash.CodeGeneration.LLVMGenerator
{
    public class IRBuilder 
    {
        public LLVMBasicBlockRef CurrentBlock => LLVM.GetInsertBlock(Builder);

        private LLVMBuilderRef Builder { get; }

        public IRBuilder()
        {
            Builder = LLVM.CreateBuilder();
        }

        public void PositionAtEnd(LLVMBasicBlockRef block)
        {
            LLVM.PositionBuilderAtEnd(Builder, block);
        }

        public void ReturnVoid()
        {
            LLVM.BuildRetVoid(Builder);
        }

        public void Return(LLVMValueRef value)
        {
            LLVM.BuildRet(Builder, value);
        }

        public LLVMValueRef Add(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildAdd(Builder, left, right, "");
        }

        public LLVMValueRef FAdd(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildFAdd(Builder, left, right, "");
        }

        public LLVMValueRef Sub(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildSub(Builder, left, right, "");
        }

        public LLVMValueRef FSub(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildFSub(Builder, left, right, "");
        }

        public LLVMValueRef Mul(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildMul(Builder, left, right, "");
        }

        public LLVMValueRef FMul(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildFMul(Builder, left, right, "");
        }

        public LLVMValueRef SignedDiv(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildSDiv(Builder, left, right, "");
        }

        public LLVMValueRef UnsignedDiv(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildUDiv(Builder, left, right, "");
        }

        public LLVMValueRef FDiv(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildFDiv(Builder, left, right, "");
        }

        public LLVMValueRef SignedRem(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildSRem(Builder, left, right, "");
        }

        public LLVMValueRef UnsignedRem(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildURem(Builder, left, right, "");
        }

        public LLVMValueRef SignedLT(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildICmp(Builder, LLVMIntPredicate.LLVMIntSLT, left, right, "");
        }

        public LLVMValueRef UnsignedLT(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildICmp(Builder, LLVMIntPredicate.LLVMIntULT, left, right, "");
        }

        public LLVMValueRef FLT(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildFCmp(Builder, LLVMRealPredicate.LLVMRealULT, left, right, "");
        }

        public LLVMValueRef SignedLE(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildICmp(Builder, LLVMIntPredicate.LLVMIntSLE, left, right, "");
        }

        public LLVMValueRef UnsignedLE(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildICmp(Builder, LLVMIntPredicate.LLVMIntULE, left, right, "");
        }

        public LLVMValueRef FLE(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildFCmp(Builder, LLVMRealPredicate.LLVMRealULE, left, right, "");
        }

        public LLVMValueRef SignedGT(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildICmp(Builder, LLVMIntPredicate.LLVMIntSGT, left, right, "");
        }

        public LLVMValueRef UnsignedGT(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildICmp(Builder, LLVMIntPredicate.LLVMIntUGT, left, right, "");
        }

        public LLVMValueRef FGT(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildFCmp(Builder, LLVMRealPredicate.LLVMRealUGT, left, right, "");
        }

        public LLVMValueRef SignedGE(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildICmp(Builder, LLVMIntPredicate.LLVMIntSGE, left, right, "");
        }

        public LLVMValueRef UnsignedGE(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildICmp(Builder, LLVMIntPredicate.LLVMIntUGE, left, right, "");
        }

        public LLVMValueRef FGE(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildFCmp(Builder, LLVMRealPredicate.LLVMRealUGE, left, right, "");
        }

        public LLVMValueRef EQ(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildICmp(Builder, LLVMIntPredicate.LLVMIntEQ, left, right, "");
        }

        public LLVMValueRef NE(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildICmp(Builder, LLVMIntPredicate.LLVMIntNE, left, right, "");
        }

        public LLVMValueRef FEQ(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildFCmp(Builder, LLVMRealPredicate.LLVMRealUEQ, left, right, "");
        }

        public LLVMValueRef FNE(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildFCmp(Builder, LLVMRealPredicate.LLVMRealUNE, left, right, "");
        }

        public LLVMValueRef Or(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildOr(Builder, left, right, "");
        }

        public LLVMValueRef Xor(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildXor(Builder, left, right, "");
        }

        public LLVMValueRef And(LLVMValueRef left, LLVMValueRef right)
        {
            return LLVM.BuildAnd(Builder, left, right, "");
        }

        public LLVMValueRef BitNot(LLVMValueRef value)
        {
            return LLVM.BuildNot(Builder, value, "");
        }

        public LLVMValueRef Neg(LLVMValueRef value)
        {
            return LLVM.BuildNeg(Builder, value, "");
        }

        public LLVMValueRef FNeg(LLVMValueRef value)
        {
            return LLVM.BuildFNeg(Builder, value, "");
        }

        public LLVMValueRef SignExtendOrCastInt(LLVMValueRef value, LLVMTypeRef destinationType)
        {
            return LLVM.BuildSExtOrBitCast(Builder, value, destinationType, "");
        }

        public LLVMValueRef ZeroExtendOrCastInt(LLVMValueRef value, LLVMTypeRef destinationType)
        {
            return LLVM.BuildZExtOrBitCast(Builder, value, destinationType, "");
        }

        public LLVMValueRef TruncateOrCastInt(LLVMValueRef value, LLVMTypeRef destinationType)
        {
            return LLVM.BuildTruncOrBitCast(Builder, value, destinationType, "");
        }

        public LLVMValueRef CastInt(LLVMValueRef value, LLVMTypeRef destinationType)
        {
            return LLVM.BuildIntCast(Builder, value, destinationType, "");
        }

        public LLVMValueRef FloatToSigned(LLVMValueRef value, LLVMTypeRef destinationType)
        {
            return LLVM.BuildFPToSI(Builder, value, destinationType, "");
        }

        public LLVMValueRef FloatToUnsigned(LLVMValueRef value, LLVMTypeRef destinationType)
        {
            return LLVM.BuildFPToUI(Builder, value, destinationType, "");
        }

        public LLVMValueRef SignedToFloat(LLVMValueRef value, LLVMTypeRef destinationType)
        {
            return LLVM.BuildSIToFP(Builder, value, destinationType, "");
        }

        public LLVMValueRef UnsignedToFloat(LLVMValueRef value, LLVMTypeRef destinationType)
        {
            return LLVM.BuildUIToFP(Builder, value, destinationType, "");
        }

        public LLVMValueRef TruncateFloat(LLVMValueRef value, LLVMTypeRef destinationType)
        {
            return LLVM.BuildFPTrunc(Builder, value, destinationType, "");
        }

        public LLVMValueRef ExtendFloat(LLVMValueRef value, LLVMTypeRef destinationType)
        {
            return LLVM.BuildFPExt(Builder, value, destinationType, "");
        }

        public LLVMValueRef CallFunction(LLVMValueRef function, Arg[] args)
        {
            return LLVM.BuildCall(Builder, function, args.Select(a => a.Value).ToArray(), "");
        }

        public LLVMValueRef AllocateVariable(LLVMTypeRef type, string name)
        {
            return LLVM.BuildAlloca(Builder, type, name);
        }

        public LLVMValueRef AllocateArray(params LLVMValueRef[] elements)
        {
            var arrayType = LLVM.ArrayType(elements[0].TypeOf(), (uint) elements.Length);
            var array = LLVM.BuildAlloca(Builder, arrayType, "");

            for (uint i = 0; i < elements.Length; i++)
            {
                var pointer = GetElementPointer(array, 0, i);
                Store(elements[i], pointer);
            }

            return array;
        }

        public LLVMValueRef AllocateStruct(params LLVMValueRef[] fields)
        {
            var structType = LLVM.StructType(fields.Select(m => m.TypeOf()).ToArray(), false);
            return AllocateStruct(structType, fields);
        }

        public LLVMValueRef AllocateStruct(LLVMTypeRef structType, params LLVMValueRef[] fields)
        {
            var structVariable = LLVM.BuildAlloca(Builder, structType, "");

            for (var i = 0; i < fields.Length; i++)
            {
                var llvmZero = LLVM.ConstInt(LLVM.Int32Type(), 0, false);
                var llvmIndex = LLVM.ConstInt(LLVM.Int32Type(), (uint) i, false);
                var pointer = LLVM.BuildInBoundsGEP(Builder, structVariable, new[] {llvmZero, llvmIndex}, "");

                Store(fields[i], pointer);
            }

            return structVariable;
        }

        public LLVMValueRef AddGlobalString(string name, string value)
        {
            return LLVM.BuildGlobalString(Builder, value, name);
        }

        public LLVMValueRef ReinterpretCast(LLVMValueRef value, LLVMTypeRef type)
        {
            return LLVM.BuildBitCast(Builder, value, type, "");
        }

        public LLVMValueRef GetElementPointer(LLVMValueRef pointer, params uint[] indices)
        {
            var llvmIndices = indices.Select(i => LLVM.ConstInt(LLVM.Int32Type(), i, false)).ToArray();
            return GetElementPointer(pointer, llvmIndices);
        }

        public LLVMValueRef GetElementPointer(LLVMValueRef pointer, params LLVMValueRef[] indices)
        {
            return LLVM.BuildGEP(Builder, pointer, indices, "");
        }

        public LLVMValueRef GetElementPointerInBounds(LLVMValueRef pointer, params uint[] indices)
        {
            var llvmIndices = indices.Select(i => LLVM.ConstInt(LLVM.Int32Type(), i, false)).ToArray();
            return GetElementPointerInBounds(pointer, llvmIndices);
        }

        public LLVMValueRef GetElementPointerInBounds(LLVMValueRef pointer, params LLVMValueRef[] indices)
        {
            return LLVM.BuildInBoundsGEP(Builder, pointer, indices, "");
        }

        public LLVMValueRef StructField(LLVMValueRef structValue, uint fieldIndex)
        {
            return GetElementPointerInBounds(structValue, 0, fieldIndex);
        }

        public LLVMValueRef Ternary(LLVMValueRef condition, LLVMValueRef trueValue, LLVMValueRef falseValue)
        {
            return LLVM.BuildSelect(Builder, condition, trueValue, falseValue, "");
        }

        public LLVMValueRef Load(LLVMValueRef variable)
        {
            return LLVM.BuildLoad(Builder, variable, "");
        }

        public void Store(LLVMValueRef value, LLVMValueRef variable)
        {
            LLVM.BuildStore(Builder, value, variable);
        }

        public void Branch(LLVMBasicBlockRef block)
        {
            LLVM.BuildBr(Builder, block);
        }

        public void ConditionalBranch(LLVMValueRef condition, LLVMBasicBlockRef trueBlock, LLVMBasicBlockRef falseBlock)
        {
            LLVM.BuildCondBr(Builder, condition, trueBlock, falseBlock);
        }
    }
}

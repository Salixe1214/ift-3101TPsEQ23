using System;
using LLVMSharp;

namespace Ccash.CodeGeneration.LLVMGenerator
{
    public static class BasicBlockExtensions
    {
        public static bool HasTerminator(this LLVMBasicBlockRef block)
        {
            return block.GetBasicBlockTerminator().Pointer != IntPtr.Zero;
        }

        public static LLVMBasicBlockRef AppendBlock(this LLVMBasicBlockRef block, string blockName)
        {
            var nextBlock = block.GetNextBasicBlock();

            if (nextBlock.Pointer == IntPtr.Zero)
            {
                var function = block.GetBasicBlockParent();
                return LLVM.AppendBasicBlock(function, blockName);
            }

            return nextBlock.PrependBlock(blockName);
        }

        public static LLVMBasicBlockRef PrependBlock(this LLVMBasicBlockRef block, string blockName)
        {
            return block.InsertBasicBlock(blockName);
        }
    }
}

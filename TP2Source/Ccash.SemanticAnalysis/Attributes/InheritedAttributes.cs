namespace Ccash.SemanticAnalysis.Attributes
{
    public class InheritedAttributes
    {
        public CodeGeneratorAttribute NextBlock { get; private set; } = new CodeGeneratorAttribute();

        public CodeGeneratorAttribute ConditionBlock { get; private set; } = new CodeGeneratorAttribute();

        public InheritedAttributes WithNextBlock(CodeGeneratorAttribute nextBlock)
        {
            var newAttributes = (InheritedAttributes) MemberwiseClone();
            newAttributes.NextBlock = nextBlock;
            return newAttributes;
        }
        
        public InheritedAttributes WithConditionBlock(CodeGeneratorAttribute conditionBlock)
        {
            var newAttributes = (InheritedAttributes) MemberwiseClone();
            newAttributes.ConditionBlock = conditionBlock;
            return newAttributes;
        }
    }
}
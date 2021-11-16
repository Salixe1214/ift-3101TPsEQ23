namespace Ccash.SemanticAnalysis.Types
{
    public struct Field
    {
        public string Name { get; }
        
        public CcashType Type { get; }

        public Field(string name, CcashType type)
        {
            Name = name;
            Type = type;
        }
    }
}

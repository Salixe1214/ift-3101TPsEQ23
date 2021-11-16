using System;

namespace Ccash.SemanticAnalysis.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SemanticRule : Attribute
    {
        public string Rule { get; }

        public SemanticRule(string rule)
        {
            Rule = rule;
        }
    }
}

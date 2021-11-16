using System.Collections.Generic;
using System.Linq;
using Ccash.SemanticAnalysis.Nodes;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types.Constructors;
using StructDeclarationContext = Ccash.Antlr.CcashParser.StructDeclarationContext;

namespace Ccash.SemanticAnalysis.Types
{
    public class StructType : ValueType
    {
        public override string Name { get; }

        public List<Field> Fields { get; } = new List<Field>();

        public override List<Constructor> Constructors { get; } = new List<Constructor>();

        public ModuleScope ModuleScope { get; }

        public List<FunctionHeader> Methods { get; } = new List<FunctionHeader>();

        protected StructType()
        {
            ModuleScope = new ModuleScope();
        }
        
        public StructType(string name, AbstractScope parent)
        {
            Name = name;
            ModuleScope = new ModuleScope(parent);
        }

        public override bool CanBeCoerced(CcashType destination)
        {
            if (Constructors.Any())
            {
                return base.CanBeCoerced(destination);
            }

            return this == destination;
        }

        public override bool Equals(CcashType other)
        {
            return base.Equals(other)
                   && other is StructType otherStruct
                   && Name == otherStruct.Name;
        }

        public void AddMembers(StructDeclarationContext context)
        {
            foreach (var fieldContext in context.field())
            {
                if (Fields.Any(n => n.Name == fieldContext.Identifier().GetText()))
                {
                    ErrorManager.AddError(fieldContext,
                                          $"field with name `{fieldContext.Identifier().GetText()}` is already declared");
                }
                else
                {
                    var field = new Field(fieldContext.Identifier().GetText(), Create(fieldContext.type()));
                    Fields.Add(field);
                    ModuleScope.AddStructField(field.Name, field.Type);
                }
            }

            Methods.AddRange(context.method().Select(m => new FunctionHeader(this, m.methodHeader())));
            foreach (var functionHeader in Methods)
            {
                var firstArg = functionHeader.FunctionType.ParameterTypes.FirstOrDefault();
                if (firstArg != null && firstArg is ReferenceType refType && refType.ReferredType == this)
                {
                    ModuleScope.AddFunction(functionHeader);
                }
                else
                {
                    ModuleScope.Parent.AddFunction(functionHeader);
                }
            }
        }
    }
}

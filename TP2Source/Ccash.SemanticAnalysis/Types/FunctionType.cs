using System.Collections.Generic;
using System.Linq;
using Ccash.SemanticAnalysis.Types.Constructors;
using FunctionHeaderContext = Ccash.Antlr.CcashParser.FunctionHeaderContext;
using MethodHeaderContext = Ccash.Antlr.CcashParser.MethodHeaderContext;

namespace Ccash.SemanticAnalysis.Types
{
    public class FunctionType : CcashType
    {
        public override string Name => $"func({string.Join(", ", ParameterTypes)}) {ReturnType?.ToString() ?? ""}";

        public override List<Constructor> Constructors => new List<Constructor>();

        public List<CcashType> ParameterTypes { get; }

        public CcashType ReturnType { get; }

        public FunctionType(CcashType returnType, params CcashType[] parameterTypes)
        {
            ParameterTypes = parameterTypes.ToList();
            ReturnType = returnType;
        }

        public FunctionType(FunctionHeaderContext context)
        {
            ParameterTypes = context.functionParameters()?.variable()?.Select(Create).ToList() ?? new List<CcashType>();
            ReturnType = context.type() == null ? Void : Create(context.type());
        }

        public FunctionType(StructType ownerType, MethodHeaderContext context)
        {
            ParameterTypes = new List<CcashType>();
            if (context.Static() == null)
            {
                ParameterTypes.Add(context.Mut() == null ? ownerType.ConstRef : ownerType.MutRef);
            }
            
            ParameterTypes.AddRange(context.functionParameters()?.variable()?.Select(Create) ?? new CcashType[]{});
            ReturnType = context.type() == null ? Void : Create(context.type());
        }

        public override bool Equals(CcashType other)
        {
            return base.Equals(other)
                   && other is FunctionType otherFunc
                   && ParameterTypes.SequenceEqual(otherFunc.ParameterTypes)
                   && ReturnType == otherFunc.ReturnType;
        }
    }
}

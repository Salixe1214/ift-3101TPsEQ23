using System;
using System.Collections.Generic;
using System.Linq;
using Ccash.SemanticAnalysis.Nodes;
using Ccash.SemanticAnalysis.Nodes.Expressions;
using Ccash.SemanticAnalysis.Types.Constructors;
using TypeContext = Ccash.Antlr.CcashParser.TypeContext;
using VariableContext = Ccash.Antlr.CcashParser.VariableContext;
using ValueTypeContext = Ccash.Antlr.CcashParser.ValueTypeContext;
using IntegralTypeContext = Ccash.Antlr.CcashParser.IntegralTypeContext;
using FloatTypeContext = Ccash.Antlr.CcashParser.FloatTypeContext;
using BoolTypeContext = Ccash.Antlr.CcashParser.BoolTypeContext;
using ArrayTypeContext = Ccash.Antlr.CcashParser.ArrayTypeContext;
using ReferenceTypeContext = Ccash.Antlr.CcashParser.ReferenceTypeContext;
using StructTypeContext = Ccash.Antlr.CcashParser.StructTypeContext;

namespace Ccash.SemanticAnalysis.Types
{
    public abstract class CcashType : IEquatable<CcashType>
    {
        public abstract string Name { get; }

        public abstract List<Constructor> Constructors { get; }

        public static CcashType Create(VariableContext context)
        {
            if (context.variableType().valueType() != null)
            {
                return Create(context.variableType().valueType());
            }

            if (context.variableType().referenceType() != null)
            {
                return Create(context.variableType().referenceType(), context.variableType().Mut() != null);
            }

            if (context.variableType().arrayType() != null)
            {
                return Create(context.variableType().arrayType(), context.variableType().Mut() != null);
            }

            throw new NotImplementedException();
        }

        private static CcashType Create(ArrayTypeContext context, bool isMutable)
        {
            CcashType type;
            if (context.valueType() != null)
            {
                type = Create(context.valueType());
            }
            else if (context.referenceType() != null)
            {
                type = Create(context.referenceType(), isMutable);
            }
            else
            {
                type = Create(context.arrayType(), isMutable);
            }

            return new ArrayType(type, isMutable);
        }

        protected static CcashType Create(TypeContext context)
        {
            if (context.valueType() != null)
            {
                return Create(context.valueType());
            }

            if (context.referenceType() != null)
            {
                return Create(context.referenceType(), context.Mut() != null);
            }

            if (context.arrayType() != null)
            {
                return Create(context.arrayType(), context.Mut() != null);
            }

            throw new NotImplementedException();
        }

        private static ValueType Create(ValueTypeContext context)
        {
            switch (context)
            {
                case IntegralTypeContext integralTypeContext:
                    return new IntegerType(integralTypeContext.GetText());
                case FloatTypeContext floatTypeContext:
                    return new FloatType(floatTypeContext.GetText());
                case BoolTypeContext _:
                    return Boolean;
                case StructTypeContext structTypeContext:
                {
                    var structName = structTypeContext.Identifier().GetText();
                    if (Structs.ContainsKey(structName))
                    {
                        return Structs[structName];
                    }

                    ErrorManager.AddError(context, $"no such type `{structName}`");
                    return Void;
                }
                default:
                    throw new NotImplementedException();
            }
        }

        private static ReferenceType Create(ReferenceTypeContext context, bool isMutable)
        {
            return new ReferenceType(Create(context.valueType()), isMutable);
        }

        public FunctionCallExpression CallConstructor(IExpression argument)
        {
            return new FunctionCallExpression(this, argument);
        }

        public virtual bool CanBeCoerced(CcashType destination) =>
            destination.Constructors.Any(c => c is ImplicitConstructor &&
                                              c.FunctionType.ParameterTypes.Count == 1 &&
                                              c.FunctionType.ParameterTypes.First() == this);


        public override string ToString()
        {
            return Name;
        }

        public virtual bool Equals(CcashType other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (GetType() != other.GetType()) return false;
            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CcashType);
        }

        public static bool operator ==(CcashType left, CcashType right)
        {
            return ReferenceEquals(left, right)
                   || !ReferenceEquals(left, null)
                   && !ReferenceEquals(right, null)
                   && left.Equals(right);
        }

        public static bool operator !=(CcashType right, CcashType left)
        {
            return !(right == left);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public static Dictionary<string, StructType> Structs { get; } = new Dictionary<string, StructType>();
        public static BooleanType Boolean => new BooleanType();
        public static IntegerType Int8 => new IntegerType(8, true);
        public static IntegerType Uint8 => new IntegerType(8, false);
        public static IntegerType Int16 => new IntegerType(16, true);
        public static IntegerType Uint16 => new IntegerType(16, false);
        public static IntegerType Int32 => new IntegerType(32, true);
        public static IntegerType Uint32 => new IntegerType(32, false);
        public static IntegerType Int64 => new IntegerType(64, true);
        public static IntegerType Uint64 => new IntegerType(64, false);
        public static FloatType Float32 => new FloatType(32);
        public static FloatType Float64 => new FloatType(64);
        public static ArrayType ConstString => new ArrayType(Uint8, false);
        public static VoidType Void => new VoidType();

        public static readonly IntegerType[] IntegerTypes =
        {
            Int8, Uint8, Int16, Uint16, Int32, Uint32, Int64, Uint64
        };

        public static readonly FloatType[] FloatTypes = {Float32, Float64};

        public static readonly CcashType[] AllPrimitives =
            new[] {Boolean}.Concat(IntegerTypes.Concat<CcashType>(FloatTypes)).ToArray();

        public static FunctionHeader[] NativeOperators { get; }

        static CcashType()
        {
            NativeOperators = GenerateOperators();
        }

        private static FunctionHeader[] GenerateOperators()
        {
            var operators = new List<FunctionHeader>();

            foreach (var op in new[] {"+", "-", "*", "/"})
            {
                operators.AddRange(IntegerTypes.Select(type => CreateOperator(op, type, type, type)));
                operators.AddRange(FloatTypes.Select(type => CreateOperator(op, type, type, type)));
            }

            foreach (var op in new[] {"+=", "-=", "*=", "/="})
            {
                operators.AddRange(IntegerTypes.Select(type => CreateOperator(op, Void, type.MutRef, type)));
                operators.AddRange(FloatTypes.Select(type => CreateOperator(op, Void, type.MutRef, type)));
            }

            foreach (var op in new[] {"<", "<=", ">", ">="})
            {
                operators.AddRange(IntegerTypes.Select(type => CreateOperator(op, Boolean, type, type)));
                operators.AddRange(FloatTypes.Select(type => CreateOperator(op, Boolean, type, type)));
            }

            foreach (var op in new[] {"==", "!="})
            {
                operators.Add(CreateOperator(op, Boolean, Boolean, Boolean));
                operators.AddRange(IntegerTypes.Select(type => CreateOperator(op, Boolean, type, type)));
                operators.AddRange(FloatTypes.Select(type => CreateOperator(op, Boolean, type, type)));
            }

            foreach (var op in new[] {"&", "|", "^"})
            {
                operators.Add(CreateOperator(op, Boolean, Boolean, Boolean));
                operators.AddRange(IntegerTypes.Select(type => CreateOperator(op, type, type, type)));
            }

            operators.AddRange(new[] {"&&", "||"}.Select(op => CreateOperator(op, Boolean, Boolean, Boolean)));

            foreach (var op in new[] {"&=", "|=", "^="})
            {
                operators.Add(CreateOperator(op, Void, Boolean.MutRef, Boolean));
                operators.AddRange(IntegerTypes.Select(type => CreateOperator(op, Void, type.MutRef, type)));
            }

            foreach (var op in new[] {"--", "++"})
            {
                operators.AddRange(IntegerTypes.Select(type => CreateOperator(op, Void, type.MutRef)));
            }

            operators.Add(CreateOperator(":=", Void, Boolean.MutRef, Boolean));
            operators.AddRange(IntegerTypes.Select(type => CreateOperator(":=", Void, type.MutRef, type)));
            operators.AddRange(FloatTypes.Select(type => CreateOperator(":=", Void, type.MutRef, type)));

            operators.Add(CreateOperator(":=:", Void, Boolean.MutRef, Boolean.MutRef));
            operators.AddRange(IntegerTypes.Select(type => CreateOperator(":=:", Void, type.MutRef, type.MutRef)));
            operators.AddRange(FloatTypes.Select(type => CreateOperator(":=:", Void, type.MutRef, type.MutRef)));

            operators.AddRange(IntegerTypes.Select(type => CreateOperator("%", type, type, type)));
            operators.AddRange(IntegerTypes.Select(type => CreateOperator("%=", Void, type.MutRef, type)));

            operators.Add(CreateOperator("!", Boolean, Boolean));
            operators.AddRange(IntegerTypes.Select(type => CreateOperator("!", type, type)));

            operators.AddRange(IntegerTypes.Select(type => CreateOperator("-", type, type)));
            operators.AddRange(FloatTypes.Select(type => CreateOperator("-", type, type)));

            return operators.ToArray();
        }

        private static FunctionHeader CreateOperator(string op, CcashType returnType, params CcashType[] parameters)
        {
            return new FunctionHeader($"operator{op}", returnType, parameters);
        }
    }
}

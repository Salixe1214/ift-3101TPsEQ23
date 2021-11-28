using System;
using Ccash.SemanticAnalysis.Types;
using IntegerLiteralExpressionContext = Ccash.Antlr.CcashParser.IntegerLiteralExpressionContext;

namespace Ccash.SemanticAnalysis.Nodes.Expressions.Literals
{
    public sealed class IntegerLiteralExpression : IRvalueExpression
    {
        public string Text { get; }

        public CcashType Type { get; }

        public IntegerType IntegerType => (IntegerType) Type;

        private ulong Value { get; }

        /// Unboxes Value and converts it to ulong
        public ulong UnsignedValue => Value;

        /// Unboxes Value and converts it to long
        public long SignedValue => (long) Value;

        public IntegerLiteralExpression(ulong value, IntegerType intType): this(value.ToString(), intType)
        {
        }

        public IntegerLiteralExpression(long value, IntegerType intType): this(value.ToString(), intType)
        {
        }

        public IntegerLiteralExpression(IntegerLiteralExpressionContext context) : this(context, CcashType.Int32)
        {
        }

        public IntegerLiteralExpression(IntegerLiteralExpressionContext context, IntegerType intType)
            : this(context.IntegerLiteral().GetText(), intType)
        {
        }

        public bool FitsInto(IntegerType targetType)
        {
            if (IntegerType.IsSigned && SignedValue < 0)
            {
                if (!targetType.IsSigned)
                    return false;

                switch (targetType.Size)
                {
                    case 8:
                        return sbyte.MinValue <= SignedValue;
                    case 16:
                        return short.MinValue <= SignedValue;
                    case 32:
                        return int.MinValue <= SignedValue;
                    case 64:
                        return true;
                    default:
                        throw new ArgumentException($"{targetType} is not supported as a literal type");
                }
            }

            if (targetType.IsSigned)
            {
                switch (targetType.Size)
                {
                    case 8:
                        return UnsignedValue <= (ulong) sbyte.MaxValue;
                    case 16:
                        return UnsignedValue <= (ulong) short.MaxValue;
                    case 32:
                        return UnsignedValue <= int.MaxValue;
                    case 64:
                        return UnsignedValue <= long.MaxValue;
                    default:
                        throw new ArgumentException($"{targetType} is not supported as a literal type");
                }
            }

            switch (targetType.Size)
            {
                case 8:
                    return UnsignedValue <= byte.MaxValue;
                case 16:
                    return UnsignedValue <= ushort.MaxValue;
                case 32:
                    return UnsignedValue <= uint.MaxValue;
                case 64:
                    return true;
                default:
                    throw new ArgumentException($"{targetType} is not supported as a literal type");
            }
        }

        private IntegerLiteralExpression(string value, IntegerType intType)
        {
            Type = intType;
            Text = value;

            if (intType.IsSigned)
            {
                switch (intType.Size)
                {
                    case 8:
                        Value = (ulong) sbyte.Parse(Text);
                        break;
                    case 16:
                        Value = (ulong) short.Parse(Text);
                        break;
                    case 32:
                        Value = (ulong) int.Parse(Text);
                        break;
                    case 64:
                        Value = (ulong) long.Parse(Text);
                        break;
                    default:
                        throw new ArgumentException($"{intType} is not supported as a literal type");
                }
            }
            else
            {
                switch (intType.Size)
                {
                    case 8:
                        Value = byte.Parse(Text);
                        break;
                    case 16:
                        Value = ushort.Parse(Text);
                        break;
                    case 32:
                        Value = uint.Parse(Text);
                        break;
                    case 64:
                        Value = ulong.Parse(Text);
                        break;
                    default:
                        throw new ArgumentException($"{intType} is not supported as a literal type");
                }
            }
        }
    }
}

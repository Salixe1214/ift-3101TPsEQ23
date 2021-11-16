using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Ccash.SemanticAnalysis.Nodes;
using Ccash.SemanticAnalysis.Nodes.Expressions;
using Ccash.SemanticAnalysis.Types;

namespace Ccash.SemanticAnalysis
{
    public static class ErrorManager
    {
        public static string CurrentFileName { get; set; }

        public static bool HasErrors => Errors.Count > 0;

        private static List<string> Errors { get; } = new List<string>();

        private static List<string> Warnings { get; } = new List<string>();

        public static void PrintErrors()
        {
            var builder = new StringBuilder();
            Errors.ForEach(s => builder.AppendLine(s));
            Console.Write(builder.ToString());
        }

        public static void PrintWarnings()
        {
            var builder = new StringBuilder();
            Warnings.ForEach(s => builder.AppendLine(s));
            Console.Write(builder.ToString());
        }

        public static void MismatchedTypes(ParserRuleContext context, CcashType expected, CcashType actual, string hint = "")
        {
            const string error = "mismatched types";
            var details = $"expected `{expected}`, found `{actual}`";
            AddError(context, error, details, hint);
        }

        public static void IdentifierNotInScope(ParserRuleContext context, string identifier)
        {
            var error = $"cannot find identifier `{identifier}` in this scope";
            AddError(context, error);
        }

        public static void NoValidOverload(ParserRuleContext context, string identifier, params IExpression[] arguments)
        {
            var error = $"no suitable overload for {identifier}";
            var details = $"with argument types ({string.Join(", ", arguments.Select(a => a.Type))})";
            AddError(context, error, details);
        }

        public static void AmbiguousCall(ParserRuleContext context,
                                         List<FunctionHeader> overloads,
                                         params IExpression[] arguments)
        {
            var identifier = overloads.First().Name;
            var error = $"ambiguous call to {identifier}, cannot resolve which overload to call";
            var details = $"with argument types ({string.Join(", ", arguments.Select(a => a.Type))})";
            const string tab = "       ";
            details +=
                $"\n{tab}candidate overloads are:\n{tab}{string.Join($"\n{tab}", overloads.Select(o => o.FullName))}";

            AddError(context, error, details);
        }

        public static void WrongArity(ParserRuleContext context, string identifier, int expectedArity, int actualArity)
        {
            var argument = expectedArity == 1 ? "argument" : "arguments";
            var expected = expectedArity == 0 ? "no" : expectedArity.ToString();

            var was = actualArity == 1 ? "was" : "were";
            var actual = actualArity == 0 ? "none" : actualArity.ToString();
            string error = $"`{identifier}` takes {expected} {argument}, but {actual} {was} supplied";

            AddError(context, error);
        }

        public static void DuplicateIdentifier(ParserRuleContext context, string identifier)
        {
            AddError(context, "Duplicate definitions of identifier", $"identifier `{identifier}`");
        }

        public static void AddSyntaxError(string error, int line, int column)
        {
            Errors.Add(
                       $@"ERROR: {error}
    --> {CurrentFileName}:{line}:{column}");
        }

        public static void AddError(ParserRuleContext context, string error, string details = "", string hint = "")
        {
            var line = (uint) context.Start.Line;
            var column = (uint) context.Start.Column;
            details = string.IsNullOrEmpty(details) ? "" : $"|  ^^^^ {details}";
            hint = string.IsNullOrEmpty(hint) ? "" : $"|       {hint}\n";
            Errors.Add(
                $@"ERROR: {error}
    --> {CurrentFileName}:{line}:{column}
      |
      |  {ExtractText(context)}
      {details}
      {hint}");
        }
        
        public static void AddWarning(ParserRuleContext context, string warning, string details = "")
        {
            var line = (uint) context.Start.Line;
            var column = (uint) context.Start.Column;
            details = string.IsNullOrEmpty(details) ? "" : $"|  ^^^^ {details}\n";

            Warnings.Add(
                $@"WARNING: {warning}
    --> {CurrentFileName}:{line}:{column}
      |
      |  {ExtractText(context)}
      {details}");
        }

        private static string ExtractText(ParserRuleContext context)
        {
            var start = context.Start.StartIndex;
            var stop = context.Stop.StopIndex;
            var inputStream = context.Start.InputStream;
            var text = inputStream.GetText(new Interval(start, stop));

            if (text.Contains('\n'))
            {
                var indentation = context.Start.Column;
                var lines = text.Split('\n');
                var builder = new StringBuilder(lines.First());
                foreach (var line in lines.Skip(1))
                {
                    builder.Append("\n      |  ");
                    foreach (var c in line.Skip(indentation))
                    {
                        builder.Append(c);
                    }
                }

                return builder.ToString();
            }

            return text;
        }
    }
}

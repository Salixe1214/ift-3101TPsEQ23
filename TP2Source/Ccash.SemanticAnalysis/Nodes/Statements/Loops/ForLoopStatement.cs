using System;
using System.Linq;
using Ccash.SemanticAnalysis.Attributes;
using Ccash.SemanticAnalysis.Nodes.Expressions;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;
using ForStatementContext = Ccash.Antlr.CcashParser.ForStatementContext;
using ForInitializationContext = Ccash.Antlr.CcashParser.ForInitializationContext;
using ForUpdateContext = Ccash.Antlr.CcashParser.ForUpdateContext;

namespace Ccash.SemanticAnalysis.Nodes.Statements.Loops
{
    [SemanticRule("this.ConditionBlock = codegenblock()")]
    [SemanticRule("this.NextBlock = codegenblock()")]
    [SemanticRule("Statements.inherited = {this.NextBlock, this.ConditionBlock, ..this.inherited}")]
    [SemanticRule("this.AlwaysReturns = false")]
    public class ForLoopStatement : LoopStatement
    {
        public IStatement Declaration { get; }

        public IStatement Assignment { get; }

        public ForLoopStatement(ForStatementContext context, AbstractScope parent, InheritedAttributes inheritedAttributes) :
            base(parent)
        {
            if (context.forHeader().forInitialization() != null)
            {
                Declaration = CreateStatement(context.forHeader().forInitialization());
            }

            var expression = context.forHeader().expression();
            ConditionExpression = expression == null ? BooleanType.True : ExpressionFactory.Create(expression, this);

            if (ConditionExpression.Type.CanBeCoerced(CcashType.Boolean))
            {
                ConditionExpression = ExpressionFactory.Coerce(ConditionExpression, CcashType.Boolean);
            }
            else
            {
                ErrorManager.MismatchedTypes(context.forHeader(), CcashType.Boolean, ConditionExpression.Type);
            }
            var childrenAttributes = inheritedAttributes.WithConditionBlock(ConditionBlock).WithNextBlock(NextBlock);
            Statements = context.loopBlock()
                                .statement()
                                .Select(s => StatementFactory.Create(s, this, childrenAttributes))
                                .ToList();

            if (context.forHeader().forUpdate() != null)
            {
                Assignment = CreateStatement(context.forHeader().forUpdate());
            }
            inheritedAttributes.NextBlock.Data = NextBlock.Data;
            inheritedAttributes.ConditionBlock.Data = ConditionBlock.Data;
        }

        private IStatement CreateStatement(ForInitializationContext context)
        {
            if (context.variableDeclaration() != null)
            {
                return new VariableDeclaration(context.variableDeclaration(), this);
            }

            if (context.reassignment() != null)
            {
                return new FunctionCallStatement(context.reassignment(), this);
            }

            if (context.functionCall() != null)
            {
                return new FunctionCallStatement(context.functionCall(), this);
            }

            throw new NotImplementedException(
                $"{context.GetType()} is not yet implemented in `for` loop initialization");
        }

        private IStatement CreateStatement(ForUpdateContext context)
        {
            if (context.reassignment() != null)
            {
                return new FunctionCallStatement(context.reassignment(), this);
            }

            if (context.functionCall() != null)
            {
                return new FunctionCallStatement(context.functionCall(), this);
            }

            throw new NotImplementedException($"{context.GetType()} is not yet implemented in `for` loop update");
        }
    }
}

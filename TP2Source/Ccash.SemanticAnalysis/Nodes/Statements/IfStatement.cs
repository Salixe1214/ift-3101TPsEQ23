using System.Collections.Generic;
using System.Linq;
using Ccash.Antlr;
using Ccash.SemanticAnalysis.Attributes;
using Ccash.SemanticAnalysis.Nodes.Expressions;
using Ccash.SemanticAnalysis.Nodes.Expressions.Literals;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;
using ElseIfStatementContext = Ccash.Antlr.CcashParser.ElseIfStatementContext;
using ElseStatementContext = Ccash.Antlr.CcashParser.ElseStatementContext;

namespace Ccash.SemanticAnalysis.Nodes.Statements
{
    [SemanticRule("ElseIfStatements.inherited = this.inherited")]
    [SemanticRule("Statements.inherited = this.inherited")]
    [SemanticRule("this.AlwaysReturns = Statements.AlwaysReturns")]
    public class IfStatement : BlockScope, IStatement
    {
        public bool AlwaysReturns { get; }

        public IExpression Expression { get; }

        public List<ElseIfStatement> ElseIfStatements { get; } = new List<ElseIfStatement>();

        public ElseStatement ElseStatement { get; }

        public IfStatement(CcashParser.IfStatementContext context,
                           AbstractScope parent,
                           InheritedAttributes inheritedAttributes) : base(parent)
        {
            Expression = ExpressionFactory.Create(context.expression(), parent);
            if (Expression.Type.CanBeCoerced(CcashType.Boolean))
            {
                Expression = ExpressionFactory.Coerce(Expression, CcashType.Boolean);
            }
            else
            {
                ErrorManager.MismatchedTypes(context, CcashType.Boolean, Expression.Type);
            }


            if (context.elseIfStatement() != null)
            {
                ElseIfStatements = context.elseIfStatement()
                                          .Select(e => new ElseIfStatement(e, parent, inheritedAttributes))
                                          .ToList();
            }

            Statements = context.block()
                                .statement()
                                .Select(s => StatementFactory.Create(s, this, inheritedAttributes))
                                .ToList();

            if (context.elseStatement() != null)
            {
                ElseStatement = new ElseStatement(context.elseStatement(), parent, inheritedAttributes);
            }

            AlwaysReturns = Statements.Any(s => s.AlwaysReturns);
            if (AlwaysReturns && Statements.First(s => s.AlwaysReturns) != Statements.Last())
            {
                ErrorManager.AddError(context, "Unreachable code after return");
            }

            AlwaysReturns &= (ElseStatement?.AlwaysReturns ?? false)
                             && ElseIfStatements.All(s => s.AlwaysReturns);
        }
    }

    [SemanticRule("Statements.inherited = this.inherited")]
    public class ElseIfStatement : BlockScope, IStatement
    {
        public bool AlwaysReturns { get; protected set; }

        public IExpression Expression { get; }

        protected ElseIfStatement(AbstractScope parent) : base(parent)
        {
            Expression = new BooleanLiteralExpression(true);
        }

        public ElseIfStatement(ElseIfStatementContext context, AbstractScope parent, InheritedAttributes inheritedAttributes) :
            base(parent)
        {
            if (context.expression() != null)
            {
                Expression = ExpressionFactory.Create(context.expression(), parent);
                if (!(Expression.Type is BooleanType))
                {
                    ErrorManager.MismatchedTypes(context, CcashType.Boolean, Expression.Type);
                }
            }

            Statements = context.block()
                                .statement()
                                ?.Select(s => StatementFactory.Create(s, this, inheritedAttributes))
                                .ToList();

            AlwaysReturns = Statements.Any(s => s.AlwaysReturns);
            if (AlwaysReturns && Statements.First(s => s.AlwaysReturns) != Statements.Last())
            {
                ErrorManager.AddError(context, "Unreachable code after `return`");
            }
        }
    }

    public class ElseStatement : ElseIfStatement
    {
        public ElseStatement(ElseStatementContext context, AbstractScope parent, InheritedAttributes inheritedAttributes)
            : base(parent)
        {
            Statements = context.block()
                                .statement()
                                ?.Select(s => StatementFactory.Create(s, this, inheritedAttributes))
                                .ToList();

            AlwaysReturns = Statements.Any(s => s.AlwaysReturns);
            if (AlwaysReturns && Statements.First(s => s.AlwaysReturns) != Statements.Last())
            {
                ErrorManager.AddError(context, "Unreachable code after `return`");
            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Ccash.Antlr;
using Ccash.SemanticAnalysis.Attributes;
using Ccash.SemanticAnalysis.Nodes.Expressions;
using Ccash.SemanticAnalysis.Nodes.Expressions.Literals;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;
using CaseStatementContext = Ccash.Antlr.CcashParser.CaseStatementContext;
using DefaultStatementContext = Ccash.Antlr.CcashParser.DefaultStatementContext;

namespace Ccash.SemanticAnalysis.Nodes.Statements
{
    [SemanticRule("ElseSwitchStatements.inherited = this.inherited")]
    [SemanticRule("Statements.inherited = this.inherited")]
    [SemanticRule("this.AlwaysReturns = Statements.AlwaysReturns")]
    public class SwitchStatement : BlockScope, IStatement
    {
        public bool AlwaysReturns { get; }

        public bool defaultCase { get; set; } = true;

        public bool FallthroughBool { get; } = false;

        public IExpression Expression { get; }

        public CodeGeneratorAttribute NextBlock { get; set; } = new CodeGeneratorAttribute();

        public List<CaseStatement> CaseStatements { get; } = new List<CaseStatement>();

        public DefaultStatement DefaultStatement { get; }

        public SwitchStatement(CcashParser.SwitchStatementContext context,
                           AbstractScope parent,
                           InheritedAttributes inheritedAttributes) : base(parent)
        {
            Expression = ExpressionFactory.Create(context.expression(), parent);
            if (Expression.Type.CanBeCoerced(CcashType.Int64))
            {
                Expression = ExpressionFactory.Coerce(Expression, CcashType.Int64);
            }
            else if(Expression.Type.CanBeCoerced(CcashType.Boolean))
            {
                Expression = ExpressionFactory.Coerce(Expression, CcashType.Int64);
            }
            else
            {
                ErrorManager.MismatchedTypes(context, CcashType.Int64, Expression.Type);
            }

            if (context.caseStatement() != null)
            {
                CaseStatements = context.caseStatement()
                                          .Select(e => new CaseStatement(e, parent, inheritedAttributes))
                                          .ToList();
            }

            if (context.defaultStatement() != null)
            {
                DefaultStatement = new DefaultStatement(context.defaultStatement(), parent, inheritedAttributes);
            }

            inheritedAttributes.NextBlock.Data = NextBlock.Data;

            AlwaysReturns = Statements.Any(s => s.AlwaysReturns);
            if (AlwaysReturns && Statements.First(s => s.AlwaysReturns) != Statements.Last())
            {
                ErrorManager.AddError(context, "Unreachable code after return");
            }

            FallthroughBool = context.children.First().GetText() == "fallthrough";
            
            var childrenAttributes = inheritedAttributes.WithNextBlock(NextBlock);

            AlwaysReturns &= (DefaultStatement?.AlwaysReturns ?? false)
                             && CaseStatements.All(s => s.AlwaysReturns);

        }
    }

    [SemanticRule("Statements.inherited = this.inherited")]
    public class CaseStatement : BlockScope, IStatement
    {
        public bool AlwaysReturns { get; protected set; }

        public IExpression Expression { get; }

        public CodeGeneratorAttribute NextBlock { get; set; } = new CodeGeneratorAttribute();

        public CodeGeneratorAttribute NextCase { get; set; } = new CodeGeneratorAttribute();

        protected CaseStatement(AbstractScope parent) : base(parent)
        {
            Expression = new IntegerLiteralExpression(0, CcashType.Int64);
        }

        public CaseStatement(CaseStatementContext context, AbstractScope parent, InheritedAttributes inheritedAttributes) :
            base(parent)
        {
            if (context.expression() != null)
            {
                Expression = ExpressionFactory.Create(context.expression(), parent);
                if (Expression.Type.CanBeCoerced(CcashType.Int64))
                {
                    Expression = ExpressionFactory.Coerce(Expression, CcashType.Int64);
                }
                else if (Expression.Type.CanBeCoerced(CcashType.Boolean))
                {
                    Expression = ExpressionFactory.Coerce(Expression, CcashType.Int64);
                }
                else
                {
                    ErrorManager.MismatchedTypes(context, CcashType.Int64, Expression.Type);
                }
            }

            inheritedAttributes.NextBlock.Data = NextBlock.Data;
            var childrenAttributes = inheritedAttributes.WithNextBlock(NextBlock);

            Statements = context.block()
                                .statement()
                                ?.Select(s => StatementFactory.Create(s, this, childrenAttributes))
                                .ToList();

            AlwaysReturns = Statements.Any(s => s.AlwaysReturns);
            if (AlwaysReturns && Statements.First(s => s.AlwaysReturns) != Statements.Last())
            {
                ErrorManager.AddError(context, "Unreachable code after `return`");
            }
        }
    }

    public class DefaultStatement : CaseStatement
    {
        public DefaultStatement(DefaultStatementContext context, AbstractScope parent, InheritedAttributes inheritedAttributes)
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

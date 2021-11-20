using System;
using Ccash.SemanticAnalysis.Attributes;
using Ccash.SemanticAnalysis.Nodes.Statements.ControlFlow;
using Ccash.SemanticAnalysis.Nodes.Statements.Loops;
using Ccash.SemanticAnalysis.SymbolTable;
using StatementContext = Ccash.Antlr.CcashParser.StatementContext;
using ReassignmentStatementContext = Ccash.Antlr.CcashParser.ReassignmentStatementContext;
using FunctionCallStatementContext = Ccash.Antlr.CcashParser.FunctionCallStatementContext;
using VariableDeclarationStatementContext = Ccash.Antlr.CcashParser.VariableDeclarationStatementContext;
using ConditionalStatementContext = Ccash.Antlr.CcashParser.ConditionalStatementContext;
using LoopStatementContext = Ccash.Antlr.CcashParser.LoopStatementContext;
using ControlFlowStatementContext = Ccash.Antlr.CcashParser.ControlFlowStatementContext;
using BlockStatementContext = Ccash.Antlr.CcashParser.BlockStatementContext;
using MethodCallStatementContext = Ccash.Antlr.CcashParser.MethodCallStatementContext;

namespace Ccash.SemanticAnalysis.Nodes.Statements
{
    public interface IStatement
    {
        bool AlwaysReturns { get; }
    }
    
    public static class StatementFactory
    {
        public static IStatement Create(StatementContext context, AbstractScope scope, InheritedAttributes inheritedAttributes)
        {
            switch (context)
            {
                case ReassignmentStatementContext reassignmentStatementContext:
                    return new FunctionCallStatement(reassignmentStatementContext.reassignment(), scope);

                case FunctionCallStatementContext functionCallContext:
                    return new FunctionCallStatement(functionCallContext.functionCall(), scope);

                case VariableDeclarationStatementContext variableDeclarationContext:
                    return new VariableDeclaration(variableDeclarationContext.variableDeclaration(), scope);

                case ConditionalStatementContext conditionalStatement:
                    return Create(conditionalStatement, scope, inheritedAttributes);

                case LoopStatementContext loopStatement:
                    return Create(loopStatement, scope, inheritedAttributes);

                case ControlFlowStatementContext controlFlowStatement:
                    return Create(controlFlowStatement, scope, inheritedAttributes);

                case BlockStatementContext blockStatementContext:
                    return new BlockStatement(blockStatementContext.block(), scope, inheritedAttributes);
                
                case MethodCallStatementContext methodCallContext:
                    return new MethodCallStatement(methodCallContext, scope);
            }

            throw new NotImplementedException($"{context.GetType()} is not yet implemented");
        }

        private static IStatement Create(ConditionalStatementContext context,
                                         AbstractScope scope,
                                         InheritedAttributes inheritedAttributes)
        {
            if (context.ifStatement() != null)
            {
                return new IfStatement(context.ifStatement(), scope, inheritedAttributes);
            }
            
            throw new NotImplementedException($"{context.GetType()} is not yet implemented");
        }

        private static IStatement Create(LoopStatementContext context,
                                         AbstractScope scope,
                                         InheritedAttributes inheritedAttributes)
        {
            if (context.whileStatement() != null)
            {
                return new WhileStatement(context.whileStatement(), scope, inheritedAttributes);
            }
            if (context.doStatement() != null)
            {
                return new DoStatement(context.doStatement(), scope, inheritedAttributes);
            }
            if (context.forStatement() != null)
            {
                return new ForLoopStatement(context.forStatement(), scope, inheritedAttributes);
            }
            if (context.repeatStatement() != null)
            {
                return new RepeatStatement(context.repeatStatement(), scope, inheritedAttributes);
            }
            

            throw new NotImplementedException($"{context.GetType()} is not yet implemented");
        }

        private static IStatement Create(ControlFlowStatementContext context,
                                         AbstractScope scope,
                                         InheritedAttributes inheritedAttributes)
        {
            if (context.returnStatement() != null)
            {
                return new ReturnStatement(context.returnStatement(), scope);
            }
            if (context.breakStatement() != null)
            {
                return new BreakStatement(context.breakStatement(), scope, inheritedAttributes);
            }
            if (context.continueStatement() != null)
            {
                return new ContinueStatement(context.continueStatement(), scope, inheritedAttributes);
            }

            throw new NotImplementedException($"{context.GetType()} is not yet implemented");
        }
    }
}

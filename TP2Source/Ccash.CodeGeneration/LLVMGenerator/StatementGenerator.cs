using System;
using System.Collections.Generic;
using System.Linq;
using Ccash.CodeGeneration.LLVMGenerator.Intrinsics;
using Ccash.SemanticAnalysis.Nodes.Statements;
using Ccash.SemanticAnalysis.Nodes.Statements.ControlFlow;
using Ccash.SemanticAnalysis.Nodes.Statements.Loops;
using Ccash.SemanticAnalysis.SymbolTable;
using Ccash.SemanticAnalysis.Types;
using LLVMSharp;

namespace Ccash.CodeGeneration.LLVMGenerator
{
    public class StatementGenerator
    {
        private uint _ifCounter;

        private uint _whileCounter;

        private uint _doCounter;

        private uint _repeatCounter;

        private uint _forCounter;

        private IRBuilder Builder { get; }

        private LLVMValueRef Printf { get; }

        public StatementGenerator(IRBuilder builder, LLVMValueRef printf)
        {
            Builder = builder;
            Printf = printf;
        }

        public void ResetCounters()
        {
            _ifCounter = 0;
            _whileCounter = 0;
            _doCounter = 0;
            _forCounter = 0;
            _repeatCounter = 0;
        }

        public void Generate(IStatement statement, AbstractScope scope)
        {
            switch (statement)
            {
                case MethodCallStatement methodCallStatement:
                    GenerateMethodCall(methodCallStatement, scope);
                    break;

                case FunctionCallStatement functionCallStatement:
                    GenerateFunctionCall(functionCallStatement, scope);
                    break;

                case ReturnStatement returnStatement:
                    GenerateReturn(returnStatement, scope);
                    break;

                case IfStatement ifStatement:
                    GenerateIf(ifStatement);
                    break;

                case WhileStatement whileStatement:
                    GenerateWhile(whileStatement);
                    break;
                case DoStatement doStatement:
                    GenerateDo(doStatement);
                    break;
                case RepeatStatement repeatStatement:
                    GenerateRepeat(repeatStatement);
                    break;
                case ForLoopStatement forLoopStatement:
                    GenerateFor(forLoopStatement);
                    break;

                case VariableDeclaration variableStatement:
                    GenerateVariableDeclaration(variableStatement, scope);
                    break;

                case BlockStatement blockStatement:
                    blockStatement.Statements.ForEach(s => Generate(s, blockStatement));
                    break;

                default:
                    throw new NotImplementedException($"{statement.GetType()} is not yet supported");
            }
        }

        private void GenerateMethodCall(MethodCallStatement methodCallStatement, AbstractScope scope)
        {
            var args = methodCallStatement.FunctionCall.Arguments
                                          .Select(a => new Arg(Builder.Expression(a, scope), a.Type))
                                          .ToArray();

            GenerateFunctionCall(methodCallStatement.StructType.ModuleScope, args,
                                 methodCallStatement.FunctionCall.FullFunctionName);
        }

        private void GenerateFunctionCall(FunctionCallStatement functionCallStatement, AbstractScope scope)
        {
            var args = functionCallStatement.Arguments
                                            .Select(a => new Arg(Builder.Expression(a, scope), a.Type))
                                            .ToArray();

            var functionName = functionCallStatement.FullFunctionName;
            if (functionCallStatement.IsIntrinsic())
            {
                Builder.IntrinsicAssignmentCall(functionName, args);
            }
            else
            {
                GenerateFunctionCall(scope, args, functionName);
            }
        }

        private void GenerateFunctionCall(AbstractScope scope, Arg[] args, string functionName)
        {
            if (functionName == "printf")
            {
                for (var i = 0; i < args.Length; i++)
                {
                    if (args[i].Type is ReferenceType refType && refType.ReferredType == CcashType.ConstString)
                    {
                        LLVMValueRef stringPointer = Builder.Load(Builder.StructField(args[i].Value, 1));
                        args[i] = new Arg(stringPointer, args[i].Type);
                    }
                }

                Builder.CallFunction(Printf, args);
            }
            else
            {
                Builder.CallFunction(scope.GetCodeGeneratorData<LLVMValueRef>(functionName), args);
            }
        }

        private void GenerateReturn(ReturnStatement returnStatement, AbstractScope scope)
        {
            if (returnStatement.Expression == null)
            {
                Builder.ReturnVoid();
            }
            else
            {
                LLVMValueRef expression = Builder.Expression(returnStatement.Expression, scope);
                Builder.Return(expression);
            }
        }

        private void GenerateVariableDeclaration(VariableDeclaration variableDeclaration, AbstractScope scope)
        {
            LLVMTypeRef type = TypeResolver.Resolve(variableDeclaration.Type);
            LLVMValueRef initExpression = Builder.Expression(variableDeclaration.Expression, scope);
            if (variableDeclaration.Type is ReferenceType)
            {
                scope[variableDeclaration.Name].CodeGeneratorAttribute.Data = initExpression;
            }
            else
            {
                LLVMValueRef variable = Builder.AllocateVariable(type, variableDeclaration.Name);
                Builder.Store(initExpression, variable);
                scope[variableDeclaration.Name].CodeGeneratorAttribute.Data = variable;
            }
        }

        private void GenerateWhile(WhileStatement whileStatement)
        {
            var id = _whileCounter++;

            var conditionExpressionBlock = Builder.CurrentBlock.AppendBlock($"while{id}Condition");
            var loopBodyBlock = conditionExpressionBlock.AppendBlock($"while{id}Body");
            var nextBlock = loopBodyBlock.AppendBlock($"while{id}Next");

            whileStatement.ConditionBlock.Data = conditionExpressionBlock;
            whileStatement.NextBlock.Data = nextBlock;

            Builder.Branch(conditionExpressionBlock);

            Builder.PositionAtEnd(conditionExpressionBlock);
            LLVMValueRef condition = Builder.Expression(whileStatement.ConditionExpression, whileStatement.Parent);
            Builder.ConditionalBranch(condition, loopBodyBlock, nextBlock);

            Builder.PositionAtEnd(loopBodyBlock);
            whileStatement.Statements.ForEach(s => Generate(s, whileStatement));
            if (!Builder.CurrentBlock.HasTerminator())
            {
                Builder.Branch(conditionExpressionBlock);
            }

            Builder.PositionAtEnd(nextBlock);
        }
        
        private void GenerateDo(DoStatement doStatement)
        {
            var id = _doCounter++;

            var conditionExpressionBlock = Builder.CurrentBlock.AppendBlock($"do{id}Condition");
            var loopBodyBlock = conditionExpressionBlock.AppendBlock($"do{id}Body");
            var nextBlock = loopBodyBlock.AppendBlock($"do{id}Next");

            doStatement.ConditionBlock.Data = conditionExpressionBlock;
            doStatement.NextBlock.Data = nextBlock;

            Builder.Branch(loopBodyBlock);

            Builder.PositionAtEnd(conditionExpressionBlock);
            LLVMValueRef condition = Builder.Expression(doStatement.ConditionExpression, doStatement.Parent);
            Builder.ConditionalBranch(condition, loopBodyBlock, nextBlock);

            Builder.PositionAtEnd(loopBodyBlock);
            doStatement.Statements.ForEach(s => Generate(s, doStatement));
            if (!Builder.CurrentBlock.HasTerminator())
            {
                Builder.Branch(conditionExpressionBlock);
            }

            Builder.PositionAtEnd(nextBlock);
        }

        private void GenerateFor(ForLoopStatement forLoop)
        {
            var id = _forCounter++;

            var conditionExpressionBlock = Builder.CurrentBlock.AppendBlock($"for{id}Condition");
            var loopBodyBlock = conditionExpressionBlock.AppendBlock($"for{id}Body");
            var nextBlock = loopBodyBlock.AppendBlock($"for{id}Next");

            forLoop.ConditionBlock.Data = conditionExpressionBlock;
            forLoop.NextBlock.Data = nextBlock;

            if (forLoop.Declaration != null)
            {
                Generate(forLoop.Declaration, forLoop);
            }

            Builder.Branch(conditionExpressionBlock);

            Builder.PositionAtEnd(conditionExpressionBlock);
            LLVMValueRef cond = Builder.Expression(forLoop.ConditionExpression, forLoop);
            Builder.ConditionalBranch(cond, loopBodyBlock, nextBlock);

            Builder.PositionAtEnd(loopBodyBlock);
            forLoop.Statements.ForEach(s => Generate(s, forLoop));
            if (forLoop.Assignment != null)
            {
                Generate(forLoop.Assignment, forLoop);
            }

            if (!Builder.CurrentBlock.HasTerminator())
            {
                Builder.Branch(conditionExpressionBlock);
            }

            Builder.PositionAtEnd(nextBlock);
        }
        
        private void GenerateRepeat(RepeatStatement repeatLoop)
        {
            var id = _repeatCounter++;
            LLVMTypeRef type = TypeResolver.Resolve(CcashType.Uint64);
            LLVMValueRef initExpression = Builder.Expression(repeatLoop.ConditionExpression, repeatLoop);
            var expressionBlock = Builder.CurrentBlock.AppendBlock($"repeat{id}Condition");
            var loopBodyBlock = expressionBlock.AppendBlock($"repeat{id}Body");
            var nextBlock = loopBodyBlock.AppendBlock($"repeat{id}Next");

            var one = LLVM.ConstInt(TypeResolver.Resolve(CcashType.Uint64), 1, CcashType.Uint64.IsSigned);
            var zero = LLVM.ConstInt(TypeResolver.Resolve(CcashType.Uint64), 0, CcashType.Uint64.IsSigned);

            repeatLoop.ConditionBlock.Data = expressionBlock;
            repeatLoop.NextBlock.Data = nextBlock;

            LLVMValueRef valRef1 = Builder.AllocateVariable(type, "tmp");
            Builder.Store(initExpression, valRef1);

            Builder.Branch(expressionBlock);
            Builder.PositionAtEnd(expressionBlock);
            LLVMValueRef cond = Builder.UnsignedGT(Builder.Load(valRef1), zero);
            Builder.ConditionalBranch(cond, loopBodyBlock, nextBlock);

            Builder.PositionAtEnd(loopBodyBlock);

            LLVMValueRef oldValue = Builder.Load(valRef1);
            oldValue.SetValueName("__rvalue__");
            LLVMValueRef newValue = Builder.Sub(oldValue, one);
            Builder.Store(newValue, valRef1);

            repeatLoop.Statements.ForEach(s => Generate(s, repeatLoop));
            if (!Builder.CurrentBlock.HasTerminator())
            {
                Builder.Branch(expressionBlock);
            }

            Builder.PositionAtEnd(nextBlock);

        }

        private void GenerateIf(IfStatement ifStatement)
        {
            var originalBlock = Builder.CurrentBlock;
            var ifId = $"if{_ifCounter++}";
            var thenBlock = Builder.CurrentBlock.AppendBlock($"{ifId}Then");
            var nextBlock = thenBlock.AppendBlock($"{ifId}Next");

            var lastElseIfBlock = nextBlock;
            if (ifStatement.ElseStatement != null)
            {
                var elseBlock = nextBlock.PrependBlock($"{ifId}Else");
                Builder.PositionAtEnd(elseBlock);
                GenerateElse(ifStatement.ElseStatement, nextBlock);
                lastElseIfBlock = elseBlock;
            }

            GenerateElseIfs(ifStatement.ElseIfStatements, ifId, lastElseIfBlock);

            Builder.PositionAtEnd(thenBlock);
            ifStatement.Statements.ForEach(s => Generate(s, ifStatement));
            if (!Builder.CurrentBlock.HasTerminator())
            {
                Builder.Branch(nextBlock);
            }

            Builder.PositionAtEnd(originalBlock);
            LLVMValueRef condition = Builder.Expression(ifStatement.Expression, ifStatement.Parent);
            Builder.ConditionalBranch(condition, thenBlock, lastElseIfBlock);

            if (ifStatement.AlwaysReturns)
            {
                // If we always return then there is never a branch to the next block
                nextBlock.DeleteBasicBlock();
            }
            else
            {
                Builder.PositionAtEnd(nextBlock);
            }
        }

        private void GenerateElse(ElseStatement elseStatement, LLVMBasicBlockRef nextBlock)
        {
            elseStatement.Statements.ForEach(s => Generate(s, elseStatement));
            if (!Builder.CurrentBlock.HasTerminator())
            {
                Builder.Branch(nextBlock);
            }
        }

        private void GenerateElseIfs(List<ElseIfStatement> elseIfStatements, string ifId, LLVMBasicBlockRef nextBlock)
        {
            var nextElseIfBlock = nextBlock;
            for (var i = elseIfStatements.Count - 1; i >= 0; i--)
            {
                var elseIfId = $"{ifId}ElseIf{i}";
                var elseIfBlock = nextElseIfBlock.PrependBlock($"{elseIfId}");
                var elseIfThenBlock = elseIfBlock.AppendBlock($"{elseIfId}Then");

                var elseIfStatement = elseIfStatements[i];

                Builder.PositionAtEnd(elseIfBlock);
                LLVMValueRef elseIfCondition = Builder.Expression(elseIfStatement.Expression, elseIfStatement.Parent);
                Builder.ConditionalBranch(elseIfCondition, elseIfThenBlock, nextElseIfBlock);

                Builder.PositionAtEnd(elseIfThenBlock);
                elseIfStatement.Statements.ForEach(s => Generate(s, elseIfStatement));
                if (!Builder.CurrentBlock.HasTerminator())
                {
                    Builder.Branch(nextBlock);
                }

                nextElseIfBlock = elseIfBlock;
            }
        }

    }
}

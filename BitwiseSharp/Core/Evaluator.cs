using BitwiseSharp.Types;
using BitwiseSharp.Enums;

using static BitwiseSharp.Constants.Colors;

namespace BitwiseSharp.Core
{
    internal class Evaluator
    {
        private readonly bool _verbose;
        private readonly EnvironmentContext _environmentContext;

        internal Evaluator(bool verbose, EnvironmentContext environmentContext) 
        {
            _verbose = verbose;
            _environmentContext = environmentContext;
        }

        internal Result<ArbitraryNumber> Evaluate(Node node)
        {
            LogVerbose($"Evaluating Node: {node.GetType().Name}", LIGHT_BLUE);
            _indentLevel++;

            try
            {
                switch (node)
                {
                    case VariableDefinitionNode varDefNode:
                        LogVerbose($"Defining variable '{varDefNode.VariableName}'", GREEN);
                        var initialResult = Evaluate(varDefNode.RightHandSide);
                        if (!initialResult.IsSuccess) return initialResult;

                        ArbitraryNumber initialValue = initialResult.Value;
                        if (!_environmentContext.TryCreateVariable(varDefNode.VariableName, initialValue))
                        {
                            return Result<ArbitraryNumber>.Failure($"The variable '{varDefNode.VariableName}' is already defined in this scope.");
                        }

                        LogVerbose($"Variable '{varDefNode.VariableName}' defined with value: {initialValue}", YELLOW);
                        return Result<ArbitraryNumber>.Success(initialValue);

                    case VariableReferenceNode varRefNode:
                        LogVerbose($"Referencing variable '{varRefNode.VariableName}'", GREEN);
                        ArbitraryNumber variableValue;
                        if (!_environmentContext.TryGetVariable(varRefNode.VariableName, out variableValue))
                        {
                            return Result<ArbitraryNumber>.Failure($"The variable '{varRefNode.VariableName}' is not defined in this scope.");
                        }

                        LogVerbose($"Variable '{varRefNode.VariableName}' has value: {variableValue}", YELLOW);
                        return Result<ArbitraryNumber>.Success(variableValue);

                    case VariableAssignmentNode varAssignmentNode:
                        LogVerbose($"Assigning to variable '{varAssignmentNode.VariableName}'", GREEN);
                        var newValueResult = Evaluate(varAssignmentNode.RightHandSide);
                        if (!newValueResult.IsSuccess) return newValueResult;

                        ArbitraryNumber newValue = newValueResult.Value;
                        if (!_environmentContext.HasVariable(varAssignmentNode.VariableName))
                        {
                            return Result<ArbitraryNumber>.Failure($"The variable '{varAssignmentNode.VariableName}' is not defined in this scope.");
                        }

                        _environmentContext.SetVariable(varAssignmentNode.VariableName, newValue);
                        LogVerbose($"Variable '{varAssignmentNode.VariableName}' updated with value: {newValue}", YELLOW);
                        return Result<ArbitraryNumber>.Success(newValue);

                    case NumberNode numberNode:
                        LogVerbose($"Evaluating number: {numberNode.Value}", CYAN);
                        return Result<ArbitraryNumber>.Success(numberNode.Value);

                    case UnaryNode unaryNode:
                        LogVerbose($"Evaluating unary operation '{unaryNode.Operator}'", GREEN);
                        var operandResult = Evaluate(unaryNode.Operand);
                        if (!operandResult.IsSuccess) return operandResult;

                        ArbitraryNumber operand = operandResult.Value;
                        ArbitraryNumber unaryResult = unaryNode.Operator switch
                        {
                            TokenType.BitwiseNot => ~operand,
                            _ => throw new InvalidOperationException($"Invalid unary operator type: {unaryNode.Operator}")
                        };

                        LogVerbose($"Unary operation '{unaryNode.Operator}' result: {unaryResult}", YELLOW);
                        return Result<ArbitraryNumber>.Success(unaryResult);

                    case BinaryNode binaryNode:
                        LogVerbose($"Evaluating binary operation '{binaryNode.Operator}'", GREEN);
                        var leftResult = Evaluate(binaryNode.Left);
                        if (!leftResult.IsSuccess) return leftResult;
                        ArbitraryNumber left = leftResult.Value;

                        var rightResult = Evaluate(binaryNode.Right);
                        if (!rightResult.IsSuccess) return rightResult;
                        ArbitraryNumber right = rightResult.Value;

                        ArbitraryNumber binaryResult = binaryNode.Operator switch
                        {
                            TokenType.BitwiseAnd => left & right,
                            TokenType.BitwiseOr => left | right,
                            TokenType.BitwiseXor => left ^ right,
                            TokenType.LeftShift => left << (int)right,
                            TokenType.RightShift => left >> (int)right,
                            _ => throw new InvalidOperationException($"Invalid binary operator type: {binaryNode.Operator}")
                        };

                        LogVerbose($"Binary operation '{binaryNode.Operator}' result: {binaryResult}", YELLOW);
                        return Result<ArbitraryNumber>.Success(binaryResult);

                    default:
                        return Result<ArbitraryNumber>.Failure("Unknown error evaluating node.");
                }
            }
            finally
            {
                _indentLevel--;
                LogVerbose($"Exiting Node: {node.GetType().Name}", LIGHT_BLUE);
            }
        }

        private int _indentLevel = 0;

        private void LogVerbose(string message, string color)
        {
            if (_verbose)
            {
                Console.WriteLine($"{color}{new string(' ', _indentLevel * 2)}{message}{ANSI_RESET}");
                Console.ResetColor();
            }
        }

    }
}
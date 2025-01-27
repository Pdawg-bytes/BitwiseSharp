using BitwiseSharp.Types;
using BitwiseSharp.Enums;

using static BitwiseSharp.Constants.Colors;

namespace BitwiseSharp.Core
{
    internal class Evaluator
    {
        private readonly EnvironmentContext _environmentContext;

        internal Evaluator(EnvironmentContext environmentContext) 
        {
            _environmentContext = environmentContext;
        }

        internal Result<ArbitraryNumber> Evaluate(Node node)
        {
            switch (node)
            {
                case VariableDefinitionNode varDefNode:
                    var initialResult = Evaluate(varDefNode.RightHandSide);
                    if (!initialResult.IsSuccess) return initialResult;

                    ArbitraryNumber initialValue = initialResult.Value;
                    if (!_environmentContext.TryCreateVariable(varDefNode.VariableName, initialValue))
                        return Result<ArbitraryNumber>.Failure($"The variable '{varDefNode.VariableName}' is already defined in this scope.");

                    return Result<ArbitraryNumber>.Success(initialValue);

                case VariableReferenceNode varRefNode:
                    ArbitraryNumber variableValue;
                    if (!_environmentContext.TryGetVariable(varRefNode.VariableName, out variableValue))
                        return Result<ArbitraryNumber>.Failure($"The variable '{varRefNode.VariableName}' is not defined in this scope.");

                    return Result<ArbitraryNumber>.Success(variableValue);

                case VariableAssignmentNode varAssignmentNode:
                    var newValueResult = Evaluate(varAssignmentNode.RightHandSide);
                    if (!newValueResult.IsSuccess) return newValueResult;

                    ArbitraryNumber newValue = newValueResult.Value;
                    if (!_environmentContext.HasVariable(varAssignmentNode.VariableName))
                        return Result<ArbitraryNumber>.Failure($"The variable '{varAssignmentNode.VariableName}' is not defined in this scope.");

                    _environmentContext.SetVariable(varAssignmentNode.VariableName, newValue);
                    return Result<ArbitraryNumber>.Success(newValue);

                case NumberNode numberNode:
                    return Result<ArbitraryNumber>.Success(numberNode.Value);

                case UnaryNode unaryNode:
                    var operandResult = Evaluate(unaryNode.Operand);
                    if (!operandResult.IsSuccess) return operandResult;

                    ArbitraryNumber operand = operandResult.Value;
                    return unaryNode.Operator switch
                    {
                        TokenType.BitwiseNot => Result<ArbitraryNumber>.Success(~operand),
                        TokenType.Minus => Result<ArbitraryNumber>.Success(-operand),
                        _ => Result<ArbitraryNumber>.Failure($"Invalid unary operator type: {unaryNode.Operator}")
                    };

                case BinaryNode binaryNode:
                    var leftResult = Evaluate(binaryNode.Left);
                    if (!leftResult.IsSuccess) return leftResult;
                    ArbitraryNumber left = leftResult.Value;

                    var rightResult = Evaluate(binaryNode.Right);
                    if (!rightResult.IsSuccess) return rightResult;
                    ArbitraryNumber right = rightResult.Value;

                    return binaryNode.Operator switch
                    {
                        TokenType.BitwiseAnd => Result<ArbitraryNumber>.Success(left & right),
                        TokenType.BitwiseOr => Result<ArbitraryNumber>.Success(left | right),
                        TokenType.BitwiseXor => Result<ArbitraryNumber>.Success(left ^ right),
                        TokenType.LeftShift => Result<ArbitraryNumber>.Success(left << (int)right),
                        TokenType.RightShift => Result<ArbitraryNumber>.Success(left >> (int)right),
                        TokenType.Plus => Result<ArbitraryNumber>.Success(left + right),
                        TokenType.Minus => Result<ArbitraryNumber>.Success(left - right),
                        TokenType.Multiply => Result<ArbitraryNumber>.Success(left * right),
                        TokenType.Divide => Result<ArbitraryNumber>.Success(left / right),
                        _ => Result<ArbitraryNumber>.Failure($"Invalid binary operator type: {binaryNode.Operator}")
                    };
            }

            return Result<ArbitraryNumber>.Failure("Unknown error evaluating node.");
        }
    }
}
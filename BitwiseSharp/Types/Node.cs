using BitwiseSharp.Enums;

namespace BitwiseSharp.Types
{
    internal abstract record Node(ExpressionType Type);

    internal record NumberNode(ArbitraryNumber Value) : Node(ExpressionType.Number);

    internal record UnaryNode(TokenType Operator, Node Operand) : Node(ExpressionType.UnaryOperator);

    internal record BinaryNode(TokenType Operator, Node Left, Node Right) : Node(ExpressionType.BinaryOperator);

    internal record VariableDefinitionNode(string VariableName, Node RightHandSide) : Node(ExpressionType.VariableDefinition);

    internal record VariableAssignmentNode(string VariableName, Node RightHandSide) : Node(ExpressionType.VariableAssignment);

    internal record VariableReferenceNode(string VariableName) : Node(ExpressionType.IdentifierReference);
}
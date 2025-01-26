using BitwiseSharp.Enums;

namespace BitwiseSharp.Types
{
    internal abstract record Node(ExpressionType Type);

    internal record NumberNode(ArbitraryNumber Value) : Node(ExpressionType.Number);

    internal record UnaryNode(string OperatorName, TokenType Operator, Node Operand) : Node(ExpressionType.UnaryOperator);

    internal record BinaryNode(string OperatorName, TokenType Operator, Node Left, Node Right) : Node(ExpressionType.BinaryOperator);
}
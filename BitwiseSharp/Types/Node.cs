using BitwiseSharp.Enums;

namespace BitwiseSharp.Types
{
    /// <summary>Represents the base record for all expression nodes in the AST.</summary>
    /// <param name="Type">The type of expression the node represents.</param>
    internal abstract record Node(ExpressionType Type);

    /// <summary>Represents a number node in the AST.</summary>
    /// <param name="Value">The numeric value of the node.</param>
    internal record NumberNode(ArbitraryNumber Value) : Node(ExpressionType.Number);

    /// <summary>Represents a unary operation node in the AST.</summary>
    /// <param name="Operator">The unary operator that is applied to the <paramref name="Operand"/>.</param>
    /// <param name="Operand">The operand of the unary expression.</param>
    internal record UnaryNode(TokenType Operator, Node Operand) : Node(ExpressionType.UnaryOperator);

    /// <summary>Represents a binary operation node in the AST.</summary>
    /// <param name="Operator">The binary operator that is applied between the <paramref name="Left"/> and <paramref name="Right"/> expressions.</param>
    /// <param name="Left">The left-hand operand of the binary expression.</param>
    /// <param name="Right">The right-hand operand of the binary expression.</param>
    internal record BinaryNode(TokenType Operator, Node Left, Node Right) : Node(ExpressionType.BinaryOperator);

    /// <summary>Represents a variable definition node in the AST.</summary>
    /// <param name="VariableName">The name of the variable being defined.</param>
    /// <param name="RightHandSide">The right-hand side expression for the variable definition.</param>
    internal record VariableDefinitionNode(string VariableName, Node RightHandSide) : Node(ExpressionType.VariableDefinition);

    /// <summary>Represents a variable assignment node in the AST.</summary>
    /// <param name="VariableName">The name of the variable being assigned to.</param>
    /// <param name="RightHandSide">The right-hand side expression for the variable definition.</param>
    internal record VariableAssignmentNode(string VariableName, Node RightHandSide) : Node(ExpressionType.VariableAssignment);

    /// <summary>Represents a variable reference node in the AST.</summary>
    /// <param name="VariableName">The name of the referenced variable.</param>
    internal record VariableReferenceNode(string VariableName) : Node(ExpressionType.IdentifierReference);
}
using BitwiseSharp.Types;

namespace BitwiseSharp.Enums
{
    /// <summary>
    /// The type of expression represented by an AST <see cref="Node"/>.
    /// </summary>
    internal enum ExpressionType
    {
        Number,
        UnaryOperator,
        BinaryOperator,
        VariableDefinition,
        VariableAssignment,
        IdentifierReference
    }
}
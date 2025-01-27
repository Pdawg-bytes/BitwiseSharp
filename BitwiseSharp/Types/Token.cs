using BitwiseSharp.Enums;

namespace BitwiseSharp.Types
{
    /// <summary>
    /// Represents a single lexical token of an expression.
    /// </summary>
    internal record Token
    {
        internal TokenType Type;
        internal ArbitraryNumber NumberValue;
        internal string? VariableName;

        /// <summary>
        /// Initializes a new instance of the <see cref="Token"/> record.
        /// </summary>
        /// <param name="type">The <see cref="TokenType"/> of the token.</param>
        /// <param name="value">The numeric value of the token if applicable.</param>
        /// <param name="identifier">The identifier stored in the token if applicable.</param>
        internal Token(TokenType type, ArbitraryNumber value = default, string? identifier = null)
        {
            Type = type;

            NumberValue = type == TokenType.Number ? value : default;
            VariableName = type == TokenType.Identifier || type == TokenType.Let ? identifier : null;
        }

        public override string ToString()
        {
            return Type switch
            {
                TokenType.Identifier => $"Type: {Type}, Variable: {VariableName}",
                TokenType.Number => $"Type: {Type}, Value: {NumberValue}",
                _ => $"Type: {Type}"
            };
        }
    }
}
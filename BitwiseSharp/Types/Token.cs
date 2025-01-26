using BitwiseSharp.Enums;

namespace BitwiseSharp.Types
{
    internal record Token
    {
        internal TokenType Type;
        internal ArbitraryNumber NumberValue;
        internal string? VariableName;

        internal Token(TokenType type, ArbitraryNumber value = default, string? variableName = null)
        {
            Type = type;

            NumberValue = type == TokenType.Number ? value : default;
            VariableName = type == TokenType.Identifier || type == TokenType.Let ? variableName : null;
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
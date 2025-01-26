using BitwiseSharp.Enums;

namespace BitwiseSharp.Types
{
    internal record Token
    {
        internal TokenType Type;
        internal ArbitraryNumber TokenValue;

        internal Token(TokenType type, ArbitraryNumber tokenValue = default)
        {
            Type = type;
            TokenValue = tokenValue;
        }
    }
}
using BitwiseSharp.Enums;

namespace BitwiseSharp.Core
{
    internal static class Precedence
    {
        internal static readonly Dictionary<TokenType, (int Precedence, bool RightAssociative)> OperatorPrecedence = new()
        {
            { TokenType.BitwiseOr, (1, false) },
            { TokenType.BitwiseXor, (2, false) },
            { TokenType.BitwiseAnd, (3, false) },
            { TokenType.LeftShift, (4, false) },
            { TokenType.RightShift, (4, false) },
            { TokenType.BitwiseNot, (5, true) }
        };
    }
}
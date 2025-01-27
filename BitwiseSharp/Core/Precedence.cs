using BitwiseSharp.Enums;

namespace BitwiseSharp.Core
{
    internal static class Precedence
    {
        /// <summary>
        /// Defines the precedence and associativity for each operator type.
        /// </summary>
        internal static readonly Dictionary<TokenType, (int Precedence, bool RightAssociative)> OperatorPrecedence = new()
        {
            { TokenType.BitwiseOr, (1, false) },
            { TokenType.BitwiseXor, (2, false) },
            { TokenType.BitwiseAnd, (3, false) },
            { TokenType.LeftShift, (4, false) },
            { TokenType.RightShift, (4, false) },
            { TokenType.Plus, (5, false) },
            { TokenType.Minus, (5, false) },
            { TokenType.Multiply, (6, false) },
            { TokenType.Divide, (6, false) },
            { TokenType.BitwiseNot, (7, true) }
        };
    }
}
using BitwiseSharp.Enums;

namespace BitwiseSharp.Core
{
    internal static class Precedence
    {
        internal static int GetPrecedence(TokenType tokenType)
        {
            switch (tokenType)
            {
                case TokenType.LeftParenthesis:
                    return int.MaxValue;
                case TokenType.RightParenthesis:
                    return -50;

                case TokenType.BitwiseNot:
                    return 4;
                case TokenType.LeftShift:
                case TokenType.RightShift:
                    return 3;
                case TokenType.BitwiseAnd:
                    return 2;
                case TokenType.BitwiseXor:
                    return 1;
                case TokenType.BitwiseOr:
                    return 0;
                default:
                    return -99;
            }
        }
    }
}
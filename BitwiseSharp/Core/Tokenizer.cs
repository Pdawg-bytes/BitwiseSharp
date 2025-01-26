using BitwiseSharp.Enums;
using BitwiseSharp.Types;
using System.Globalization;
using System.Text.RegularExpressions;

using static BitwiseSharp.Constants.Colors;

namespace BitwiseSharp.Core
{
    internal static class Tokenizer
    {
        private static Regex _inputPattern = new(@"\blet\b|[a-zA-Z_][a-zA-Z0-9_]*|=|~|\(|\)|-?0(x|X)[0-9a-fA-F]+|-?0(b|B)[01]+|-?\d+|<<|>>|&|\^|\|", RegexOptions.Compiled);

        internal static bool Verbose;

        internal static Result<List<Token>> Tokenize(string expression)
        {
            MatchCollection matches = _inputPattern.Matches(expression);
            if (matches.Count == 0)
            {
                return Result<List<Token>>.Failure($"The expression \"{expression}\" was not able to be tokenized.");
            }

            List<Token> tokens = new();
            int currentIndex = 0;

            foreach (Match match in matches)
            {
                string value = match.Value;
                while (currentIndex < match.Index)
                {
                    char unrecognizedChar = expression[currentIndex];
                    if (!char.IsWhiteSpace(unrecognizedChar))
                    {
                        return Result<List<Token>>.Failure($"Unrecognized character '{unrecognizedChar}' in the expression.");
                    }
                    currentIndex++;
                }

                TokenType tokenType = value switch
                {
                    "let" => TokenType.Let,
                    "=" => TokenType.Assignment,
                    "(" => TokenType.LeftParenthesis,
                    ")" => TokenType.RightParenthesis,
                    "&" => TokenType.BitwiseAnd,
                    "|" => TokenType.BitwiseOr,
                    "^" => TokenType.BitwiseXor,
                    "~" => TokenType.BitwiseNot,
                    "<<" => TokenType.LeftShift,
                    ">>" => TokenType.RightShift,
                    _ => Regex.IsMatch(value, @"^[a-zA-Z_][a-zA-Z_]*$") ? TokenType.Identifier : TokenType.Unknown
                };

                if (tokenType == TokenType.Unknown && value.Any(char.IsDigit)) tokens.Add(new Token(TokenType.Number, ParseNumber(value)));
                else tokens.Add(new Token(tokenType, variableName: tokenType == TokenType.Identifier ? value : null));

                currentIndex = match.Index + match.Length;
            }

            while (currentIndex < expression.Length)
            {
                char unrecognizedChar = expression[currentIndex];
                if (!char.IsWhiteSpace(unrecognizedChar))
                {
                    return Result<List<Token>>.Failure($"Unrecognized character '{unrecognizedChar}' in the expression.");
                }
                currentIndex++;
            }
            return Result<List<Token>>.Success(tokens);
        }


        private static ArbitraryNumber ParseNumber(string input)
        {
            bool isNegative = input.StartsWith("-");
            if (isNegative)
                input = input.Substring(1);

            ArbitraryNumber result = input switch
            {
                string s when s.StartsWith("0x", StringComparison.OrdinalIgnoreCase) =>
                    ParseHex(s.Substring(2)),
                string s when s.StartsWith("0b", StringComparison.OrdinalIgnoreCase) =>
                    ParseBinary(s.Substring(2)),
                _ => ArbitraryNumber.Parse(input, NumberStyles.Integer)
            };

            return isNegative ? -result : result;
        }

        private static ArbitraryNumber ParseBinary(string binaryInput) => binaryInput.Aggregate<Char, ArbitraryNumber>(0, (result, bit) => (result << 1) | (bit == '1' ? 1 : 0));

        private static ArbitraryNumber ParseHex(string hexInput) => hexInput.Aggregate<Char, ArbitraryNumber>(0, (result, hexChar) => (result << 4) | HexCharToValue(hexChar));

        private static int HexCharToValue(char hexChar) =>
            (hexChar >= '0' && hexChar <= '9') ? hexChar - '0' :
            (hexChar >= 'A' && hexChar <= 'F') ? hexChar - 'A' + 10 :
            (hexChar >= 'a' && hexChar <= 'f') ? hexChar - 'a' + 10 :
            throw new FormatException($"Invalid hexadecimal character: {hexChar}");
    }
}
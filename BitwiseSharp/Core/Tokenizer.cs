using BitwiseSharp.Enums;
using BitwiseSharp.Types;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BitwiseSharp.Core
{
    internal static class Tokenizer
    {
        private static Regex _inputPattern = new("~|\\(|\\)|-?0(x|X)[0-9a-fA-F]+|-?0(b|B)[01]+|-?\\d+|<<|>>|&|\\^|\\|", RegexOptions.Compiled);

        internal static bool Verbose;

        internal static List<Token> Tokenize(string expression)
        {
            MatchCollection matches = _inputPattern.Matches(expression);
            if (matches.Count == 0) throw new InvalidDataException($"The expression \"{expression}\" was not able to be tokenized.");

            List<Token> tokens = new();
            foreach (Match match in matches)
            {
                if (match.Value.Any(char.IsDigit))
                {
                    tokens.Add(new(TokenType.Number, ParseNumber(match.Value)));
                    continue;
                }

                tokens.Add(new(match.Value switch
                {
                    "(" => TokenType.LeftParenthesis,
                    ")" => TokenType.RightParenthesis,

                    "&" => TokenType.BitwiseAnd,
                    "|" => TokenType.BitwiseOr,
                    "^" => TokenType.BitwiseXor,
                    "~" => TokenType.BitwiseNot,
                    "<<" => TokenType.LeftShift,
                    ">>" => TokenType.RightShift,

                    _ => TokenType.Unknown
                }));
            }

            if (Verbose)
            {
                Console.WriteLine("Tokenized expression:");
                int typeWidth = tokens.Max(t => t.Type.ToString().Length);
                int valueWidth = tokens.Max(t => t.TokenValue.ToString().Length);

                for (int i = 0; i < tokens.Count; i++)
                {
                    Console.WriteLine($"Token {(i + 1).ToString("000")}: Type: {tokens[i].Type.ToString().PadRight(typeWidth)} Value: {tokens[i].TokenValue.ToString().PadLeft(valueWidth)}");
                }
                Console.WriteLine();
            }
            return tokens;
        }

        private static ArbitraryNumber ParseNumber(string input)
        {
            bool isNegative = input.StartsWith("-");
            if (isNegative)
                input = input.Substring(1);

            ArbitraryNumber result = input switch
            {
                var s when s.StartsWith("0x", StringComparison.OrdinalIgnoreCase) =>
                    ParseHex(s.Substring(2)),
                var s when s.StartsWith("0b", StringComparison.OrdinalIgnoreCase) =>
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
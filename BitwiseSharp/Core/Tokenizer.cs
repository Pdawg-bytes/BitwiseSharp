using BitwiseSharp.Enums;
using BitwiseSharp.Types;
using System.Globalization;
using System.Text.RegularExpressions;

using static BitwiseSharp.Constants.Colors;

namespace BitwiseSharp.Core
{
    /// <summary>
    /// Tokenizes string expressions into lists of <see cref="Token"/>s for further parsing.
    /// </summary>
    internal class Tokenizer
    {
        private static Regex _inputPattern = new(@"\blet\b|[a-zA-Z_][a-zA-Z0-9_]*|=|~|\(|\)|-?0(x|X)[0-9a-fA-F]+|-?0(b|B)[01]+|-?\d+|<<|>>|&|\^|\||\+|\-|\*|\/", RegexOptions.Compiled);

        private readonly VerboseLogContext _logCtx;

        /// <summary>Initializes an instance of the <see cref="Tokenizer"/> class.</summary>
        /// <param name="logCtx">The logging context of the tokenizer.</param>
        internal Tokenizer(VerboseLogContext logCtx)
        {
            _logCtx = logCtx;
        }

        /// <summary>
        /// Tokenizes the <paramref name="expression"/> into a series of parsable <see cref="Token"/>s.
        /// </summary>
        /// <param name="expression">The expression to tokenize.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a list of <see cref="Token"/> objects, or a failure result if the tokenization fails.
        /// </returns>
        internal Result<List<Token>> Tokenize(string expression)
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

                    "+" => TokenType.Plus,
                    "-" => TokenType.Minus,
                    "*" => TokenType.Multiply,
                    "/" => TokenType.Divide,

                    _ => Regex.IsMatch(value, @"^[a-zA-Z_][a-zA-Z_]*$") ? TokenType.Identifier : TokenType.Unknown
                };

                if (tokenType == TokenType.Unknown && value.Any(char.IsDigit)) tokens.Add(new Token(TokenType.Number, ParseNumber(value)));
                else tokens.Add(new Token(tokenType, identifier: tokenType == TokenType.Identifier ? value : null));

                currentIndex = match.Index + match.Length;
            }

            if (_logCtx.Verbose)
            {
                int typeWidth = tokens.Max(t => t.Type.ToString().Length);
                int valueWidth = tokens.Max(t => t.Type == TokenType.Identifier ? t.VariableName.Length : t.NumberValue.ToString().Length);

                _logCtx.NewLine(VerboseLogType.Tokenizer);

                for (int i = 0; i < tokens.Count; i++)
                {
                    string val = tokens[i].Type == TokenType.Identifier ? tokens[i].VariableName : tokens[i].NumberValue.ToString();

                    _logCtx.Log(VerboseLogType.Tokenizer,
                        $"{_logCtx.GetColor(WHITE)}Token {(i + 1).ToString("000")}:{ANSI_RESET} " +
                        $"{_logCtx.GetColorForTokenType(tokens[i].Type)}{tokens[i].Type.ToString().PadRight(typeWidth)}{ANSI_RESET} " +
                        $"Value: {val.PadLeft(valueWidth)}");
                }
            }

            return Result<List<Token>>.Success(tokens);
        }

        /// <summary>
        /// Parses a <see cref="string"/> into a <see cref="ArbitraryNumber"/>.
        /// </summary>
        /// <param name="input">The string representation of the number, in hex, binary, or decimal.</param>
        /// <returns>The numerical value of the <paramref name="input"/>.</returns>
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
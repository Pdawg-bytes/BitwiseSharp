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
        private static Regex _inputPattern = new(@"
            \blet\b
            |[a-zA-Z_][a-zA-Z0-9_]*
            |=
            |~
            |\(
            |\)
            |0[xX][a-zA-Z0-9_]+
            |0[bB][a-zA-Z0-9_]+
            |\d+
            |<<
            |>>
            |&
            |\^
            |\|
            |\+
            |\-
            |\*
            |\/
            |\%",
        RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);


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
                    "%" => TokenType.Modulus,

                    _ => Regex.IsMatch(value, @"^[a-zA-Z_][a-zA-Z0-9_]*$") ? TokenType.Identifier : TokenType.Unknown
                };

                if (tokenType == TokenType.Unknown && value.Any(char.IsDigit))
                {
                    var result = ParseNumber(value);
                    if (result.IsSuccess)
                        tokens.Add(new Token(TokenType.Number, result.Value));
                    else
                        return Result<List<Token>>.Failure(result.Error);
                }
                else tokens.Add(new Token(tokenType, identifier: tokenType == TokenType.Identifier ? value : null));

                currentIndex = match.Index + match.Length;
            }

            if (_logCtx.Verbose)
            {
                int typeWidth = tokens.Max(t => t.Type.ToString().Length);
                int valueWidth = tokens.Max(t => t.Type == TokenType.Identifier ? t.VariableName.Length : t.NumberValue.ToString().Length);

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

        private static Result<ArbitraryNumber> ParseNumber(string input)
        {
            bool isNegative = input.StartsWith("-");
            if (isNegative)
                input = input.Substring(1);

            try
            {
                ArbitraryNumber result;

                if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    string hex = input.Substring(2);
                    if (!Regex.IsMatch(hex, @"^[0-9a-fA-F]+$"))
                        return Result<ArbitraryNumber>.Failure("Hex number must only contain digits 0-9 and letters A-F.");
                    result = ParseHex(hex);
                }
                else if (input.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
                {
                    string binary = input.Substring(2);
                    if (!Regex.IsMatch(binary, @"^[01]+$"))
                        return Result<ArbitraryNumber>.Failure("Binary number must only contain digits 0 and 1.");
                    result = ParseBinary(binary);
                }
                else
                {
                    result = ArbitraryNumber.Parse(input, NumberStyles.Integer);
                }

                return Result<ArbitraryNumber>.Success(isNegative ? -result : result);
            }
            catch (Exception ex)
            {
                return Result<ArbitraryNumber>.Failure($"Failed to parse number: {ex.Message}");
            }
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
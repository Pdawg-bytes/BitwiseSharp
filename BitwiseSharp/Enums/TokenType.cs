namespace BitwiseSharp.Enums
{
    /// <summary>
    /// Represents the various types of tokens that can be encountered during lexical analysis.
    /// </summary>
    internal enum TokenType
    {
        /// <summary>Token representing a numeric value.</summary>
        Number,

        /// <summary>Token representing a left parenthesis '('.</summary>
        LeftParenthesis,

        /// <summary>Token representing a right parenthesis ')'.</summary>
        RightParenthesis,

        /// <summary>Token representing the 'let' keyword for variable declaration.</summary>
        Let,

        /// <summary> Token representing an identifier (e.g., a variable name).</summary>
        Identifier,

        /// <summary>Token representing the assignment operator '='.</summary>
        Assignment,

        /// <summary>Token representing the bitwise AND operator '&'.</summary>
        BitwiseAnd,

        /// <summary>Token representing the bitwise OR operator '|'.</summary>
        BitwiseOr,

        /// <summary>Token representing the bitwise XOR operator '^'.</summary>
        BitwiseXor,

        /// <summary>Token representing the bitwise NOT operator '~'.</summary>
        BitwiseNot,

        /// <summary>Token representing the subtraction or arithmetic negation operator '-'.</summary>
        Minus,

        /// <summary>Token representing the addition operator '+'.</summary>
        Plus,

        /// <summary>Token representing the multiplication operator '*'.</summary>
        Multiply,

        /// <summary>Token representing the division operator '/'.</summary>
        Divide,

        /// <summary>Token representing the left shift operator '<<'.</summary>
        LeftShift,

        /// <summary> Token representing the right shift operator '>>'.</summary>
        RightShift,

        /// <summary>Token representing an unknown or unrecognized token.</summary>
        /// <remarks>This token should never be encountered.</remarks>
        Unknown
    }
}
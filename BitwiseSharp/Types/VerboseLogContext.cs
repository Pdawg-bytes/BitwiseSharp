using BitwiseSharp.Enums;
using System.Runtime.CompilerServices;

using static BitwiseSharp.Constants.Colors;

namespace BitwiseSharp.Types
{
    /// <summary>
    /// Defines the context for logging with support for color and verbosity controls.
    /// </summary>
    public class VerboseLogContext
    {
        public bool ColorsEnabled;
        public bool Verbose;

        /// <summary>
        /// Initializes a new instance of the <see cref="VerboseLogContext"/> class.
        /// </summary>
        /// <param name="colorsEnabled">Indicates whether color output is enabled.</param>
        /// <param name="enabled">Indicates whether verbose logging is enabled.</param>
        public VerboseLogContext(bool colorsEnabled, bool enabled)
        {
            ColorsEnabled = colorsEnabled;
            Verbose = enabled;
        }

        /// <summary>
        /// Event triggered when a log entry is generated.
        /// </summary>
        public event EventHandler<LogGeneratedEventArgs> LogGenerated;
        internal void Log(VerboseLogType logType, string logData) => LogGenerated?.Invoke(this, new LogGeneratedEventArgs(logType, logData));
        internal void NewLine(VerboseLogType logType) => LogGenerated?.Invoke(this, new LogGeneratedEventArgs(logType, "\n"));

        /// <summary>
        /// Gets the color for a specific color code if color output is enabled.
        /// </summary>
        /// <param name="color">The color code to retrieve the color for.</param>
        /// <returns>The color string if enabled, otherwise an empty string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal string GetColor(string color) => ColorsEnabled ? color : string.Empty;

        /// <summary>
        /// Gets the color for a specific token type.
        /// </summary>
        /// <param name="type">The token type to retrieve the color for.</param>
        /// <returns>The color string for the specified token type.</returns>
        internal string GetColorForTokenType(TokenType type) => type switch
        {
            TokenType.Let => GetColor(LIGHT_BLUE),
            TokenType.Identifier => GetColor(GREEN),
            TokenType.Assignment => GetColor(CYAN),
            TokenType.Number => GetColor(ORANGE),
            TokenType.LeftParenthesis => GetColor(LIGHT_RED),
            TokenType.RightParenthesis => GetColor(LIGHT_RED),
            TokenType.BitwiseNot => GetColor(YELLOW),
            TokenType.BitwiseAnd => GetColor(YELLOW),
            TokenType.BitwiseOr => GetColor(YELLOW),
            TokenType.BitwiseXor => GetColor(YELLOW),
            TokenType.LeftShift => GetColor(YELLOW),
            TokenType.RightShift => GetColor(YELLOW),
            TokenType.Plus => GetColor(YELLOW),
            TokenType.Minus => GetColor(YELLOW),
            TokenType.Multiply => GetColor(YELLOW),
            TokenType.Divide => GetColor(YELLOW),
            TokenType.Unknown => GetColor(RED),
            _ => GetColor(ANSI_RESET)
        };
    }

    public class LogGeneratedEventArgs : EventArgs
    {
        public LogGeneratedEventArgs(VerboseLogType logType, string logData)
        {
            LogSeverity = logType;
            LogData = logData;
        }

        /// <summary>
        /// The severity of the generated log.
        /// </summary>
        public VerboseLogType LogSeverity { get; set; }

        /// <summary>
        /// The message data of the generated log.
        /// </summary>
        public string LogData { get; set; }
    }
}

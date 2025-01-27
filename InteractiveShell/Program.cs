using BitwiseSharp;
using BitwiseSharp.Types;
using static BitwiseSharp.Constants.Colors;

namespace InteractiveShell
{
    internal class Program
    {
        static BitwiseEvalulator _eval;
        static string _numberFormat = "D";
        static int _padding = 0;

        static void Main(string[] args)
        {
            VerboseLogContext logCtx = new(false, false);
            logCtx.LogGenerated += LogCtx_LogGenerated;
            _eval = new(logCtx, new BitwiseSharp.Core.EnvironmentContext());
            RunShell();
        }

        private static void LogCtx_LogGenerated(object? sender, LogGeneratedEventArgs e) => Console.WriteLine(e.LogData);

        static void RunShell()
        {
            Console.Write($"{PURPLE}>>> {LIGHT_BLUE}");
            string input = Console.ReadLine();
            Console.Write(ANSI_RESET);

            if (input == "exit") return;
            if (input == "sb") { _numberFormat = "B"; RunShell(); }
            if (input == "sd") { _numberFormat = "D"; RunShell(); }
            if (input == "sh") { _numberFormat = "X"; RunShell(); }
            if (input == "sp")
            {
                Console.Write("Padding: ");
                if (!int.TryParse(Console.ReadLine(), out _padding))
                    Console.WriteLine("Expected number for padding value");

                RunShell();
            }

            var exp = _eval.EvaluateExpression(input);
            if (exp.IsSuccess) Console.WriteLine(exp.Value.ToString(_numberFormat).PadLeft(_padding, '0'));
            else Console.WriteLine($"[{RED}ERROR{ANSI_RESET}]: {LIGHT_RED}{exp.Error}{ANSI_RESET}");

            RunShell();
        }
    }
}
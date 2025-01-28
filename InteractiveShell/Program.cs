using BitwiseSharp;
using BitwiseSharp.Types;
using static BitwiseSharp.Constants.Colors;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InteractiveShell
{
    internal class Program
    {
        static BitwiseEvalulator _eval;
        static string _numberFormat = "D";
        static int _padding = 0;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnCancelKeyPress);

            VerboseLogContext logCtx = new(false, false);
            logCtx.LogGenerated += LogCtx_LogGenerated;
            _eval = new(logCtx, new BitwiseSharp.Core.EnvironmentContext());
            RunShell();
        }

        private static void LogCtx_LogGenerated(object? sender, LogGeneratedEventArgs e) => Console.WriteLine(e.LogData);

        static void RunShell()
        {
            Console.Write($"{PURPLE}>>> {LIGHT_PURPLE}");
            string input = Console.ReadLine();
            Console.Write(ANSI_RESET);

            if (input == null) { Console.WriteLine(); RunShell(); }
            if (input == "exit") return;
            if (input == "help") { PrintHelp(); RunShell(); }
            if (input == "sb") { _numberFormat = "B"; RunShell(); }
            if (input == "sd") { _numberFormat = "D"; RunShell(); }
            if (input == "sh") { _numberFormat = "X"; RunShell(); }

            if (input.StartsWith("sp"))
            {
                string paddingValue = input.Substring(2).Trim();
                if (!int.TryParse(paddingValue, out _padding))
                    Console.WriteLine($"{RED}[ERROR]:{ANSI_RESET} Expected number for padding value.");

                RunShell();
            }

            var exp = _eval.EvaluateExpression(input);
            if (exp.IsSuccess) Console.WriteLine(exp.Value.ToString(_numberFormat).PadLeft(_padding, '0'));
            else Console.WriteLine($"{RED}[ERROR]:{ANSI_RESET} {exp.Error}");

            RunShell();
        }

        static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
        }

        static void PrintHelp()
        {
            string[] helpMessage = [
                "Type a bitwise operation (e.g., 5 & 3) or an arithmetic operation (e.g., 10 + 2) and press Enter to see the result.",
                $"Define variables using: {LIGHT_PURPLE}let <name> = <expression>{ANSI_RESET} (e.g., {LIGHT_PURPLE}let x = 5 & 3{ANSI_RESET}).",
                "  ",
                "Supported bitwise operations: ~, &, ^, |, <<, >>.",
                "Supported integer operations: +, -, *, /, %.",
                "  ",
                "Commands for formatting outputs:",
                $"  {LIGHT_PURPLE}sb{ANSI_RESET} - Switch output to binary.",
                $"  {LIGHT_PURPLE}sh{ANSI_RESET} - Switch output to hexadecimal.",
                $"  {LIGHT_PURPLE}sd{ANSI_RESET} - Switch output to decimal.",
                $"  {LIGHT_PURPLE}sp <number>{ANSI_RESET} - Set padding (e.g., {LIGHT_PURPLE}sp 8{ANSI_RESET} for 8-character width).",
                "  ",
                $"To view this help message again, type {LIGHT_PURPLE}help{ANSI_RESET}.",
                $"This shell also supports basic features like history navigation (using the up and down arrows) and clearing with {LIGHT_PURPLE}Ctrl-C{ANSI_RESET}.",
            ];

            foreach (string line in helpMessage) Console.WriteLine(line);
        }
    }
}
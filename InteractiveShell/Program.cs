using BitwiseSharp;
using static BitwiseSharp.Constants.Colors;

namespace InteractiveShell
{
    internal class Program
    {
        static BitwiseEvalulator _parser;  

        static void Main(string[] args)
        {
            _parser = new(false, new BitwiseSharp.Core.EnvironmentContext());
            RunShell();
        }

        static void RunShell()
        {
            Console.Write($"{PURPLE}>>> {LIGHT_BLUE}");
            string input = Console.ReadLine();
            Console.Write(ANSI_RESET);

            if (input == "exit") return;

            var exp = _parser.EvaluateExpression(input);
            if (exp.IsSuccess) Console.WriteLine(exp.Value);
            else Console.WriteLine(RED + exp.Error + ANSI_RESET);

            RunShell();
        }
    }
}
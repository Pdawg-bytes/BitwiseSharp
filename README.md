# BitwiseSharp Expression Evaluator

## Overview
BitwiseSharp is a powerful expression evaluator designed to evaluate complex bitwise, arithmetic, and variable-based expressions. It supports tokenization, parsing, and evaluating mathematical and bitwise operations on arbitrary precision numbers (using `System.Numerics.BigInteger`). The project implements a recursive descent parser and an environment-based evaluator for handling variables and their assignments.

The core functionality supports bitwise operations such as `AND`, `OR`, `XOR`, `NOT`, bit shifting, and traditional arithmetic operations (`+`, `-`, `*`, `/`, `%`).

## Features
- **Bitwise Operations:** Supports common bitwise operators like `AND (&)`, `OR (|)`, `XOR (^)`, and `NOT (~)`.
- **Arithmetic Operations:** Supports standard arithmetic operations: `+`, `-`, `*`, `/`, and `%`.
- **Variable Assignment:** You can assign values to existing variables, or create new ones using the `let` keyword and perform operations using them.
- **Arbitrary Precision:** Uses `BigInteger` from `System.Numerics` to handle expressions involving large numbers.
- **Recursive Descent Parser:** The parser uses a recursive descent method to build an Abstract Syntax Tree (AST) from tokens.
- **Verbose Logging:** Enables logging of parsing steps and evaluation process for debugging purposes.

## Known Limitations
- Due to the way the `<<` and `>>` operators work in C#, the right-hand operand of the expression must be within the bounds of [`int.MinValue`, `int.MaxValue`].
- This library may not be the most efficient in terms of performance, as `BigInteger` is a heap-allocated type designed for arbitrary-precision numbers.
    - Do note that it is possible to change the numeric type by editing the `global using ArbitraryNumber = <type>;` line in `BitwiseEvaluator.cs` and recompiling the library; however, keep in mind that it defaults to `BigInteger`.

## How It Works
1. **Tokenization:** The expression string is first tokenized into meaningful components such as numbers, operators.
    - Relies on Regex to tokenize the expression into string literals, which are then converted to proper tokens.
2. **Parsing:** The list of tokens is parsed into an Abstract Syntax Tree (AST) using a recursive descent parser.
3. **Evaluation:** The AST is then evaluated, and the result is returned as an `ArbitraryNumber`.

## Example Expressions
```js
>>> let x = 10 + 2
12
>>> let y = x << 2
48
>>> 3 | y
51
```
- `x` is assigned the value `12` (variable assignemnts and definitions are evaluated inline; variables can only store an `ArbitraryNumber`).
- `y` is assigned the value of `x << 2`.
- Computes `3 | y` and returns the result without storing it.

## Usage
You can use the `BitwiseEvalulator` class to evaluate expressions:

```csharp
using BitwiseSharp;
using BitwiseSharp.Types;

// Output any generated logs to the console.
static void LogCtx_LogGenerated(object? sender, LogGeneratedEventArgs e) => Console.WriteLine(e.LogData);

static void Main()
{
    // Initializes a new logging context with logging & ANSI colors enabled.
    var logCtx = new VerboseLogContext(true, true);
    logCtx.LogGenerated += LogCtx_LogGenerated;

    var envCtx = new EnvironmentContext();

    // Initializes a new evalulator with a fresh environment context.
    var evaluator = new BitwiseEvalulator(logCtx, envCtx);
    string expression = "(~(3 | 4) & 9) + 2 << 3 + 7";

    // Evaluates the expression and returns a Result<ArbitraryNumber> containing its value or error message.
    var result = evaluator.EvaluateExpression(expression);
    if (result.IsSuccess)
        Console.WriteLine($"Evaluation result: {result.Value}"); // Outputs: 10240
    else
        Console.WriteLine($"Error: {result.Error}");
}
```
#### Arguments
- `expression`: The expression to evaluate, provided as a string.
#### Return Value
The evaluation result is returned as a Result<ArbitraryNumber>, which contains the evaluated result or an error message if the evaluation fails.

\
You may also modify or interact with the `EnvironmentContext` of the evaluator:
```csharp
// Previous initialization code above...

// Attempts to create a new symbol called "name" with the value 40. This will return false if the symbol is already defined.
envCtx.TryCreateVariable("name", 40);

// ouput will contain the value 40. This will return false if the symbol does not exist.
System.Numerics.BigInteger output;
envCtx.TryGetVariable("name", out output);

// First, we check if the variable is defined, and, if so, we set it to 50.
if (envCtx.HasVariable("name"))
    envCtx.SetVariable("name", 50);

// Attempts to remove the symbol called "name". This will return false if the symbol does not exist.
envCtx.TryRemoveVariable("name");
```

## Project Structure
- ### `BitwiseSharp`
    The main library containing the tokenizer, parser, evaluator, and types.
- ### `InteractiveShell`
    A sample program that provides a simple shell to interface with the evaluator.
- ### `WASMBridge`
    A project targeting `browser-wasm` to allow for use in the browser.

## Contributions
Feel free to open an issue or submit a PR if you encounter bugs or wish to contribute improvements!

## License
This project is licensed under the [BSD-3-Clause License](https://opensource.org/license/bsd-3-clause) - see the LICENSE file for details.
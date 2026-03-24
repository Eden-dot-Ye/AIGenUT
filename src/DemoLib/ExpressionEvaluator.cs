using System.Globalization;

namespace DemoLib;

/// <summary>
/// A mathematical expression evaluator that supports parsing and evaluating
/// arithmetic expressions with variables, functions, and operator precedence.
/// Supports: +, -, *, /, %, ^ (power), parentheses, and built-in functions.
/// </summary>
public class ExpressionEvaluator
{
    private readonly Dictionary<string, double> _variables = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Func<double[], double>> _functions;

    public ExpressionEvaluator()
    {
        _functions = new Dictionary<string, Func<double[], double>>(StringComparer.OrdinalIgnoreCase)
        {
            ["abs"] = args => Math.Abs(args[0]),
            ["sqrt"] = args =>
            {
                if (args[0] < 0)
                    throw new ArgumentException("Cannot compute square root of a negative number.");
                return Math.Sqrt(args[0]);
            },
            ["max"] = args => args.Max(),
            ["min"] = args => args.Min(),
            ["round"] = args => args.Length > 1
                ? Math.Round(args[0], (int)args[1], MidpointRounding.AwayFromZero)
                : Math.Round(args[0], MidpointRounding.AwayFromZero),
            ["floor"] = args => Math.Floor(args[0]),
            ["ceil"] = args => Math.Ceiling(args[0]),
            ["sin"] = args => Math.Sin(args[0]),
            ["cos"] = args => Math.Cos(args[0]),
            ["log"] = args => args.Length > 1
                ? Math.Log(args[0], args[1])
                : Math.Log(args[0]),
            ["pow"] = args => Math.Pow(args[0], args[1]),
        };
    }

    /// <summary>
    /// Sets a variable value for use in expressions.
    /// </summary>
    public void SetVariable(string name, double value)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Variable name cannot be empty.", nameof(name));
        _variables[name] = value;
    }

    /// <summary>
    /// Gets a variable value, or throws if not defined.
    /// </summary>
    public double GetVariable(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (!_variables.TryGetValue(name, out var value))
            throw new KeyNotFoundException($"Variable '{name}' is not defined.");
        return value;
    }

    /// <summary>
    /// Clears all defined variables.
    /// </summary>
    public void ClearVariables() => _variables.Clear();

    /// <summary>
    /// Registers a custom function for use in expressions.
    /// </summary>
    public void RegisterFunction(string name, Func<double[], double> implementation)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(implementation);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Function name cannot be empty.", nameof(name));
        _functions[name] = implementation;
    }

    /// <summary>
    /// Evaluates a mathematical expression string and returns the result.
    /// Supports: numbers, +, -, *, /, %, ^ (power), parentheses, variables, and functions.
    /// </summary>
    public double Evaluate(string expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Expression cannot be empty.", nameof(expression));

        var tokens = Tokenize(expression);
        var pos = 0;
        var result = ParseExpression(tokens, ref pos);

        if (pos < tokens.Count)
            throw new FormatException($"Unexpected token '{tokens[pos].Value}' at position {tokens[pos].Position}.");

        return result;
    }

    /// <summary>
    /// Tries to evaluate an expression, returning success/failure without throwing.
    /// </summary>
    public bool TryEvaluate(string expression, out double result)
    {
        result = 0;
        try
        {
            result = Evaluate(expression);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Tokenizes the expression string into a list of tokens.
    /// </summary>
    private List<Token> Tokenize(string expression)
    {
        var tokens = new List<Token>();
        var i = 0;

        while (i < expression.Length)
        {
            var ch = expression[i];

            if (char.IsWhiteSpace(ch))
            {
                i++;
                continue;
            }

            if (char.IsDigit(ch) || ch == '.')
            {
                var start = i;
                var hasDot = ch == '.';
                i++;
                while (i < expression.Length && (char.IsDigit(expression[i]) || expression[i] == '.'))
                {
                    if (expression[i] == '.')
                    {
                        if (hasDot)
                            throw new FormatException($"Invalid number at position {start}.");
                        hasDot = true;
                    }
                    i++;
                }
                var numStr = expression[start..i];
                if (!double.TryParse(numStr, CultureInfo.InvariantCulture, out _))
                    throw new FormatException($"Invalid number '{numStr}' at position {start}.");
                tokens.Add(new Token(TokenType.Number, numStr, start));
                continue;
            }

            if (char.IsLetter(ch) || ch == '_')
            {
                var start = i;
                i++;
                while (i < expression.Length && (char.IsLetterOrDigit(expression[i]) || expression[i] == '_'))
                    i++;
                var name = expression[start..i];
                tokens.Add(new Token(TokenType.Identifier, name, start));
                continue;
            }

            switch (ch)
            {
                case '+':
                case '-':
                case '*':
                case '/':
                case '%':
                case '^':
                    tokens.Add(new Token(TokenType.Operator, ch.ToString(), i));
                    i++;
                    break;
                case '(':
                    tokens.Add(new Token(TokenType.LeftParen, "(", i));
                    i++;
                    break;
                case ')':
                    tokens.Add(new Token(TokenType.RightParen, ")", i));
                    i++;
                    break;
                case ',':
                    tokens.Add(new Token(TokenType.Comma, ",", i));
                    i++;
                    break;
                default:
                    throw new FormatException($"Unexpected character '{ch}' at position {i}.");
            }
        }

        return tokens;
    }

    private double ParseExpression(List<Token> tokens, ref int pos)
    {
        var left = ParseTerm(tokens, ref pos);

        while (pos < tokens.Count && tokens[pos].Type == TokenType.Operator
               && (tokens[pos].Value == "+" || tokens[pos].Value == "-"))
        {
            var op = tokens[pos].Value;
            pos++;
            var right = ParseTerm(tokens, ref pos);
            left = op == "+" ? left + right : left - right;
        }

        return left;
    }

    private double ParseTerm(List<Token> tokens, ref int pos)
    {
        var left = ParsePower(tokens, ref pos);

        while (pos < tokens.Count && tokens[pos].Type == TokenType.Operator
               && (tokens[pos].Value == "*" || tokens[pos].Value == "/" || tokens[pos].Value == "%"))
        {
            var op = tokens[pos].Value;
            pos++;
            var right = ParsePower(tokens, ref pos);

            left = op switch
            {
                "*" => left * right,
                "/" => right == 0
                    ? throw new DivideByZeroException("Division by zero.")
                    : left / right,
                "%" => right == 0
                    ? throw new DivideByZeroException("Modulo by zero.")
                    : left % right,
                _ => left
            };
        }

        return left;
    }

    private double ParsePower(List<Token> tokens, ref int pos)
    {
        var baseVal = ParseUnary(tokens, ref pos);

        if (pos < tokens.Count && tokens[pos].Type == TokenType.Operator && tokens[pos].Value == "^")
        {
            pos++;
            var exponent = ParsePower(tokens, ref pos); // Right-associative
            return Math.Pow(baseVal, exponent);
        }

        return baseVal;
    }

    private double ParseUnary(List<Token> tokens, ref int pos)
    {
        if (pos < tokens.Count && tokens[pos].Type == TokenType.Operator)
        {
            if (tokens[pos].Value == "-")
            {
                pos++;
                return -ParseUnary(tokens, ref pos);
            }
            if (tokens[pos].Value == "+")
            {
                pos++;
                return ParseUnary(tokens, ref pos);
            }
        }

        return ParsePrimary(tokens, ref pos);
    }

    private double ParsePrimary(List<Token> tokens, ref int pos)
    {
        if (pos >= tokens.Count)
            throw new FormatException("Unexpected end of expression.");

        var token = tokens[pos];

        // Number literal
        if (token.Type == TokenType.Number)
        {
            pos++;
            return double.Parse(token.Value, CultureInfo.InvariantCulture);
        }

        // Identifier (variable or function call)
        if (token.Type == TokenType.Identifier)
        {
            var name = token.Value;
            pos++;

            // Check if it's a function call
            if (pos < tokens.Count && tokens[pos].Type == TokenType.LeftParen)
            {
                pos++; // skip '('
                var args = new List<double>();

                if (pos < tokens.Count && tokens[pos].Type != TokenType.RightParen)
                {
                    args.Add(ParseExpression(tokens, ref pos));
                    while (pos < tokens.Count && tokens[pos].Type == TokenType.Comma)
                    {
                        pos++; // skip ','
                        args.Add(ParseExpression(tokens, ref pos));
                    }
                }

                if (pos >= tokens.Count || tokens[pos].Type != TokenType.RightParen)
                    throw new FormatException($"Missing closing parenthesis for function '{name}'.");
                pos++; // skip ')'

                if (!_functions.TryGetValue(name, out var func))
                    throw new FormatException($"Unknown function '{name}'.");

                return func(args.ToArray());
            }

            // It's a variable
            if (_variables.TryGetValue(name, out var value))
                return value;

            // Check built-in constants
            if (name.Equals("PI", StringComparison.OrdinalIgnoreCase))
                return Math.PI;
            if (name.Equals("E", StringComparison.OrdinalIgnoreCase))
                return Math.E;

            throw new FormatException($"Unknown variable '{name}'.");
        }

        // Parenthesized expression
        if (token.Type == TokenType.LeftParen)
        {
            pos++; // skip '('
            var result = ParseExpression(tokens, ref pos);
            if (pos >= tokens.Count || tokens[pos].Type != TokenType.RightParen)
                throw new FormatException("Missing closing parenthesis.");
            pos++; // skip ')'
            return result;
        }

        throw new FormatException($"Unexpected token '{token.Value}' at position {token.Position}.");
    }

    private enum TokenType
    {
        Number,
        Operator,
        LeftParen,
        RightParen,
        Comma,
        Identifier
    }

    private record Token(TokenType Type, string Value, int Position);
}

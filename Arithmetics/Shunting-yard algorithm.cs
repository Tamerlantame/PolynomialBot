using Arithmetics.Polynomial1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Arithmetics.Parsers
{
    //C# realization of Shunting-yard algorithm
    //used this git link
    //https://en.wikipedia.org/wiki/Abstract_syntax_tree
    //https://en.wikipedia.org/wiki/Shunting-yard_algorithm
    //https://web.archive.org/web/20110718214204/http://en.literateprograms.org/Shunting_yard_algorithm_(C)
    //https://gist.github.com/istupakov/c49ef290c3bc3dbe329bf68f67971470
    //https://www.codeproject.com/Tips/351042/Shunting-Yard-algorithm-in-Csharp
    public enum TokenType { Polynomial, Variable, Function, Parenthesis, Operator, Comma, WhiteSpace, Exeption };

    public enum FunctionType { Unary, Binary };

    public enum OperatorType { Binary, Boolean };

    public delegate Polynomial BinaryFunc(Polynomial leftOp, Polynomial rightOp);

    public delegate bool BinaryBoolOperator(Polynomial leftOp, Polynomial rightOp);

    public delegate Polynomial UnaryFunc(Polynomial Func);

    public struct Token
    {
        public TokenType Type { get; }
        public string Value { get; }

        public override string ToString()
        {
            return $"{Type}: {Value}";
        }

        public Token(TokenType type, string value)
        {
            Type = type;
            Value = value;
        }
    }

    public class Operator : IComparable<Operator>
    {
        //Двусимвольные Операторы пока недоступны для использования
        private static readonly IDictionary<string, Operator> operators = new Dictionary<string, Operator>
        {
            ["+"] = new Operator
            {
                Name = "+",
                Precedence = 1,
                Type = OperatorType.Binary,
                biOperator = (Polynomial x, Polynomial y) =>
                {
                    return x + y;
                }
            },
            ["-"] = new Operator
            {
                Name = "-",
                Precedence = 1,
                Type = OperatorType.Binary,
                biOperator = (Polynomial x, Polynomial y) =>
                {
                    return x - y;
                }
            },
            ["*"] = new Operator
            {
                Name = "*",
                Precedence = 2,
                Type = OperatorType.Binary,
                biOperator = (Polynomial x, Polynomial y) =>
                {
                    return x * y;
                }
            },
            ["/"] = new Operator
            {
                Name = "/",
                Precedence = 2,
                Type = OperatorType.Binary,
                biOperator = (Polynomial x, Polynomial y) =>
                {
                    return x / y;
                }
            },
            ["%"] = new Operator
            {
                Name = "%",
                Precedence = 2,
                Type = OperatorType.Binary,
                biOperator = (Polynomial x, Polynomial y) =>
                {
                    return x % y;
                }
            },
            //["^"] = new Operator { Name = "^", Precedence = 3, RightAssociative = true, function = ((double x, double y) => Math.Pow(x, y)) }
            [">"] = new Operator
            {
                Name = ">",
                Precedence = 0,
                Type = OperatorType.Boolean,
                biBoolOperator = (Polynomial x, Polynomial y) =>
                {
                    return x > y;
                }
            },
            [">="] = new Operator
            {
                Name = ">=",
                Precedence = 0,
                Type = OperatorType.Boolean,
                biBoolOperator = (Polynomial x, Polynomial y) =>
                {
                    return x >= y;
                }
            },
            ["<"] = new Operator
            {
                Name = "<",
                Precedence = 0,
                Type = OperatorType.Boolean,
                biBoolOperator = (Polynomial x, Polynomial y) =>
                {
                    return x < y;
                }
            },
            ["<="] = new Operator
            {
                Name = "<=",
                Precedence = 0,
                Type = OperatorType.Boolean,
                biBoolOperator = (Polynomial x, Polynomial y) =>
                {
                    return x <= y;
                }
            },
            ["=="] = new Operator
            {
                Name = "==",
                Precedence = 0,
                Type = OperatorType.Boolean,
                biBoolOperator = (Polynomial x, Polynomial y) =>
                {
                    return x == y;
                }
            },
            ["!="] = new Operator
            {
                Name = "!=",
                Precedence = 0,
                Type = OperatorType.Boolean,
                biBoolOperator = (Polynomial x, Polynomial y) =>
                {
                    return x != y;
                }
            },
        };

        public OperatorType Type { get; set; }
        public string Name { get; set; }
        public int Precedence { get; set; }
        public bool RightAssociative { get; set; }

        public BinaryFunc biOperator;
        public BinaryBoolOperator biBoolOperator;

        /// <summary>
        /// return IDictionary with operators
        /// </summary>
        /// <returns></returns>
        public static IDictionary<string, Operator> GetOperators()
        {
            return operators;
        }

        public int CompareTo(Operator other)
        {
            return this.Precedence - other.Precedence;
        }
    }

    public class Function : IComparable<Function>
    {
        public FunctionType Type { get; set; }
        public string Name { get; set; }
        public int Precedence { get; set; }
        public BinaryFunc BiFunction;
        public UnaryFunc UFunction;

        /// <summary>
        /// return IDictionary with functions
        /// </summary>
        /// <returns></returns>
        private static readonly IDictionary<string, Function> functions = new Dictionary<string, Function>
        {
            ["Eval"] = new Function { Name = "Eval", Precedence = 0, Type = FunctionType.Binary, BiFunction = ((Polynomial x, Polynomial y) => x.Eval(y)) },
            ["Diff"] = new Function { Name = "Diff", Precedence = 0, Type = FunctionType.Unary, UFunction = ((Polynomial x) => Polynomial.Diff(x)) }
        };

        public static IDictionary<string, Function> GetFunctions()
        {
            return functions;
        }

        public int CompareTo(Function other)
        {
            return this.Precedence - other.Precedence;
        }
    }

    internal class Parser
    {
        public static IDictionary<string, Operator> operators = Operator.GetOperators();
        public static IDictionary<string, Function> functions = Function.GetFunctions();

        private bool CompareOperators(Operator op1, Operator op2)
        {
            return op1.RightAssociative ? op1.Precedence < op2.Precedence : op1.Precedence <= op2.Precedence;
        }

        private bool CompareOperators(string op1, string op2) => CompareOperators(operators[op1], operators[op2]);

        private TokenType DetermineType(char ch)
        {
            if (char.IsLetter(ch) && ch != 'x')
                return TokenType.Variable;
            if (char.IsDigit(ch) || ch == '^' || ch == 'x')
                return TokenType.Polynomial;
            if (char.IsWhiteSpace(ch))
                return TokenType.WhiteSpace;
            if (ch == ',')
                return TokenType.Comma;
            if (ch == '(' || ch == ')')
                return TokenType.Parenthesis;
            if (operators.ContainsKey(Convert.ToString(ch)))
                return TokenType.Operator;
            return TokenType.Exeption;
        }

        public IEnumerable<Token> Tokenize(TextReader reader)
        {
            var token = new StringBuilder();

            int curr;
            while ((curr = reader.Read()) != -1)
            {
                var ch = (char)curr;
                var currType = DetermineType(ch);
                if (currType == TokenType.WhiteSpace)
                    continue;
                if (currType == TokenType.Exeption)
                {
                    ShuntingYardException ShuntingYardExeption = new ShuntingYardException("Wrong symbol", ch);
                    continue;
                }

                token.Append(ch);

                var next = reader.Peek();
                var nextType = next != -1 ? DetermineType((char)next) : TokenType.WhiteSpace;
                if (currType != nextType)
                {
                    if (next == '(')
                        yield return new Token(TokenType.Function, token.ToString());
                    else
                        yield return new Token(currType, token.ToString());
                    token.Clear();
                }
            }
        }

        public IEnumerable<Token> ShuntingYard(IEnumerable<Token> tokens)
        {
            var stack = new Stack<Token>();
            foreach (var tok in tokens)
            {
                switch (tok.Type)
                {
                    case TokenType.Polynomial:
                    case TokenType.Variable:
                        yield return tok;
                        break;

                    case TokenType.Function:
                        stack.Push(tok);
                        break;

                    case TokenType.Comma:
                        while (stack.Peek().Value != "(")
                            yield return stack.Pop();
                        break;

                    case TokenType.Operator:
                        while (stack.Any() && stack.Peek().Type == TokenType.Operator && CompareOperators(tok.Value, stack.Peek().Value))
                            yield return stack.Pop();
                        stack.Push(tok);
                        break;

                    case TokenType.Parenthesis:
                        if (tok.Value == "(")
                            stack.Push(tok);
                        else
                        {
                            if (stack.Count != 0)
                            {
                                while (stack.Peek().Value != "(")
                                    yield return stack.Pop();
                                stack.Pop();

                                if (stack.Peek().Type == TokenType.Function)
                                    yield return stack.Pop();
                            }
                        }
                        break;

                    default:
                        ShuntingYardException ShuntingYardExeption = new ShuntingYardException("Wrong token", tok);
                        break;
                }
            }
            while (stack.Any())
            {
                var tok = stack.Pop();
                if (tok.Type == TokenType.Parenthesis)
                {
                    ShuntingYardException ShuntingYardExeption = new ShuntingYardException("Mismatched parentheses");
                    continue;
                }
                yield return tok;
            }
        }
    }
}
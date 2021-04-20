using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Arithmetics.Parsers;
using Arithmetics.Polynomial1;

namespace Arithmetics
{
    class PolynomialСalculator
    {
        public Dictionary<string, Polynomial> PolyVars { get; private set; }
        public PolynomialСalculator()
        {
            PolyVars = new Dictionary<string, Polynomial>();
        }
        public PolynomialСalculator(Dictionary<string, Polynomial> vars)
        {
            PolyVars = new Dictionary<string, Polynomial>(vars);
        }

        /// <summary>
        /// Парсит строку в обратную польскую запись
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private string ExpressionToRPN(string expression, out List<Token> tokens)
        {
            var text = expression;
            var reader = new StringReader(text);
            var parser = new Parser();
            tokens = parser.Tokenize(reader).ToList();
            var rpn = parser.ShuntingYard(tokens);
            var rpnStr = string.Join(" ", rpn.Select(t => t.Value));
            tokens = rpn.ToList();
            return rpnStr;
        }
        /// <summary>
        /// вычисляет выражение, записанное в RPN(обратная польская запись)
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public string RPNToAnswer(string expression)
        {

            var text = ExpressionToRPN(expression, out var tokensList);
            var tokens = tokensList.ToArray();
            var stack = new Stack<Token>();
            Token leftOp, rightOp;
            Polynomial result;
            for (int i = 0; i < tokens.Length; i++)
            {
                switch (tokens[i].Type)
                {
                    case TokenType.Polynomial:
                        stack.Push(tokens[i]);
                        break;
                    case TokenType.Variable:
                        if (PolyVars.ContainsKey(tokens[i].Value.ToString()))
                            stack.Push(new Token(TokenType.Polynomial, PolyVars[tokens[i].Value].ToString()));
                        else
                            stack.Push(tokens[i]);
                        break;
                    case TokenType.Function:
                        if (Function.GetFunctions()[tokens[i].Value].Type == FunctionType.Unary)
                        {
                            try
                            {
                                leftOp = stack.Pop();
                            }
                            catch (System.InvalidOperationException)
                            {
                                SyntaxException syntaxException = new SyntaxException("Не удалось определить функцию");
                                leftOp = new Token(TokenType.Exeption, syntaxException.Message);
                            }
                            result = Function.GetFunctions()[tokens[i].Value].UFunction
                              (
                              new Polynomial(PolynomialParser.Parse(leftOp.Value))
                              );
                            stack.Push(new Token(TokenType.Polynomial, result.ToString()));
                        }
                        else
                        {
                            try
                            {
                                rightOp = stack.Pop();
                                leftOp = stack.Pop();
                            }
                            catch (System.InvalidOperationException)
                            {
                                SyntaxException syntaxException = new SyntaxException("Не удалось применить функцию к операнду(ам)");
                                rightOp = new Token(TokenType.Exeption, syntaxException.Message);
                                leftOp  = new Token(TokenType.Exeption, syntaxException.Message);
                            }
                            result = Function.GetFunctions()[tokens[i].Value].BiFunction
                              (
                              new Polynomial(PolynomialParser.Parse(leftOp.Value)),
                              new Polynomial(PolynomialParser.Parse(rightOp.Value))
                              );
                            stack.Push(new Token(TokenType.Polynomial, result.ToString()));
                        }
                        break;
                    case TokenType.Operator:
                        try
                        {
                            rightOp = stack.Pop();
                            //leftOp = stack.Pop();
                        }
                        catch (System.InvalidOperationException)
                        {
                            SyntaxException syntaxException = new SyntaxException("Не удалось применить оператор к операнду(ам)");
                            rightOp = new Token(TokenType.Polynomial, "0");
                            //leftOp  = new Token(TokenType.Exeption, syntaxException.Message);

                        }
                        try
                        {
                            //rightOp = stack.Pop();
                            leftOp = stack.Pop();
                        }
                        catch (System.InvalidOperationException)
                        {
                            SyntaxException syntaxException = new SyntaxException("Не удалось применить оператор к операнду(ам)");
                            //rightOp = new Token(TokenType.Exeption, syntaxException.Message);
                            leftOp  = new Token(TokenType.Polynomial, "0");

                        }
                        if (Operator.GetOperators()[tokens[i].Value].Type == OperatorType.Binary)
                        {
                            try
                            {
                                result = Operator.GetOperators()[tokens[i].Value].biOperator
                                   (
                                   new Polynomial(PolynomialParser.Parse(leftOp.Value)),
                                   new Polynomial(PolynomialParser.Parse(rightOp.Value))
                                   );
                                stack.Push(new Token(TokenType.Polynomial, result.ToString()));
                            }
                            catch(InvalidPolynomialStringException exception)
                            {
                                stack.Push(new Token(TokenType.Exeption, exception.Message));
                            }
                            
                        }
                        else//TODO подумать как детальнее реализовать взаимодействие с логическими операциями
                        {
                            result = Convert.ToDouble(Operator.GetOperators()[tokens[i].Value].biBoolOperator
                            (
                            new Polynomial(PolynomialParser.Parse(leftOp.Value)),
                            new Polynomial(PolynomialParser.Parse(rightOp.Value))
                            ));
                            stack.Push(new Token(TokenType.Polynomial, result.ToString()));
                        }
                        break;

                    default:
                        throw new Exception("Wrong token");
                }
            }
            try
            {
                text = stack.Pop().Value;
            }
            catch (System.InvalidOperationException)
            {
                text = "";
            }
            return text;
        }
    }
}

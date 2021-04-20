using System;

namespace ElementaryInterpreter
{
    public class Executor<T> where T : IComputerAlgebraType, new()
    {
        private readonly Calculator<T> calculator;

        public Executor()
        {
            calculator = new Calculator<T>();
        }

        public string GetVars()
        {
            string result = "";

            foreach (var item in calculator.Vars.Keys)
            {
                calculator.Vars.TryGetValue(item, out var computerAlgebraType);
                result += item + " := " + computerAlgebraType.ToString() + "\n";
            }
            return result;
        }

        public string Launch(string text)
        {
            var lines = text.Replace(" ", "").Split(new Char[] { '\n' });
            string result = "";
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("While"))
                {
                    result += PerformWhile(lines[i] + '\n' + '{' + '\n' + GetBody(text, out int bodyLenght) + '\n' + '}') + '\n';
                    i += bodyLenght;
                }
                else
                {
                    if (!(i < lines.Length))
                        break;
                    try
                    {
                        result += Execute(lines[i]) + "\n";
                    }
                    catch (Exception exeption)
                    {
                        result += exeption.Message + "\n";
                    }
                }
            }
            return result;
        }

        /// <summary>
        ///  A:= some expression,
        ///  A:= A*A;
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private string Execute(string expression)
        {
            string result;

            // нужен regex
            if (expression.Contains(":="))
            {
                string[] operands = expression.Split(new string[] { ":=" }, StringSplitOptions.None);
                result = calculator.Execute(operands[1]);
                if (!calculator.Vars.ContainsKey(operands[0]))
                    calculator.Vars.Add(operands[0], new T().ParseExpression(result));
                else
                    calculator.Vars[operands[0]] = new T().ParseExpression(result);
                result = $"{operands[0]}:={result}";
            }
            else
            {
                result = calculator.Execute(expression);
            }
            return result;
        }

        /// <summary>
        /// Get result of while cycle
        /// </summary>
        /// <param name="cycle">
        /// string with cycle in
        /// "While(...)
        /// {
        /// ...
        /// }"
        /// format</param>
        /// <returns></returns>
        private string PerformWhile(string cycle)
        {
            var lines = cycle;
            // Условие, добавлена только обработка >< оперторов
            var condition = lines.Substring(startIndex: lines.IndexOf('(') + 1, length: lines.IndexOf(')') - (lines.IndexOf('(') + 1));
            var result = "";
            while (Convert.ToBoolean(Convert.ToDouble(Execute(condition))))
            {
                result = Launch(GetBody(lines, out _));
            }
            return result;
        }

        private string GetBody(string text, out int bodyLength)
        {
            int leftBracetNum = 0, rightBracetNum = 0, stringNum = 0;
            int firstBracetIndex = 0, lastBracetIndex = text.Length;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                    stringNum++;
                if (text[i] == '{')
                    leftBracetNum++;
                if (text[i] == '}')
                    rightBracetNum++;
                if (leftBracetNum != 0 && leftBracetNum == rightBracetNum)
                    break;
            }
            if (leftBracetNum != rightBracetNum)
                throw new Exception("Ошибка с фигурными скобками");
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '{')
                {
                    firstBracetIndex = i;
                    break;
                }
            }
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '}')
                {
                    rightBracetNum--;
                    if (rightBracetNum == 0)
                        lastBracetIndex = i;
                }
            }
            bodyLength = stringNum;
            return text.Substring(firstBracetIndex + 1, lastBracetIndex - (firstBracetIndex + 1));
        }
    }
}
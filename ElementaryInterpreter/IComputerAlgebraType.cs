using System.Collections.Generic;

namespace ElementaryInterpreter
{
    public interface IComputerAlgebraType
    {
        IComputerAlgebraType ParseExpression(string expr);

        string Execute(string expression, Dictionary<string, IComputerAlgebraType> vars);
    }
}

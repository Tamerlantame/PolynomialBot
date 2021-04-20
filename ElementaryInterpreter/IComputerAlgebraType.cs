namespace ElementaryInterpreter
{
    using System.Collections.Generic;

    public interface IComputerAlgebraType
    {
        IComputerAlgebraType ParseExpression(string expr);

        string Execute(string expression, Dictionary<string, IComputerAlgebraType> vars);
    }
}

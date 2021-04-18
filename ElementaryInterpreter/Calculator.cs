using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementaryInterpreter
{
    public class Calculator<T> where T : IComputerAlgebraType, new()
    {
        public Dictionary<string, IComputerAlgebraType> Vars { get; private set; }
        public Calculator()
        {
            Vars = new Dictionary<string, IComputerAlgebraType>();
        }
        public string Execute(string expr) => new T().Execute(expr, Vars);
    }
}

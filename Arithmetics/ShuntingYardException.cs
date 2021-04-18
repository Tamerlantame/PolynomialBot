using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arithmetics.Parsers
{
    class ShuntingYardException : Exception
    {
        readonly Token token;
        readonly char symbol;
        public ShuntingYardException(string message) : base(message)
        {
        }
        public ShuntingYardException(string message, Token token) : base(message)
        {
            this.token = token;
        }
        public ShuntingYardException(string message, char symbol) : base(message)
        {
            this.symbol = symbol;
        }
    }
}

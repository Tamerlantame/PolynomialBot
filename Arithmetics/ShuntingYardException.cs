using System;

namespace Arithmetics.Parsers
{
    internal class ShuntingYardException : Exception
    {
        private readonly Token token;
        private readonly char symbol;

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
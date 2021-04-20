// <copyright file="InvalidPolynomialStringException.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Arithmetics.Polynomial1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class InvalidPolynomialStringException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidPolynomialStringException"/> class.
        /// </summary>
        /// <param name="massage"></param>
        public InvalidPolynomialStringException(string massage)
            : base(massage)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidPolynomialStringException"/> class.
        /// </summary>
        public InvalidPolynomialStringException() : base()
        {
        }
    }
}

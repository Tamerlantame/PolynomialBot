using Arithmetics.Polynomial1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arithmetics.Parsers
{
    /// <summary>
    /// Contains method for convertion of a given string representation of polynomials into lists of coefficients.
    /// </summary>
    public class PolynomialParser
    {
        /// <summary>
        /// Converts a given string representation <paramref name="polyString"/> of a polynomial in one variable to a <see cref="SortedList{int, double}"/> of integer powers as keys
        /// and double coefficients as values. <br/>
        /// The representation has form of a sum of monomials of the form either a, ax or ax^b for some integers a and b.
        /// </summary>
        /// <param name="polyString">string representation of a polynomial</param>
        /// <returns>coefficents of a polynomial as <see cref="SortedList{int, double}"/></returns>
        /// <exception cref="InvalidPolynomialStringException">Thrown when <paramref name="polyString"/> does not have the specified form</exception>
        public static SortedList<int, double> Parse(string polyString)
        {
            SortedList<int, double> coefficientsList = new SortedList<int, double>();
            int monomialDeg, indexOfDeg, indexOfx;
            double monomialCoeff;
            try
            {
                polyString = polyString.Replace("-", "+-");
                polyString = polyString.Replace("-x", "-1x");
                string[] s = polyString.Split(new char[] { '+' });
                foreach (string monomial in s)
                {
                    if (monomial == "")
                        continue;
                    if(monomial.Contains("x"))
                    {
                        monomialDeg = 1;
                        indexOfx = monomial.IndexOf("x");
                        monomialCoeff = monomial[0] == 'x' ? 1 : Convert.ToDouble(monomial.Substring(0, indexOfx));
                        if (monomial.Contains("^"))
                        {
                            indexOfDeg = indexOfx + 2; // i.e. index of the symbol directly after substring "x^"
                            monomialDeg = Convert.ToInt32(monomial.Substring(indexOfDeg));
                        }
                        if (!coefficientsList.ContainsKey(monomialDeg))
                            coefficientsList.Add(monomialDeg, monomialCoeff);
                        else
                            coefficientsList[monomialDeg] += monomialCoeff;
                    }
                    else
                    {
                        if (!coefficientsList.ContainsKey(0))
                            coefficientsList.Add(0, Convert.ToDouble(monomial));
                        else
                            coefficientsList[0] += Convert.ToDouble(monomial);
                    }
                }

            }
            catch (Exception e)
            {
                //coefficientsList.Clear();
                //coefficientsList.Add(0, 0);
                InvalidPolynomialStringException invalidPolynomialStringException = new InvalidPolynomialStringException(e.StackTrace);
            }
            return coefficientsList;
    }

    }
}

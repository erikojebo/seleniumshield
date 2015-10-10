using System;
using SeleniumShield.Exceptions;

namespace SeleniumShield.UIRunner.Exceptions
{
    public class SeleniumShieldConversionException : SeleniumShieldException
    {
        public SeleniumShieldConversionException(string message) : base(message)
        {
        }

        public SeleniumShieldConversionException(string message, params object[] args) : base(message, args)
        {
        }

        public SeleniumShieldConversionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
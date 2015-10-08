using System;

namespace SeleniumShield.Exceptions
{
    public class SeleniumShieldAssertionFailedException : Exception
    {
        public SeleniumShieldAssertionFailedException(string message) : base(message)
        {
        }
        
        public SeleniumShieldAssertionFailedException(string message, params object[] args) : base(string.Format(message, args))
        {
        }
        
        public SeleniumShieldAssertionFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
using System;

namespace SeleniumShield.Exceptions
{
    public class SeleniumShieldException : Exception
    {
        public SeleniumShieldException(string message) : base(message)
        {
        }
        
        public SeleniumShieldException(string message, params object[] args) : base(string.Format(message, args))
        {
        }
        
        public SeleniumShieldException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
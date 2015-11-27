using System;

namespace SeleniumShield.Driver.Exceptions
{
    public class SeleniumShieldDriverTimeoutException : SeleniumShieldDriverException
    {
        public SeleniumShieldDriverTimeoutException(string message, Exception innerException) : base(message, innerException)
        {
            
        }

        public SeleniumShieldDriverTimeoutException(string format, params object[] formatParams) : base(format, formatParams)
        {
        }
    }
}
using System;
using OpenQA.Selenium;

namespace SeleniumShield.Driver.Exceptions
{
    public class SeleniumShieldDriverException : WebDriverException
    {
        public SeleniumShieldDriverException(string message) : base(message)
        {
        }

        public SeleniumShieldDriverException(string message, params object[] formatParams) : base(string.Format(message, formatParams))
        {
        }

        public SeleniumShieldDriverException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
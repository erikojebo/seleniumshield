using System;

namespace SeleniumShield.Tests.Infrastructure
{
    public class PreconfiguredFailureException : Exception
    {
        public PreconfiguredFailureException(string message) : base(message)
        {
            
        }
    }
}
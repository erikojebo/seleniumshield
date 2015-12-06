using System;

namespace SeleniumShield.Tests.Infrastructure
{
    public class PreconfiguredFailureException : Exception
    {
        public PreconfiguredFailureException(string message = "Configured to fail") : base(message)
        {
            
        }
    }
}
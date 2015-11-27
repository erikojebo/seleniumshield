using System;

namespace SeleniumShield.Driver.Logging
{
    public class SeleniumShieldDriverConsoleLogger : ISeleniumShieldDriverLogger
    {
        private readonly LogLevel _logLevel;

        public SeleniumShieldDriverConsoleLogger(LogLevel logLevel)
        {
            _logLevel = logLevel;
        }

        public void Log(LogLevel logLevel, string message)
        {
            if (_logLevel >= logLevel)
                Console.WriteLine($"[{DateTime.Now}] {message}");
        }
    }
}
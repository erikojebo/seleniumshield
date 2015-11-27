namespace SeleniumShield.Driver.Logging
{
    public interface ISeleniumShieldDriverLogger
    {
        void Log(LogLevel logLevel, string message);
    }
}
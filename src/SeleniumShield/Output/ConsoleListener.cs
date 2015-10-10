using System;

namespace SeleniumShield.Output
{
    public class ConsoleListener : IResultListener
    {
        public void OutputLine(string message, params object[] formatParams)
        {
            Console.WriteLine(message, formatParams);
        }
    }
}
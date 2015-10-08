using System;

namespace SeleniumShield.Output
{
    public class ConsoleListener : IResultListener
    {
        public void Output(string message)
        {
            Console.Write(message);
        }
    }
}
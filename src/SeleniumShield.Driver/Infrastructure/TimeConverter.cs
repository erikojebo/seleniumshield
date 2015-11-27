using System;

namespace SeleniumShield.Driver.Infrastructure
{
    internal class TimeConverter
    {
        public static int ToMilliseconds(double valueInSeconds)
        {
            return Convert.ToInt32(Math.Ceiling(valueInSeconds * 1000));
        }
    }
}
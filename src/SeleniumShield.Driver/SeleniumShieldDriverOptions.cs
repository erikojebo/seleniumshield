using SeleniumShield.Driver.Logging;

namespace SeleniumShield.Driver
{
    public class SeleniumShieldDriverOptions
    {
        public double DefaultTimeoutInSeconds { get; set; }
        public double DefaultRetryDelayInSeconds { get; set; }
        public string BaseUrl { get; set; }
        public ISeleniumShieldDriverLogger Logger { get; set; }
    }
}
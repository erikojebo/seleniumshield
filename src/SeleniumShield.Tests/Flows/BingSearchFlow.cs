using SeleniumShield.Extensions;

namespace SeleniumShield.Tests.Flows
{
    public class BingSearchFlow : AutomationFlow
    {
        public BingSearchFlow(string query)
        {
            AddStep($"Bing search for '{query}'", driver =>
            {
                driver.GoToUrl("http://www.bing.com");
                driver.WriteTo(".b_searchbox", query);
                driver.Submit(".b_searchboxSubmit");
            });
        }
    }
}
using SeleniumShield.Extensions;
using SeleniumShield.Metadata;

namespace SeleniumShield.Tests.Flows
{
    [UIExecutable(DisplayName = "Search Bing", Description = "Searches Microsoft Bing for the specified search query")]
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
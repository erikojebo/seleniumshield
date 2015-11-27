using SeleniumShield.Driver.Exceptions;

namespace SeleniumShield.Driver.Infrastructure
{
    public class UrlBuilder
    {
        public static string ToAbsoluteUrl(string baseUrl, string relativeUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new SeleniumShieldDriverException("Cannot create absolute url from relative url, since no base url has been specified");
            }

            var trimmedBaseUrl = baseUrl.TrimEnd('/');
            var trimmedRelativeUrl = relativeUrl.TrimStart('/');

            return $"{trimmedBaseUrl}/{trimmedRelativeUrl}";
        }
    }
}
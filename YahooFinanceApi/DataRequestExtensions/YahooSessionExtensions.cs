using Flurl;
using Flurl.Http;

namespace YahooFinanceApi.DataRequestExtensions;

public static class YahooSessionExtensions
{
    public static IFlurlRequest AddAuth(this Url url, YahooSession session)
    {
        return session.AddAuth(url);
    }
}
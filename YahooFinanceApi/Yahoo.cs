using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Flurl.Http;

namespace YahooFinanceApi;

public partial class Yahoo
{
    private static async Task<dynamic> GetResponseStreamAsync(Func<Task<dynamic>> doRequest)
    {
        bool reset = false;
        while (true)
        {
            try
            {
                return await doRequest().ConfigureAwait(false);
            }
            catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == (int)HttpStatusCode.NotFound)
            {
                return null;
            }
            catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == (int)HttpStatusCode.Unauthorized)
            {
                Debug.WriteLine("GetResponseStreamAsync: Unauthorized.");

                if (reset)
                    throw;
                reset = true; // try again with a new client
            }
        }
    }
}
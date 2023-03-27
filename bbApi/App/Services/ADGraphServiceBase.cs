using AutoMapper;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;

namespace bbApi.App.Services
{
    public class ADGraphServiceBase
    {
        protected GraphServiceClient graphServiceClient;
        protected readonly ITokenAcquisition tokenAcquisition;
        protected readonly IMapper mapper;

        public ADGraphServiceBase(ITokenAcquisition tokenAcquisition, IMapper mapper)
        {
            this.tokenAcquisition = tokenAcquisition;
            this.mapper = mapper;
            graphServiceClient = PrepareAuthenticatedClient();
        }

        private GraphServiceClient PrepareAuthenticatedClient()
        {
            if (graphServiceClient == null)
            {
                // Create Microsoft Graph client.
                // https://learn.microsoft.com/en-us/graph/sdks/choose-authentication-providers?tabs=CS#client-credentials-provider
                try
                {
                    var token = tokenAcquisition.GetAuthenticationResultForAppAsync("https://graph.microsoft.com/.default").Result;

                    var proxyAddress = "http://localhost:8888";

                    // Create a new System.Net.Http.HttpClientHandler with the proxy
                    var handler = new HttpClientHandler
                    {
                        // Create a new System.Net.WebProxy
                        // See WebProxy documentation for scenarios requiring
                        // authentication to the proxy
                        Proxy = new WebProxy(new Uri(proxyAddress))
                    };

                    //var httpClient = GraphClientFactory.Create(proxy: new WebProxy(new Uri(proxyAddress)));

                    graphServiceClient = new GraphServiceClient("https://graph.microsoft.com/v1.0/",
                                                                         new DelegateAuthenticationProvider(
                                                                             async (requestMessage) =>
                                                                             {
                                                                                 await Task.Run(() =>
                                                                                 {
                                                                                     requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token.AccessToken);
                                                                                 });
                                                                             }));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Could not create a graph client {ex}");
                }
            }

            return graphServiceClient;
        }
    }
}

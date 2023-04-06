using AutoMapper;
using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.ExternalConnectors;
using Microsoft.Identity.Web;
using System.Diagnostics;
using System.Net;

namespace bbApi.App.Services
{
    public class AzureRMRBACService
    {
        protected ArmClient client;
        protected readonly IMapper mapper;
        protected readonly IConfiguration configuration;

        public AzureRMRBACService(IMapper mapper, IConfiguration configuration)
        {
            this.mapper = mapper;
            this.configuration = configuration;
            client = PrepareAuthenticatedClient();
        }

        public SubscriptionResource GetSubscription(string subscriptionId) 
        {
            SubscriptionResource subscription = client.GetDefaultSubscription();
            return subscription;
        }

        public List<AuthorizationRoleDefinitionResource> GetSubscriptionRole(string subscriptionId)
        {
            SubscriptionResource subscription = client.GetDefaultSubscription();
            var roleDefitions = subscription.GetAuthorizationRoleDefinitions().ToList();
            return roleDefitions;
        }

        public ResourceGroupCollection GetResourceGroupsAsync(string subscriptionId)
        {
            SubscriptionResource subscription = client.GetDefaultSubscription();
            return subscription.GetResourceGroups();
        }

        private ArmClient PrepareAuthenticatedClient()
        {
            if (client == null)
            {
                // Create Microsoft Graph client.
                // https://learn.microsoft.com/en-us/graph/sdks/choose-authentication-providers?tabs=CS#client-credentials-provider
                try
                {
                    MicrosoftIdentityOptions aadoptions = new MicrosoftIdentityOptions();
                    configuration.GetSection("AzureAd").Bind(aadoptions);

                    var proxyAddress = "http://localhost:8888";

                    // Create a new System.Net.Http.HttpClientHandler with the proxy
                    var handler = new HttpClientHandler
                    {
                        // Create a new System.Net.WebProxy
                        // See WebProxy documentation for scenarios requiring
                        // authentication to the proxy
                        Proxy = new WebProxy(new Uri(proxyAddress))
                    };

                    TokenCredential credForTenant01 = new ClientSecretCredential(aadoptions.TenantId, aadoptions.ClientId, aadoptions.ClientSecret);
                    client = new ArmClient(credForTenant01);

                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Could not create a graph client {ex}");
                }
            }

            return client;
        }

    }
}

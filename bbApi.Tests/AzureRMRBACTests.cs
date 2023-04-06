using AutoMapper;
using bbApi.App.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bbApi.Tests
{
    [TestClass]
    public class AzureRMRBACTests
    {
        protected static IMapper _mapper;
        protected AzureRMRBACService azureRMRBACService;

   
        [TestMethod]
        public void Can_Get_Subscription()
        {
            var result = azureRMRBACService.GetSubscription("sdsd");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Can_Get_Subscription_Role_Definitions()
        {
            var result = azureRMRBACService.GetSubscriptionRole("sdsd");
            Assert.IsNotNull(result);
        }


        [TestInitialize]
        public void Initialize()
        {
            var builder = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json", true, true)
            .AddJsonFile($"appsettings.development.json", true, true)
            .AddUserSecrets<GroupAndRoleTests>()
            .AddEnvironmentVariables();
            var configuration = builder.Build();

            if (_mapper == null)
            {
                var mappingConfig = new MapperConfiguration(mc =>
                {
                    mc.AddProfile(new App.Models.AutoMapperProfile());
                });
                IMapper mapper = mappingConfig.CreateMapper();
                _mapper = mapper;
            }

            azureRMRBACService = new AzureRMRBACService(_mapper, configuration);

        }

    }
}

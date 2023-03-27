using AutoMapper;
using bbApi.App.Models;
using bbApi.App.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bbApi.Tests
{
    [TestClass]
    public class GraphTestBase
    {
        #region Init tests
        protected static IMapper _mapper;
        protected ITokenAcquisition _tokenAquisition;
        protected ADGraphGroupsService _adGraphGroupsService;
        protected ADGraphApplicationsService _adGraphAppsService;

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
                    mc.AddProfile(new AutoMapperProfile());
                });
                IMapper mapper = mappingConfig.CreateMapper();
                _mapper = mapper;
            }


            var _applicationOptions = new ConfidentialClientApplicationOptions();
            configuration.Bind("AzureAD", _applicationOptions);

            var app = ConfidentialClientApplicationBuilder
                .CreateWithApplicationOptions(_applicationOptions)
                    .Build();

            var authResult = app.AcquireTokenForClient(new string[] { "https://graph.microsoft.com/.default" }).ExecuteAsync().Result;

            var mock = new Mock<ITokenAcquisition>();
            mock.Setup(x => x.GetAuthenticationResultForAppAsync(It.IsAny<string>(), null, null)).Returns(
                Task.FromResult(authResult));
            _tokenAquisition = mock.Object;

            _adGraphGroupsService = new ADGraphGroupsService(_tokenAquisition, _mapper);
            _adGraphAppsService = new ADGraphApplicationsService(_tokenAquisition, _mapper);

        }
        #endregion


    }
}

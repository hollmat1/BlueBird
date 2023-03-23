using AutoMapper;
using bbApi.App.Models;
using bbApi.App.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Moq;

namespace bbApi.Tests
{
    [TestClass]
    public class MSGraphTests
    {
        const string TestGroupName = "aad-test1-bbapi";
        const string TestRoleName = "Directory Readers";
        const string TestGroupRoleAssignableName = "aad-test2-bbapi";
        private const string TestUserPrincipalName = "lab1@aswissbank.com";

        #region Init tests
        private static IMapper _mapper;
        ITokenAcquisition _tokenAquisition;
        ADGraphService _graphClient;

        [TestInitialize] public void Initialize()
        {
            var builder = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json", true, true)
            .AddJsonFile($"appsettings.development.json", true, true)
            .AddUserSecrets<MSGraphTests>()
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

            _graphClient = new ADGraphService(_tokenAquisition, _mapper);

        }
        #endregion

        [TestMethod]
        public void Can_Get_AD_Group_By_Name_From_MSGraph()
        {
            var result = _graphClient.GetGroupAsync(TestGroupName).Result;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Can_Get_AD_User_By_Name_From_MSGraph()
        {
            var result = _graphClient.GetUserAsync(TestUserPrincipalName).Result;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Can_Get_AAD_Role_By_Name_From_MSGraph()
        {
            var result = _graphClient.GetAADRoleAsync(TestRoleName).Result;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Can_Get_AAD_Roles()
        {
            var result = _graphClient.GetAADRoles().Result;
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count() > 1);
        }

        #region Create Tests

        [TestMethod]
        public void Can_Create_AAD_Group_With_IsAssignableToRole_Flag()
        {
            var testGroup = new NewGroupDTO
            {
                DisplayName = TestGroupRoleAssignableName,
                IsAssignableToRole = true,
                Description = "This is a test group.  to be deleted."
            };

            _graphClient.CreateGroupAsync(testGroup);

            var result = _graphClient.GetGroupAsync(testGroup.DisplayName).Result;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.SecurityEnabled);
            Assert.IsTrue(result.IsAssignableToRole);

            _graphClient.DeleteGroupAsync(result.Id);

        }

        [TestMethod]
        public void Can_Add_User_By_Name_To_AAD_Group()
        {
            var group = _graphClient.GetGroupAsync(TestGroupName).Result;

            Assert.IsNotNull(group, $"Group {TestGroupName} not found.");

            var user = _graphClient.GetUserAsync(TestUserPrincipalName).Result;

            Assert.IsNotNull(user, $"User {TestUserPrincipalName} not found.");

            _graphClient.AddGroupMembership(group.Id, user.Id);
        }

        [TestMethod]
        public void Can_Remove_User_By_Name_From_AAD_Group()
        {
            var group = _graphClient.GetGroupAsync(TestGroupName).Result;

            Assert.IsNotNull(group, $"Group {TestGroupName} not found.");

            var user = _graphClient.GetUserAsync(TestUserPrincipalName).Result;

            Assert.IsNotNull(user, $"User {TestUserPrincipalName} not found.");

            _graphClient.RemoveGroupMembershipAsync(group.Id, user.Id);
        }

        [TestMethod]
        public void Can_Add_Group_To_AAD_Role()
        {
            var group = _graphClient.GetGroupAsync(TestGroupRoleAssignableName).Result;

            if (group == null)
            {
                _graphClient.CreateGroupAsync(new NewGroupDTO
                {
                    DisplayName = TestGroupRoleAssignableName,
                    IsAssignableToRole = true,
                    Description = "This is a test group.  to be deleted."
                });

                Thread.Sleep(1000);

                group = _graphClient.GetGroupAsync(TestGroupRoleAssignableName).Result;
            }

            Assert.IsNotNull(group, $"Group {TestGroupRoleAssignableName} not found.");

            Assert.IsTrue(group.IsAssignableToRole, $"Group {TestGroupRoleAssignableName} is not flagged with IsAssignableToRole.  Group must be recreated.");

            var role = _graphClient.GetAADRoleAsync(TestRoleName).Result;

            Assert.IsNotNull(role, $"Role {TestRoleName} not found.");

            _graphClient.AssignGroupToRoleAsync(role.Id, group.Id);
        }

        [TestMethod]
        public void Can_Remove_Group_By_Name_From_AAD_Role()
        {
            var group = _graphClient.GetGroupAsync(TestGroupRoleAssignableName).Result;

            Assert.IsNotNull(group, $"Group {TestGroupRoleAssignableName} not found.");

            var role = _graphClient.GetAADRoleAsync(TestRoleName).Result;

            Assert.IsNotNull(role, $"Role {TestRoleName} not found.");

            _graphClient.RemoveAADRoleMembershipAsync(role.Id, group.Id);
        }

        [TestMethod]
        public void Can_Get_AAD_Role_Members()
        {
            var role = _graphClient.GetAADRoleAsync(TestRoleName).Result;

            var roleMembers = _graphClient.GetAADRoleMembersAsync(role.Id).Result;

            Assert.IsNotNull(roleMembers);
        }



        #endregion
    }
}

using AutoMapper;
using bbApi.App.Models;
using bbApi.App.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Moq;
using System.Data;

namespace bbApi.Tests
{
    [TestClass]
    public class GroupAndRoleTests : GraphTestBase
    {
        const string TestGroupNameM365 = "test-m365-group1";
        const string TestGroupName = "aad-test1-bbapi";
        const string TestRoleName = "Directory Readers";
        const string TestGroupRoleAssignableName = "aad-test2-bbapi";
        private const string TestUserPrincipalName = "lab1@aswissbank.com";
        private const string AdminUnitName = "AADTESTS";

        [TestMethod]
        public void Can_Get_AD_Group_By_Name_From_MSGraph()
        {
            var result = _adGraphGroupsService.GetGroupAsync(TestGroupName).Result;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Can_Get_AD_User_By_Name_From_MSGraph()
        {
            var result = _adGraphGroupsService.GetUserAsync(TestUserPrincipalName).Result;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Can_Get_AD_User_Paged_From_MSGraph()
        {
            var result = _adGraphGroupsService.GetUsersAsync(100).Result;
            Assert.IsNotNull(result.CurrentPage != null);
            Assert.IsTrue(result.CurrentPage?.Count().Equals(result.Top));
            while(result.SkipToken != null)
            {
                result = _adGraphGroupsService.GetUsersAsync(100, result.SkipToken).Result;
            }
        }

        [TestMethod]
        public void Can_Get_AD_Groups_Paged_From_MSGraph()
        {
            var results = _adGraphGroupsService.GetGroupsAsync(100).Result;
            Assert.IsNotNull(results.CurrentPage != null);
            Assert.IsTrue(results.CurrentPage?.Count().Equals(results.Top));
            while (results.SkipToken != null)
            {
                results = _adGraphGroupsService.GetGroupsAsync(100, results.SkipToken).Result;

                foreach (var result in results.CurrentPage)
                {
                    var members = _adGraphGroupsService.GetGroupMembersAsync(result.Id).Result;
                    if (members.CurrentPage?.Count() > 0)
                    {
                        Console.WriteLine("sdsadas");
                    }
                }



            }
        }

        [TestMethod]
        public void Can_Get_AAD_Role_By_Name_From_MSGraph()
        {
            var result = _adGraphGroupsService.GetAADRoleAsync(TestRoleName).Result;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Can_Get_AAD_Roles()
        {
            var result = _adGraphGroupsService.GetAADRoles().Result;
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count() > 1);
        }

        [TestMethod]
        public void Can_Check_Is_User_Member_Of_Group()
        {
            var user = _adGraphGroupsService.GetUserAsync(TestUserPrincipalName).Result;
            Assert.IsNotNull(user);

            var group = _adGraphGroupsService.GetGroupAsync(TestGroupName).Result;
            Assert.IsNotNull(group);

            var reslt = _adGraphGroupsService.IsGroupMemberAsync(group.Id, user.Id).Result;
            Assert.IsFalse(reslt);

            _adGraphGroupsService.AddGroupMemberAsync(group.Id, user.Id).GetAwaiter().GetResult();

            reslt = _adGraphGroupsService.IsGroupMemberAsync(group.Id, user.Id).Result;
            Assert.IsTrue(reslt);

            _adGraphGroupsService.RemoveGroupMemberAsync(group.Id, user.Id).GetAwaiter().GetResult();

            reslt = _adGraphGroupsService.IsGroupMemberAsync(group.Id, user.Id).Result;
            Assert.IsFalse(reslt);

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

            _adGraphGroupsService.CreateGroupAsync(testGroup).GetAwaiter().GetResult();

            var result = _adGraphGroupsService.GetGroupAsync(testGroup.DisplayName).Result;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.SecurityEnabled);
            Assert.IsTrue(result.IsAssignableToRole);

            _adGraphGroupsService.DeleteGroupAsync(result.Id).GetAwaiter().GetResult();

        }

        [TestMethod]
        public void Can_Create_M365_Group()
        {
            var testGroup = new NewGroupDTO
            {
                DisplayName = TestGroupNameM365,
                Description = "This is a test group.  to be deleted."
            };

            var result = _adGraphGroupsService.CreateM365GroupAsync(testGroup).Result;

            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsAssignableToRole);
            Assert.IsTrue(result.MailEnabled);

            _adGraphGroupsService.DeleteGroupAsync(result.Id).GetAwaiter().GetResult();

        }

        [TestMethod]
        public void Can_Add_Existing_Group_to_AdminUnit()
        {
            var group = _adGraphGroupsService.GetGroupAsync(TestGroupName).Result;

            if (group == null)
            {
                var result = _adGraphGroupsService.CreateGroupAsync(new NewGroupDTO
                {
                    DisplayName = TestGroupName,
                    IsAssignableToRole = true,
                    Description = "This is a test group.  to be deleted."
                }).Result;

                group = _adGraphGroupsService.GetGroupAsync(TestGroupName).Result;
            }

            Assert.IsNotNull(group, $"Group {TestGroupName} not found.");

            var adminUnit = _adGraphGroupsService.GetAADAdminUnit(AdminUnitName).Result;

            Assert.IsNotNull(adminUnit, $"AdmînUit {AdminUnitName} not found.");

            _adGraphGroupsService.AddAdminUnitMemberAsync(adminUnit.Id, group.Id).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void Can_Remove_Existing_Group_From_AdminUnit()
        {
            var adminUnit = _adGraphGroupsService.GetAADAdminUnit(AdminUnitName).Result;

            Assert.IsNotNull(adminUnit, $"AdmînUit {AdminUnitName} not found.");

            var group = _adGraphGroupsService.GetGroupAsync(TestGroupName).Result;

            if (group == null)
            {
                var result = _adGraphGroupsService.CreateGroupAsync(new NewGroupDTO
                {
                    DisplayName = TestGroupName,
                    IsAssignableToRole = true,
                    Description = "This is a test group.  to be deleted."
                }, adminUnit.Id).Result;

                group = _adGraphGroupsService.GetGroupAsync(TestGroupName).Result;
            }

            Assert.IsNotNull(group, $"Group {TestGroupName} not found.");


            _adGraphGroupsService.RemoveAdminUnitMemberAsync(adminUnit.Id, group.Id).GetAwaiter().GetResult();

        }


        [TestMethod]
        public void Can_Create_AAD_Group_InAdminUnit_With_IsAssignableToRole_Flag()
        {
            var adminUnit = _adGraphGroupsService.GetAADAdminUnit(AdminUnitName).Result;

            Assert.IsNotNull(adminUnit, $"Admin unit {AdminUnitName} was not found.");

            var testGroup = new NewGroupDTO
            {
                DisplayName = TestGroupRoleAssignableName,
                IsAssignableToRole = true,
                Description = "This is a test group.  to be deleted."
            };

            var result = _adGraphGroupsService.CreateGroupAsync(testGroup, adminUnit.Id).Result;
            
            Assert.IsNotNull(result);
            Assert.IsTrue(result.SecurityEnabled);
            Assert.IsTrue(result.IsAssignableToRole);

            _adGraphGroupsService.DeleteGroupAsync(result.Id).GetAwaiter().GetResult();

        }


        [TestMethod]
        public void Can_Create_M365_Group_InAdminUnit()
        {
            var adminUnit = _adGraphGroupsService.GetAADAdminUnit(AdminUnitName).Result;

            Assert.IsNotNull(adminUnit, $"Admin unit {AdminUnitName} was not found.");

            var testGroup = new NewGroupDTO
            {
                DisplayName = TestGroupNameM365,
                Description = "This is a test group.  to be deleted."
            };

            var result = _adGraphGroupsService.CreateM365GroupAsync(testGroup, adminUnit.Id).Result;

            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsAssignableToRole);
            Assert.IsTrue(result.MailEnabled);

            _adGraphGroupsService.DeleteGroupAsync(result.Id).GetAwaiter().GetResult();

        }


        [TestMethod]
        public void Can_Add_User_By_Name_To_AAD_Group()
        {
            var group = _adGraphGroupsService.GetGroupAsync(TestGroupName).Result;

            Assert.IsNotNull(group, $"Group {TestGroupName} not found.");

            var user = _adGraphGroupsService.GetUserAsync(TestUserPrincipalName).Result;

            Assert.IsNotNull(user, $"User {TestUserPrincipalName} not found.");

            _adGraphGroupsService.AddGroupMemberAsync(group.Id, user.Id).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void Can_Remove_User_By_Name_From_AAD_Group()
        {
            var group = _adGraphGroupsService.GetGroupAsync(TestGroupName).Result;

            Assert.IsNotNull(group, $"Group {TestGroupName} not found.");

            var user = _adGraphGroupsService.GetUserAsync(TestUserPrincipalName).Result;

            Assert.IsNotNull(user, $"User {TestUserPrincipalName} not found.");

            _adGraphGroupsService.RemoveGroupMemberAsync(group.Id, user.Id).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void Can_Add_Group_To_AAD_Role()
        {
            var group = _adGraphGroupsService.GetGroupAsync(TestGroupRoleAssignableName).Result;

            if (group == null)
            {
                var result = _adGraphGroupsService.CreateGroupAsync(new NewGroupDTO
                {
                    DisplayName = TestGroupRoleAssignableName,
                    IsAssignableToRole = true,
                    Description = "This is a test group.  to be deleted."
                }).Result;

                group = _adGraphGroupsService.GetGroupAsync(TestGroupRoleAssignableName).Result;
            }

            Assert.IsNotNull(group, $"Group {TestGroupRoleAssignableName} not found.");

            Assert.IsTrue(group.IsAssignableToRole, $"Group {TestGroupRoleAssignableName} is not flagged with IsAssignableToRole.  Group must be recreated.");

            var role = _adGraphGroupsService.GetAADRoleAsync(TestRoleName).Result;

            Assert.IsNotNull(role, $"Role {TestRoleName} not found.");

            _adGraphGroupsService.AssignGroupToRoleAsync(role.Id, group.Id).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void Can_Remove_Group_By_Name_From_AAD_Role()
        {
            var group = _adGraphGroupsService.GetGroupAsync(TestGroupRoleAssignableName).Result;

            Assert.IsNotNull(group, $"Group {TestGroupRoleAssignableName} not found.");

            var role = _adGraphGroupsService.GetAADRoleAsync(TestRoleName).Result;

            Assert.IsNotNull(role, $"Role {TestRoleName} not found.");

            _adGraphGroupsService.RemoveAADRoleMemberAsync(role.Id, group.Id).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void Can_Get_AAD_Role_Members()
        {
            var role = _adGraphGroupsService.GetAADRoleAsync(TestRoleName).Result;

            var roleMembers = _adGraphGroupsService.GetAADRoleMembersAsync(role.Id).Result;

            Assert.IsNotNull(roleMembers);
        }

        [TestMethod]
        public void Can_Get_AAD_Admin_Unit()
        {
            var adminUnit = _adGraphGroupsService.GetAADAdminUnit(AdminUnitName).Result;

            Assert.IsNotNull(adminUnit);

        }

        #endregion
    }
}

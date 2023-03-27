using bbApi.App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bbApi.Tests
{

    [TestClass]
    public class ApplicationTests : GraphTestBase
    {
        const string TestAppName = "bbApi-Tests-App1";

        [TestMethod]
        public void Can_Create_Application_Registration_By_Name()
        {
            var newApp = new NewApplicationDTO 
            {
                DisplayName = TestAppName 
            };

            var result = _adGraphAppsService.CreateAppRegistrationAsync(newApp).Result;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Can_Get_Application_Registration_By_Name()
        {
            var result = _adGraphAppsService.GetAppRegistrationAsync(TestAppName).Result;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Can_Create_Application_Role()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void Can_Assign_Member_To_Application_Role()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void Can_Remove_Member_From_Application_Role()
        {
            throw new NotImplementedException();
        }


        [TestMethod]
        public void Can_Assign_Custom_Policy_To_Application()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void Can_Remove_Custom_Policy_From_Application()
        {
            throw new NotImplementedException();
        }


        [TestMethod]
        public void Can_Delete_Application_Role()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void Can_Delete_Application_Registration()
        {
            throw new NotImplementedException();
        }

    }
}

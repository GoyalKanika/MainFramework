using MainFramework.Driver;
using MainFramework.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainFramework.TestBase
{
    public class TestSetup
    {
        [OneTimeSetUp]
        public void Setup()
        {
            ReportHelper.InitializeReport();
        }

        [SetUp]
        public void Init()
        {
            try
            {
                var driver = DriverManager.GetDriver();
                DriverManager.GetDriver().Navigate().GoToUrl("https://goyakanika-trials714.orangehrmlive.com/auth/login");
                DriverManager.GetDriver().Manage().Window.Maximize();
            }
            catch (Exception ex)
            {
                ReportHelper.LogFail($"Driver Initilization failed {ex.Message}");
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
            {
                ReportHelper.LogFail($"Test failed: {TestContext.CurrentContext.Result.Message}");
            }
            else
            {
                ReportHelper.LogPass("Test passed");
            }
            DriverManager.QuitDriver();
            // _driver.Dispose();
        }
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            ReportHelper.FlushReport();
        }
    }
}

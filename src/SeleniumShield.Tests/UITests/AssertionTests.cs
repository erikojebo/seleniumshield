using NUnit.Framework;
using OpenQA.Selenium;

namespace SeleniumShield.Tests.UITests
{
    [TestFixture]
    public class AssertionTests : UITestBase
    {
        [SetUp]
        public void SetUp()
        {
            CreateDriver();
        }

        [Test]
        public void Can_use_basic_asserts()
        {
            Driver.GoToRelativeUrl("/");

            Driver.WriteTo("#username", "kalle");
            Driver.WriteTo("#password", "p@ssw0rd");
            Driver.Submit(By.Id("value_submitter"));
        }

        [TearDown]
        public void TearDown()
        {
            CleanUp();
        }
    }
}
using Helpers.Drivers.Mobile;
using OpenQA.Selenium;
using NUnit.Framework;


namespace Mobile_tests
{
    public class LoginTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestLogin()
        {
            var driver = AndroidDriverBuilder.GetDriver();

            var userField = driver.FindElement(By.Id("com.companyname.app.eds:id/EntryUsername"));
            userField.SendKeys("admin");

            var passField = driver.FindElement(By.Id("com.companyname.app.eds:id/EntryPassword"));
            passField.SendKeys("admin");

            var loginButton = driver.FindElement(By.Id("com.companyname.app.eds:id/ButtonLogin"));
            loginButton.Click();

            //Assert.That(driver.PageSource.Contains());
            driver.Quit();

        }
    }
}

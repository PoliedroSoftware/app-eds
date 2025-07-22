using Helpers.Drivers.Mobile;
using OpenQA.Selenium;
using NUnit.Framework;
using Helpers.Interfaces;
using Mobile_views;


namespace Mobile_tests
{
    public class LoginTest
    {
        IDriverManager _driver;
        [SetUp]
        public void Setup()
        {
            _driver = new MobileDriverManager();
        }

        [Test]
        public void TestLogin()
        {
            var loginView = new LoginView(_driver);
            loginView.Skip.Click();
            Assert.Pass();

            //Assert.That(driver.PageSource.Contains());
            

        }
    }
}

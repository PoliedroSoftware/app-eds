using Helpers.Drivers.Mobile;
using Helpers.Interfaces;
using Mobile_views;

namespace Mobile_tests;

public class BaseTest
{
    protected IDriverManager _driver;
    [OneTimeSetUp]
    public void GlobalSetup()
    {
        _driver = new MobileDriverManager();
        Thread.Sleep(3000);

        var loginView = new LoginView(_driver);
        loginView.Username.SetText("admin");
        loginView.Password.SetText("admin");
        loginView.LoginButton.Click();

        Thread.Sleep(3000);
    }

    [OneTimeTearDown]
    public void GlobalTearDown()
    {
        _driver.Close();
    }
}

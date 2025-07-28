using Helpers.Drivers.Mobile;
using Mobile_views;

namespace Mobile_tests;

[TestFixture, Order(2)]
public class AdminBussinessTest : BaseTest
{
    [Test]
    [Category ("Bussines")]
    public void TestBussinessClick()
    {
        var adminFrameClick = new MainMenuView(_driver);
        adminFrameClick.AdminFrameClick.Element.Click();

        var bussinesButton = new BussinesView(_driver);
        bussinesButton.BussinesButton.Click();

        bussinesButton.BussinesTextValue.SetText("PruebaTestAuto");
        bussinesButton.BussinesSendenDataButton.Click();
        Thread.Sleep(4000);
        ((MobileDriverManager)_driver).OkAceptButton().Click();
        Thread.Sleep(2000);
        ((MobileDriverManager)_driver).NavigateUpButton().Click();

        Assert.Pass("Bussines Test Success !!");
    }
}

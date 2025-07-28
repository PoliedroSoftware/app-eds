using Helpers.Drivers.Mobile;
using Helpers.Enums;
using Mobile_views;

namespace Mobile_tests;

[TestFixture, Order(1)]
public class AdminCourtTest : BaseTest
{
    [Test]
    [Category ("Court")]
    public void TestAdminCourtClick()
    {
        var adminFrameClick = new MainMenuView(_driver);
        adminFrameClick.AdminFrameClick.Element.Click();

        Thread.Sleep(3000);

        var courtButton = new CourtView(_driver);
        var dateSelect = new CourtView( _driver);
        
        courtButton.CourtButton.Click();
        Thread.Sleep(5000);
        courtButton.SelectBussines.Element.Click();
        Thread.Sleep(3000);
        courtButton.ClickBussines.Element.Click();
        Thread.Sleep(2000);
        courtButton.SelectEds.Element.Click();
        Thread.Sleep(2000);
        courtButton.ClickEds.Element.Click();
        Thread.Sleep(2000);
        courtButton.SelectIslander.Element.Click();
        Thread.Sleep(2000);
        courtButton.ClickIslander.Element.Click();
        Thread.Sleep(3000);
        ((MobileDriverManager)_driver).ScrollToEnd();
        courtButton.NewSale.Click();
        Thread.Sleep(2000);
        courtButton.SelectHose.Element.Click();
        Thread.Sleep(2000);
        courtButton.ClickHose.Element.Click();
        Thread.Sleep(2000);
        courtButton.ClickAmmountAcumulate.Element.Click();
        Thread.Sleep(2000);
        courtButton.ChangeTextToInt(1);
        Thread.Sleep(2000);
        ((MobileDriverManager) _driver).PressEnter();
        Thread.Sleep(2000);
        courtButton.AddButton.Click();
        Thread.Sleep(2000);
        courtButton.AddCollectionButton.Click();
        Thread.Sleep(2000);
        courtButton.TypeCollectionClick.Element.Click();
        Thread.Sleep(2000);
        courtButton.CashSelect.Element.Click();
        Thread.Sleep(2000);
        courtButton.ValueCollection.SetNumber(1);
        Thread.Sleep(2000);
        ((MobileDriverManager)_driver).PressEnter();
        Thread.Sleep(2000);
        courtButton.ValueDescription.SetText("PruebaTestAuto");
        Thread.Sleep(2000);
        courtButton.SendCollectionButton.Click();
        Thread.Sleep(2000);
        ((MobileDriverManager)_driver).ScrollToEnd();
        Thread.Sleep(2000);
        courtButton.NewEgressButton.Click();
        Thread.Sleep(2000);
        courtButton.ExpendiSelectClick.Element.Click();
        Thread.Sleep(2000);
        courtButton.FleteSelectClick.Element.Click();
        Thread.Sleep(2000);
        courtButton.AmmountValue.SetNumber(1);
        ((MobileDriverManager)_driver).PressEnter();
        courtButton.DescriptionValue.SetText("PruebaTestAuto");
        Thread.Sleep(2000);
        courtButton.AddExpenseButton.Click();
        ((MobileDriverManager)_driver).ScrollToEnd();
        courtButton.SendDataButton.Click();
        Thread.Sleep(3000);
        ((MobileDriverManager)_driver).OkAceptButton().Click();
        courtButton.OpenListButton.Click();
        Thread.Sleep(5000);
        ((MobileDriverManager)_driver).NavigateUpButton().Click();
        Thread.Sleep(2000);
        ((MobileDriverManager)_driver).NavigateUpButton().Click();

        Assert.Pass("Court testing Success !!");
    }
}


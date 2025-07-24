using Mobile_views;

namespace Mobile_tests;

public class AdminCourtTest : BaseTest
{
    [Test]
    public void TestAdminFrameClick()
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
        Thread.Sleep(2000);
        Assert.Pass();
    }
}

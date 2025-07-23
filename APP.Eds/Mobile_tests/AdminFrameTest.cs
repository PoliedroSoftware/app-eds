using Mobile_views;

namespace Mobile_tests;

public class AdminFrameTest : BaseTest
{
    [Test]
    public void TestAdminFrameClick()
    {
        var adminFrameClick = new MainMenuView(_driver);
        adminFrameClick.AdminFrame.Element.Click();

        Thread.Sleep(3000);
        Assert.Pass();
    }
}

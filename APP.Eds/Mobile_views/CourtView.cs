using Helpers.Enums;
using Helpers.Interfaces;
using Helpers.UIElements.Mobile;

namespace Mobile_views
{
    public class CourtView : MainMenuView
    {
        public CourtView(IDriverManager driver) : base(driver) { }
        public IButton CourtButton => (IButton)_driver.GetElement(ElementType.Button, FindsBy.XPath,
            "//android.widget.Button[@resource-id=\"com.companyname.app.eds:id/ButtonMenuItem\" and @text=\"Corte\"]");

        public TextField SelectBussines => (TextField)_driver.GetElement(ElementType.TextField, FindsBy.Id,
            "com.companyname.app.eds:id/BusinessPicker");

        public TextField ClickBussines => (TextField)_driver.GetElement(ElementType.TextField, FindsBy.Id,
            "android:id/text1");

        public TextField SelectEds => (TextField)_driver.GetElement(ElementType.TextField, FindsBy.Id,
            "com.companyname.app.eds:id/EdsPicker");

        public TextField ClickEds => (TextField)_driver.GetElement(ElementType.TextField, FindsBy.Id,
            "android:id/text1");

        public TextField SelectIslander => (TextField)_driver.GetElement(ElementType.TextField, FindsBy.Id,
            "com.companyname.app.eds:id/IslanderPicker");

        public TextField ClickIslander => (TextField)_driver.GetElement(ElementType.TextField, FindsBy.Id,
            "android:id/text1");
    }
}

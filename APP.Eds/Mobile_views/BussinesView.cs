using Helpers.Enums;
using Helpers.Interfaces;
using Helpers.UIElements.Mobile;
using NUnit.Framework;

namespace Mobile_views
{
    public class BussinesView : MainMenuView
    {
        public BussinesView(IDriverManager driver) : base(driver) { }

        public IButton BussinesButton => (IButton)_driver.GetElement(ElementType.Button, FindsBy.XPath,
            "//android.widget.Button[@resource-id=\"com.companyname.app.eds:id/ButtonMenuItem\" and @text=\"Negocio\"]");

        public TextField BussinesTextValue => (TextField)_driver.GetElement(ElementType.TextField, FindsBy.Id,
            "com.companyname.app.eds:id/EntryBusinessName");

        public IButton BussinesSendenDataButton => (IButton)_driver.GetElement(ElementType.Button, FindsBy.Id,
            "com.companyname.app.eds:id/ButtonSendBusinessData");



    }
}

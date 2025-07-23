using Helpers.Enums;
using Helpers.Interfaces;
using Helpers.UIElements.Mobile;

namespace Mobile_views
{
    public class LoginView : BaseView
    {
        public LoginView(IDriverManager driver) : base(driver) { }

        public IButton LoginButton => (IButton)_driver.GetElement(ElementType.Button, FindsBy.Id, "com.companyname.app.eds:id/ButtonLogin");
        public TextField Username => (TextField)_driver.GetElement(ElementType.TextField, FindsBy.Id, "com.companyname.app.eds:id/EntryUsername");
        public TextField Password => (TextField)_driver.GetElement(ElementType.TextField, FindsBy.Id, "com.companyname.app.eds:id/EntryPassword");
    }
}

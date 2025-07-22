using Helpers.Enums;
using Helpers.Interfaces;

namespace Mobile_views
{
    public class LoginView : BaseView
    {
        public LoginView(IDriverManager driver) : base(driver) { }

        public IButton Skip => (IButton)_driver.GetElement(ElementType.Button, FindsBy.AcessibilityId, "com.companyname.app.eds:id/ButtonLogin");
    }
}

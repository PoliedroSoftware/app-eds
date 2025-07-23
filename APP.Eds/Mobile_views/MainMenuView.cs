using Helpers.Enums;
using Helpers.Interfaces;
using Helpers.UIElements.Mobile;

namespace Mobile_views
{
    public class MainMenuView : BaseView
    {
        public MainMenuView(IDriverManager driver) : base(driver) { }

        public MobileElement AdminFrame => _driver.GetElement(ElementType.TextField, FindsBy.XPath,
            "//android.widget.TextView[@resource-id=\"com.companyname.app.eds:id/LabelCategoryTitle\" and @text= 'Administración']/parent::*");
    }
}

using OpenQA.Selenium.Appium;

namespace Helpers.UIElements.Mobile
{
    public class MobileElement
    {
        protected AppiumElement _element;
        public AppiumElement Element { get { return _element; } }

        protected MobileElement(AppiumElement element)
        {
            _element = element;
        }
    }
}

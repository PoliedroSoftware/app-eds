using Helpers.Interfaces;
using OpenQA.Selenium.Appium;

namespace Helpers.UIElements.Mobile
{
    public class TextField : MobileElement, ITextField
    {
        public TextField(AppiumElement element) : base(element) { }
        void SetText(string text)
        {
            _element.SendKeys(text);
        }
    }
}

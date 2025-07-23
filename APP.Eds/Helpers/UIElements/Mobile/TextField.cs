using Helpers.Interfaces;
using OpenQA.Selenium.Appium;

namespace Helpers.UIElements.Mobile
{
    public class TextField : MobileElement, ITextField
    {
        public TextField(AppiumElement element) : base(element) { }

        string ITextField.TextField => throw new NotImplementedException();

        public void SetText(string text)
        {
            _element.SendKeys(text);
        }
    }
}

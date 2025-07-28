using OpenQA.Selenium.Appium;
using Helpers.Interfaces;
using OpenQA.Selenium;
using System.Xml.Linq;

namespace Helpers.UIElements.Mobile
{
    internal class Button : Text, IButton
    {
        public Button(AppiumElement element) : base(element) { }

        public void Click()
        {
            if (_element.Displayed && _element.Enabled)
            {
                _element.Click();
            }
            else
            {
                throw new ElementNotInteractableException("Could not click on button.");
            }
        }
    }
}

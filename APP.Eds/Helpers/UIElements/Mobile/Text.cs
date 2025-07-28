using Helpers.Interfaces;
using OpenQA.Selenium.Appium;

namespace Helpers.UIElements.Mobile
{
    public class Text : MobileElement, IText
    {
        public Text(AppiumElement element) : base(element) { }

        string IText.Text => _element.Text;
    }   
}

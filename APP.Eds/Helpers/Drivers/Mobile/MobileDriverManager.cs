using Helpers.Enums;
using Helpers.Interfaces;
using Helpers.UIElements.Mobile;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Support.UI;
using HelpersButton = Helpers.UIElements.Mobile.Button;

namespace Helpers.Drivers.Mobile
{
    public class MobileDriverManager : IDriverManager
    {
        private AppiumDriver _driver;

        public MobileDriverManager()
        {
            switch (Configuration.PlatformName)
            {
                case PlatformName.Android:
                    _driver = AndroidDriverBuilder.GetDriver();
                    break;

                case PlatformName.ios:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        public void Close()
        {
            _driver.Quit();
        }

        public MobileElement GetElement(ElementType elementType, FindsBy findsby, string locator)
        {
            AppiumElement appiumElement = FindElement(findsby, locator);

            switch (elementType)
            {
                case ElementType.Button:
                    return new HelpersButton(appiumElement);
                case ElementType.TextField:
                    return new TextField(appiumElement);
                case ElementType.Text:
                    return new Text(appiumElement);
                default:
                    throw new NotSupportedException($"Element type \"{elementType}\" is not supported.");
            }

        }

        public AppiumElement FindElement(FindsBy findsBy, string locator)
        {
            IWait<IWebDriver> wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));

            switch (findsBy)
            {
                case FindsBy.XPath:
                    return wait.Until(driver => _driver.FindElement(By.XPath(locator)));
                case FindsBy.Id:
                    return wait.Until(driver => _driver.FindElement(By.Id(locator)));
                case FindsBy.AcessibilityId:
                    return wait.Until(driver => _driver.FindElement(MobileBy.AccessibilityId(locator)));
                default:
                    throw new NotSupportedException($"Locator type \"{findsBy}\" not supported.");
            }
        }

        public byte[] TakeSchreeshot()
        {
            return _driver.GetScreenshot().AsByteArray;
        }

        public byte[] TakeScreenshot()
        {
            return _driver.GetScreenshot().AsByteArray;
        }

        public void ScrollToEnd(int maxSwipes = 10)
        {
            _driver.FindElement(MobileBy.AndroidUIAutomator(
                $"new UiScrollable(new UiSelector().scrollable(true)).scrollToEnd({maxSwipes})"));
        }

        public void PressEnter()
        {
            if (_driver is AndroidDriver androidDriver)
            {
                androidDriver.PressKeyCode(OpenQA.Selenium.Appium.Android.Enums.AndroidKeyCode.Enter);
            }
        }

        public IButton NavigateUpButton()
        {
            return (IButton)GetElement(ElementType.Button, FindsBy.AcessibilityId,
            "Navigate up");
        }

        public IButton OkAceptButton()
        {
            return (IButton)GetElement(ElementType.Button, FindsBy.Id,
            "android:id/button2");
        }
    }
}

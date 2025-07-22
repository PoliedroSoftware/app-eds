using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;

namespace Helpers.Drivers.Mobile
{
    public static class AndroidDriverBuilder
    {
        public static AppiumDriver GetDriver()
        {
            var options = new AppiumOptions();

            options.PlatformName = Configuration.PlatfromName;
            options.AutomationName = "UiAutomator2";
            options.AddAdditionalAppiumOption("appium:appPackage", Configuration.AppPackage);
            options.AddAdditionalAppiumOption("appium:appActivity", Configuration.AppActivity);
            options.AddAdditionalAppiumOption("appium:adbExexTimeout", 600000);
            
            return new AndroidDriver(new Uri(Configuration.AppiumServer), options);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;

namespace Helpers.Drivers.Android
{
    public static class AndroidDriverBuilder
    {
        public static AppiumDriver<AppiumElement> GetDriver()
        {
            var driverOptions = new AppiumOptions();
            driverOptions.AddAdditionalCapability("platformName", "Android");

            return new AndroidDriver<AppiumElement>(new Uri("http://127.0.0.1:4723"), driverOptions);
        }
    }
    
}

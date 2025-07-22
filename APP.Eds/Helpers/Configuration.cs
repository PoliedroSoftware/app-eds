using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers
{
    public static class Configuration
    {
        public static string PlatfromName = ConfigurationHelper.GetString("platformName");
        public static string AppiumServer = ConfigurationHelper.GetString("appiumServer");
        public static string AppPackage = ConfigurationHelper.GetString("appPackage");
        public static string AppActivity = ConfigurationHelper.GetString("appActivity");
    }
}

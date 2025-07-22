using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helpers.Enums;

namespace Helpers
{
    public static class Configuration
    {
        public static  PlatformName PlatformName
        {
            get
            {
                string configuration = ConfigurationHelper.GetString("platformName");
                bool supportedPlatform = Enum.TryParse(configuration, out PlatformName platformName);

                if (supportedPlatform)
                {
                    return platformName;
                }

                throw new ArgumentException("PlatformName not supported");
            }
        }
        public static string AppiumServer = ConfigurationHelper.GetString("appiumServer");
        public static string AppPackage = ConfigurationHelper.GetString("appPackage");
        public static string AppActivity = ConfigurationHelper.GetString("appActivity");
    }
}

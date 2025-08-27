namespace APP.Eds.Helpers
{
    /// <summary>
    /// Helper class to provide cross-platform compatible icons
    /// Uses Unicode symbols that work consistently across all platforms
    /// </summary>
    public static class IconHelper
    {
        // Primary icons using geometric shapes and arrows
        public static class Fuel
        {
            public const string Station = "E";
            public const string Tank = "T";
            public const string Pump = "P";
            public const string Hose = "H";
        }

        public static class Navigation
        {
            public const string Next = ">";
            public const string Previous = "<";
            public const string Up = "^";
            public const string Down = "v";
            public const string Left = "<";
            public const string Right = ">";
            public const string Forward = "->";
            public const string Back = "<-";
        }

        public static class Status
        {
            public const string Success = "OK";
            public const string Error = "ERR";
            public const string Warning = "!";
            public const string Info = "i";
            public const string Loading = "...";
            public const string Complete = "DONE";
            public const string Pending = "WAIT";
        }

        public static class Actions
        {
            public const string Add = "+";
            public const string Remove = "-";
            public const string Edit = "EDIT";
            public const string Delete = "DEL";
            public const string Save = "SAVE";
            public const string Cancel = "CANCEL";
            public const string Refresh = "REFRESH";
            public const string View = "VIEW";
            public const string Search = "SEARCH";
        }

        public static class Business
        {
            public const string Building = "B";
            public const string Location = "L";
            public const string Person = "P";
            public const string Group = "G";
            public const string Document = "D";
            public const string Money = "$";
        }

        // Alternative text-based icons for maximum compatibility
        public static class TextIcons
        {
            public const string FuelStation = "[EDS]";
            public const string Tank = "[TNK]";
            public const string Pump = "[PMP]";
            public const string Success = "[OK]";
            public const string Error = "[ERR]";
            public const string Warning = "[!]";
            public const string Info = "[i]";
            public const string Loading = "[...]";
        }

        /// <summary>
        /// Gets the most compatible icon for the current platform
        /// </summary>
        /// <param name="iconType">Type of icon needed</param>
        /// <param name="fallbackToText">Whether to fallback to text-based icons if symbols don't work</param>
        /// <returns>Icon string</returns>
        public static string GetIcon(IconType iconType, bool fallbackToText = false)
        {
            if (fallbackToText)
            {
                return iconType switch
                {
                    IconType.FuelStation => TextIcons.FuelStation,
                    IconType.Tank => TextIcons.Tank,
                    IconType.Success => TextIcons.Success,
                    IconType.Error => TextIcons.Error,
                    IconType.Warning => TextIcons.Warning,
                    IconType.Info => TextIcons.Info,
                    IconType.Loading => TextIcons.Loading,
                    _ => "[?]"
                };
            }

            return iconType switch
            {
                IconType.FuelStation => Fuel.Station,
                IconType.Tank => Fuel.Tank,
                IconType.Success => Status.Success,
                IconType.Error => Status.Error,
                IconType.Warning => Status.Warning,
                IconType.Info => Status.Info,
                IconType.Loading => Status.Loading,
                IconType.Next => Navigation.Next,
                IconType.Previous => Navigation.Previous,
                IconType.Add => Actions.Add,
                IconType.Remove => Actions.Remove,
                IconType.Refresh => Actions.Refresh,
                IconType.View => Actions.View,
                _ => "?"
            };
        }

        /// <summary>
        /// Gets font family for better icon rendering
        /// </summary>
        /// <returns>Platform-specific font family for symbols</returns>
        public static string GetSymbolFontFamily()
        {
            // Use runtime detection instead of switch expression with DevicePlatform
            var platform = DeviceInfo.Platform;
            
            if (platform == DevicePlatform.Android)
                return "monospace";
            else if (platform == DevicePlatform.iOS || platform == DevicePlatform.MacCatalyst)
                return "Menlo";
            else if (platform == DevicePlatform.WinUI)
                return "Segoe UI Symbol";
            else
                return "Courier New";
        }

        /// <summary>
        /// Gets the recommended font size for icons based on context
        /// </summary>
        /// <param name="context">Where the icon will be used</param>
        /// <returns>Font size</returns>
        public static double GetIconFontSize(IconContext context)
        {
            return context switch
            {
                IconContext.Button => 16,
                IconContext.Header => 32,
                IconContext.Card => 20,
                IconContext.List => 14,
                IconContext.Navigation => 24,
                _ => 16
            };
        }
    }

    public enum IconType
    {
        FuelStation,
        Tank,
        Success,
        Error,
        Warning,
        Info,
        Loading,
        Next,
        Previous,
        Add,
        Remove,
        Refresh,
        View
    }

    public enum IconContext
    {
        Button,
        Header,
        Card,
        List,
        Navigation
    }
}
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
            public const string Station = "?";
            public const string Tank = "???";
            public const string Pump = "?";
            public const string Hose = "??";
            public const string Dispenser = "?";
            public const string FuelNozzle = "?";
        }

        public static class Navigation
        {
            public const string Next = "??";
            public const string Previous = "??";
            public const string Up = "??";
            public const string Down = "??";
            public const string Left = "??";
            public const string Right = "??";
            public const string Forward = "?";
            public const string Back = "?";
            public const string ArrowRight = "?";
        }

        public static class Status
        {
            public const string Success = "?";
            public const string Error = "?";
            public const string Warning = "??";
            public const string Info = "??";
            public const string Loading = "?";
            public const string Complete = "?";
            public const string Pending = "?";
        }

        public static class Actions
        {
            public const string Add = "?";
            public const string Remove = "?";
            public const string Edit = "??";
            public const string Delete = "???";
            public const string Save = "??";
            public const string Cancel = "?";
            public const string Refresh = "??";
            public const string View = "???";
            public const string Search = "??";
        }

        public static class Business
        {
            public const string Building = "??";
            public const string Location = "??";
            public const string Person = "??";
            public const string Islander = "??";
            public const string Group = "??";
            public const string Document = "??";
            public const string Money = "??";
            public const string Cash = "??";
        }

        public static class Time
        {
            public const string Clock = "??";
            public const string Schedule = "??";
            public const string StartTime = "?";
            public const string EndTime = "?";
            public const string Timer = "??";
        }

        public static class Dispensers
        {
            public const string DispenserIcon = "?";
            public const string HoseNumber = "??";
            public const string GallonMeter = "??";
            public const string AmountMeter = "??";
        }

        // Alternative text-based icons for maximum compatibility
        public static class TextIcons
        {
            public const string FuelStation = "[EDS]";
            public const string Tank = "[TNK]";
            public const string Pump = "[PMP]";
            public const string Dispenser = "[DISP]";
            public const string Success = "[OK]";
            public const string Error = "[ERR]";
            public const string Warning = "[!]";
            public const string Info = "[i]";
            public const string Loading = "[...]";
            public const string Person = "[PER]";
            public const string Islander = "[ISL]";
            public const string Money = "[$]";
            public const string Time = "[TIME]";
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
                    IconType.Dispenser => TextIcons.Dispenser,
                    IconType.Person => TextIcons.Person,
                    IconType.Islander => TextIcons.Islander,
                    IconType.Money => TextIcons.Money,
                    IconType.Time => TextIcons.Time,
                    IconType.Success => TextIcons.Success,
                    IconType.Error => TextIcons.Error,
                    IconType.Warning => TextIcons.Warning,
                    IconType.Info => TextIcons.Info,
                    IconType.Loading => TextIcons.Loading,
                    _ => TextIcons.Info
                };
            }

            return iconType switch
            {
                IconType.FuelStation => Fuel.Station,
                IconType.Dispenser => Fuel.Dispenser,
                IconType.Person => Business.Person,
                IconType.Islander => Business.Islander,
                IconType.Money => Business.Money,
                IconType.Time => Time.Clock,
                IconType.Success => Status.Success,
                IconType.Error => Status.Error,
                IconType.Warning => Status.Warning,
                IconType.Info => Status.Info,
                IconType.Loading => Status.Loading,
                IconType.ArrowRight => Navigation.ArrowRight,
                IconType.Add => Actions.Add,
                IconType.Edit => Actions.Edit,
                IconType.Delete => Actions.Delete,
                _ => Status.Info
            };
        }

        /// <summary>
        /// Get appropriate icon for fuel-related elements
        /// </summary>
        /// <param name="elementType">Type of fuel element</param>
        /// <returns>Icon string</returns>
        public static string GetFuelIcon(string elementType)
        {
            return elementType?.ToLowerInvariant() switch
            {
                "dispenser" or "dispensador" => Fuel.Dispenser,
                "hose" or "manguera" => Fuel.Hose,
                "tank" or "tanque" => Fuel.Tank,
                "station" or "estacion" => Fuel.Station,
                "pump" or "bomba" => Fuel.Pump,
                _ => Fuel.Station
            };
        }

        /// <summary>
        /// Get appropriate icon for Islander roles
        /// </summary>
        /// <param name="role">Islander role</param>
        /// <returns>Icon string</returns>
        public static string GetIslanderIcon(string role)
        {
            return role?.ToLowerInvariant() switch
            {
                "supervisor" => "?????",
                "encargado de turno" => "?????",
                "cajero" => "??",
                "mantenimiento" => "??",
                "seguridad" => "???",
                "operario" => "??",
                _ => "??"
            };
        }
    }

    public enum IconType
    {
        FuelStation,
        Dispenser,
        Person,
        Islander,
        Money,
        Time,
        Success,
        Error,
        Warning,
        Info,
        Loading,
        ArrowRight,
        Add,
        Edit,
        Delete
    }
}
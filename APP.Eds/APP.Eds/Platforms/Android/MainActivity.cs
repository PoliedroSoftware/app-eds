using Android.App;
using Android.Content.PM;
using Android.OS;

namespace APP.Eds.Platforms.Android
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            UpdateSystemBars(Resources?.Configuration);
        }

        public override void OnConfigurationChanged(global::Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            UpdateSystemBars(newConfig);
        }

        private void UpdateSystemBars(global::Android.Content.Res.Configuration? configuration)
        {
            if (Window?.DecorView == null || configuration == null) return;
            var userTheme = Microsoft.Maui.Controls.Application.Current?.UserAppTheme ?? Microsoft.Maui.ApplicationModel.AppTheme.Unspecified;
            var dark = userTheme == Microsoft.Maui.ApplicationModel.AppTheme.Dark ||
                (userTheme == Microsoft.Maui.ApplicationModel.AppTheme.Unspecified &&
                 (configuration.UiMode & global::Android.Content.Res.UiMode.NightMask) == global::Android.Content.Res.UiMode.NightYes);
            var background = global::Android.Graphics.Color.ParseColor(dark ? "#0F1C27" : "#F4F7FA");
            Window.SetStatusBarColor(background);
            Window.SetNavigationBarColor(background);
            var controller = global::AndroidX.Core.View.WindowCompat.GetInsetsController(Window, Window.DecorView);
            controller.AppearanceLightStatusBars = !dark;
            controller.AppearanceLightNavigationBars = !dark;
        }
    }
}

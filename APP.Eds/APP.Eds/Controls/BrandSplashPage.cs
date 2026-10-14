using Microsoft.Maui.Controls.Shapes;

namespace APP.Eds.Controls;

/// <summary>A brief brand entrance after the platform launch screen.</summary>
public sealed class BrandSplashPage : ContentPage
{
    private readonly Action _openAccess;
    private readonly VerticalStackLayout _brand;
    private readonly Border _accent;
    private bool _started;

    public BrandSplashPage(Action openAccess)
    {
        _openAccess = openAccess;
        BackgroundColor = (Color)Application.Current!.Resources["PageBackgroundDark"];
        NavigationPage.SetHasNavigationBar(this, false);
        AutomationId = "BrandSplashPage";

        _accent = new Border
        {
            WidthRequest = 56, HeightRequest = 3, StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 2 },
            BackgroundColor = (Color)Application.Current.Resources["BrandTextDark"],
            HorizontalOptions = LayoutOptions.Center,
            Opacity = 0, ScaleX = 0.25
        };
        AutomationProperties.SetIsInAccessibleTree(_accent, false);

        var logo = new Image
        {
            Source = "poliedro_logo_dark.png", Aspect = Aspect.AspectFit,
            WidthRequest = 280, HeightRequest = 116,
            HorizontalOptions = LayoutOptions.Center
        };
        SemanticProperties.SetDescription(logo, "Poliedro Software");
        _brand = new VerticalStackLayout
        {
            Spacing = 20, HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Opacity = 0, Scale = 0.96, TranslationY = 10,
            Children =
            {
                logo,
                new Label
                {
                    Text = "EDS", FontFamily = "OpenSansSemibold",
                    FontSize = 16, CharacterSpacing = 4,
                    TextColor = (Color)Application.Current.Resources["BrandTextDark"],
                    HorizontalTextAlignment = TextAlignment.Center
                },
                _accent
            }
        };
        Content = new Grid { Padding = 32, Children = { _brand } };
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, EventArgs e)
    {
        if (_started) return;
        _started = true;
        try
        {
            // Let the first native frame render before starting the entrance.
            await Task.Delay(50);
            await Task.WhenAll(
                _brand.FadeToAsync(1, 480, Easing.CubicOut),
                _brand.ScaleToAsync(1, 560, Easing.CubicOut),
                _brand.TranslateToAsync(0, 0, 560, Easing.CubicOut));
            await Task.WhenAll(
                _accent.FadeToAsync(1, 220, Easing.CubicOut),
                _accent.ScaleXToAsync(1, 360, Easing.CubicOut));
            await _brand.FadeToAsync(0, 160, Easing.CubicIn);
        }
        catch (Exception)
        {
            // A decorative animation must never prevent access to the app.
            System.Diagnostics.Debug.WriteLine("No se pudo completar la animación de marca.");
        }
        finally
        {
            _openAccess();
        }
    }
}

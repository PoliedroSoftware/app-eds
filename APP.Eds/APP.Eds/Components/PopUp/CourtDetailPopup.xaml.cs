using System.Windows.Input;
using APP.Eds.Models.Court;
using CommunityToolkit.Maui.Views;

namespace APP.Eds.Components.PopUp;

public partial class CourtDetailPopup : Popup
{
    // Tamaños optimizados para usar todo el ancho disponible
    private const double DesktopWidthRatio = 0.98;  // Casi todo el ancho
    private const double DesktopHeightRatio = 0.85; 
    private const double MobileWidthRatio = 0.98;   // Casi todo el ancho en mobile
    private const double MobileHeightRatio = 0.90;    
    
    public CourtDetailPopup(CourtListItemModel court)
    {
        InitializeComponent();
        BindingContext = court;

        // Initialize with entry animation
        _ = AnimateEntryAsync();

        AdjustSize();

        Application.Current.MainPage.Window.SizeChanged += (s, e) =>
        {
            AdjustSize();
        };
    }

    private async Task AnimateEntryAsync()
    {
        // Start with small scale and transparency
        Frame.Scale = 0.8;
        Frame.Opacity = 0;
        Frame.TranslationY = 50;
        
        // Animate entry with spring effect
        await Task.WhenAll(
            Frame.ScaleTo(1, 400, Easing.SpringOut),
            Frame.FadeTo(1, 300, Easing.CubicOut),
            Frame.TranslateTo(0, 0, 350, Easing.CubicOut)
        );
    }

    private void AdjustSize()
    {
        try
        {
            var window = Application.Current?.MainPage?.Window;
            if (window == null) return;

            // Use almost full width for better centering and space utilization
            Frame.WidthRequest = window.Width * MobileWidthRatio;
            Frame.HeightRequest = window.Height * MobileHeightRatio;

            // Ensure minimum sizes
            Frame.WidthRequest = Math.Max(Frame.WidthRequest, 350);
            Frame.HeightRequest = Math.Max(Frame.HeightRequest, 500);
            
            // Ensure we don't exceed screen bounds
            Frame.WidthRequest = Math.Min(Frame.WidthRequest, window.Width - 16); // 8px margin on each side
            Frame.HeightRequest = Math.Min(Frame.HeightRequest, window.Height - 32); // More margin top/bottom
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adjusting popup size: {ex.Message}");
            // Fallback to full width
            var window = Application.Current?.MainPage?.Window;
            if (window != null)
            {
                Frame.WidthRequest = window.Width - 16;
                Frame.HeightRequest = window.Height * 0.85;
            }
            else
            {
                Frame.WidthRequest = 400;
                Frame.HeightRequest = 600;
            }
        }
    }

    private async void OnCloseTapped(object sender, EventArgs e)
    {
        try
        {
            await AnimateExitAsync();
            Close();
        }
        catch (ObjectDisposedException)
        {
            // Popup was already disposed, this is normal
            System.Diagnostics.Debug.WriteLine("CourtDetailPopup was already disposed");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error closing CourtDetailPopup: {ex.Message}");
            // Force close if animation fails
            try { Close(); } catch { }
        }
    }

    private async Task AnimateExitAsync()
    {
        await Task.WhenAll(
            Frame.ScaleTo(0.85, 250, Easing.CubicIn),
            Frame.FadeTo(0, 200, Easing.CubicIn),
            Frame.TranslateTo(0, -30, 250, Easing.CubicIn)
        );
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        
        // Additional setup if needed when handler is attached
        if (Handler != null)
        {
            AdjustSize();
        }
    }
}

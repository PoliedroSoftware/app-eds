using CommunityToolkit.Maui.Views;

namespace APP.Eds.Helpers;

/// <summary>Keeps existing popup contents inside the available window.</summary>
public static class PoliedroPopupSizing
{
    public static void Watch(Popup popup)
    {
        popup.Opened += (_, _) => Resize(popup);
    }

    private static void Resize(Popup popup)
    {
        if (popup.Content is not View content) return;
        if (content is Border border) border.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.SoftInput);
        else if (content is Layout layout) layout.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.SoftInput);
        else if (content is ContentView view) view.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.SoftInput);
        var display = DeviceDisplay.Current.MainDisplayInfo;
        var window = Application.Current?.Windows.FirstOrDefault();
        var availableWidth = window?.Width > 0 ? window.Width : display.Width / display.Density;
        var availableHeight = window?.Height > 0 ? window.Height : display.Height / display.Density;
        if (availableWidth <= 0 || availableHeight <= 0) return;
        var preferredWidth = popup.Size.Width > 0 ? popup.Size.Width : content.WidthRequest > 0 ? content.WidthRequest : 480;
        var preferredHeight = popup.Size.Height > 0 ? popup.Size.Height : content.HeightRequest;
        content.WidthRequest = Math.Min(Math.Min(preferredWidth, 560), Math.Max(200, availableWidth - 32));
        if (preferredHeight > 0) content.HeightRequest = Math.Min(preferredHeight, Math.Max(200, availableHeight - 48));
        if (popup.Size.Width > 0 || popup.Size.Height > 0)
            popup.Size = new Size(content.WidthRequest, content.HeightRequest > 0 ? content.HeightRequest : Math.Min(600, availableHeight - 48));
    }
}

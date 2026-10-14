namespace APP.Eds.Controls;

/// <summary>One vector icon family; legacy menu glyphs remain presentation inputs.</summary>
public sealed class PoliedroIcon : Image
{
    public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(PoliedroIcon), string.Empty, propertyChanged: Refresh);
    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(PoliedroIcon), Colors.Transparent, propertyChanged: Refresh);
    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public Color TextColor { get => (Color)GetValue(TextColorProperty); set => SetValue(TextColorProperty, value); }
    public double FontSize { get => WidthRequest; set { WidthRequest = value; HeightRequest = value; } }
    public PoliedroIcon()
    {
        WidthRequest = HeightRequest = 20;
        Aspect = Aspect.AspectFit;
        Loaded += (_, _) => { UpdateSource(); if (Application.Current is { } app) app.RequestedThemeChanged += OnThemeChanged; };
        Unloaded += (_, _) => { if (Application.Current is { } app) app.RequestedThemeChanged -= OnThemeChanged; };
        AutomationProperties.SetIsInAccessibleTree(this, false);
    }
    private static void Refresh(BindableObject sender, object oldValue, object newValue) => ((PoliedroIcon)sender).UpdateSource();
    private void OnThemeChanged(object? sender, AppThemeChangedEventArgs args) => UpdateSource();
    private void UpdateSource()
    {
        var name = Text switch
        {
            "🏢" => "business",
            "🏪" => "fuel",
            "⛽" => "fuel",
            "👤" => "user",
            "🔒" => "lock",
            "💰" => "wallet",
            "💸" => "wallet",
            "💵" => "wallet",
            "⚙️" => "settings",
            "📊" => "chart",
            "🎛️" => "device",
            "🧙‍♂️" => "settings",
            "📦" => "box",
            "🛒" => "cart",
            "🛢️" => "tank",
            "📱" => "phone",
            "⏰" => "clock",
            "📄" => "file",
            "📋" => "list",
            "📜" => "list",
            "📎" => "file",
            "💬" => "chat",
            "💳" => "card",
            "🔄" => "refresh",
            "⚠️" => "alert",
            "✅" => "check",
            "❌" => "close",
            "🔧" => "tool",
            "➕" => "plus",
            "➖" => "plus",
            "ℹ️" => "info",
            "💾" => "save",
            "📤" => "upload",
            "🔍" => "search",
            "🏭" => "business",
            "🏝️" => "fuel",
            "🔗" => "box",
            "📐" => "tank",
            "📏" => "tank",
            "📝" => "receipt",
            "💡" => "info",
            "🚀" => "arrow",
            "🎯" => "check",
            "⭐" => "check",
            "🔔" => "info",
            "⏳" => "clock",
            "🔐" => "lock",
            "✓" => "check",
            "✔" => "check",
            "✕" => "close",
            "✖" => "close",
            "×" => "close",
            "🧾" => "receipt",
            "📈" => "chart",
            "💧" => "water",
            "🏷️" => "receipt",
            "📁" => "file",
            "📅" => "calendar",
            "🔤" => "receipt",
            "🪣" => "tank",
            "🧮" => "calculator",
            "✏️" => "receipt",
            "✨" => "plus",
            "🌐" => "business",
            "🔢" => "calculator",
            "🟢" => "water",
            "🔵" => "water",
            "🤖" => "device",
            "📍" => "location",
            "👥" => "user",
            "📧" => "mail",
            "📞" => "phone",
            "👷" => "user",
            "🚛" => "truck",
            "🚚" => "truck",
            "🏦" => "bank",
            _ => "box"
        };
        Source = $"poliedro_{name}{(TextColor == Colors.White ? "_white" : Application.Current?.RequestedTheme == AppTheme.Dark ? "_dark" : string.Empty)}.png";
    }
}

using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace APP.Eds.Helpers;

/// <summary>Semantic colors shared by XAML and the screens built in C#.</summary>
public static class PoliedroTheme
{
    private static readonly Dictionary<string, Color> Light = new();

    private static readonly Dictionary<string, Color> Dark = new();

    public static Color BrandPrimary => Light["BrandPrimaryColor"];
    public static Color BrandDeep => Light["BrandDeepColor"];
    public static Color PageBackground => Light["PageBackgroundColor"];
    public static Color CardSurface => Light["CardSurfaceColor"];
    public static Color BrandSoft => Light["BrandSoftColor"];
    public static Color TextPrimary => Light["TextPrimaryColor"];
    public static Color TextSecondary => Light["TextSecondaryColor"];
    public static Color SubtleBorder => Light["SubtleBorderColor"];
    public static Color InputBorder => Light["InputBorderColor"];
    public static Color Success => Light["SuccessColor"];
    public static Color SuccessSurface => Light["SuccessSurfaceColor"];
    public static Color Warning => Light["WarningColor"];
    public static Color WarningSurface => Light["WarningSurfaceColor"];
    public static Color Error => Light["ErrorColor"];
    public static Color ErrorSurface => Light["ErrorSurfaceColor"];

    public static Color BrandText => Light["BrandTextColor"];

    public static Color ErrorFill => Light["ErrorFillColor"];

    public static Color SuccessFill => Light["SuccessFillColor"];

    public static Color WarningFill => Light["WarningFillColor"];

    public static void Initialize(Application app)
    {
        foreach (var role in new[] { "BrandPrimary", "BrandDeep", "PageBackground", "CardSurface", "BrandSoft", "TextPrimary", "TextSecondary", "SubtleBorder", "InputBorder", "Success", "SuccessSurface", "Warning", "WarningSurface", "Error", "ErrorSurface", "BrandText", "ErrorFill", "SuccessFill", "WarningFill" })
        {
            Light[role + "Color"] = (Color)app.Resources[role];
            Dark[role + "Color"] = (Color)app.Resources[role + "Dark"];
        }
    }

    // Only our own color instances are converted. Domain bindings and external
    // colors (including device/payment state) are left under their original owner.
    public static void Watch(Element root)
    {
        ApplyTree(root);
        root.DescendantAdded += (_, args) => ApplyTree(args.Element);
        if (root is VisualElement visual)
            visual.Loaded += (_, _) => ApplyTree(root);
    }

    private static void ApplyTree(Element element)
    {
        if (element is VisualElement visual)
            Bind(visual, VisualElement.BackgroundColorProperty);
        if (element is Label label) Bind(label, Label.TextColorProperty);
        if (element is Button button)
        {
            Bind(button, Button.TextColorProperty);
            Bind(button, Button.BorderColorProperty);
        }
        if (element is InputView input)
        {
            Bind(input, InputView.TextColorProperty);
            Bind(input, InputView.PlaceholderColorProperty);
        }
        if (element is Picker picker)
        {
            Bind(picker, Picker.TextColorProperty);
            Bind(picker, Picker.TitleColorProperty);
        }
        if (element is Frame frame) Bind(frame, Frame.BorderColorProperty);
        if (element is ActivityIndicator indicator) Bind(indicator, ActivityIndicator.ColorProperty);
        if (element is BoxView box) Bind(box, BoxView.ColorProperty);
        if (element is Border border && border.Stroke is SolidColorBrush brush)
        {
            foreach (var pair in Light)
                if (ReferenceEquals(brush.Color, pair.Value))
                {
                    SetColor(border, Border.StrokeProperty, pair.Key);
                    break;
                }
        }
        if (element is IVisualTreeElement tree)
            foreach (var child in tree.GetVisualChildren().OfType<Element>()) ApplyTree(child);
    }

    public static void SetColor(BindableObject target, BindableProperty property, string role)
    {
        if (property.ReturnType == typeof(Brush))
            target.SetAppTheme<Brush>(property, new SolidColorBrush(Light[role]), new SolidColorBrush(Dark[role]));
        else
            target.SetAppThemeColor(property, Light[role], Dark[role]);
    }

    private static void Bind(BindableObject target, BindableProperty property)
    {
        var value = target.GetValue(property);
        foreach (var pair in Light)
            if (ReferenceEquals(value, pair.Value))
            {
                SetColor(target, property, pair.Key);
                break;
            }
    }
}

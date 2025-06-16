namespace APP.Eds.Resources.Custom;

public partial class CustomEntryPin : Grid
{
    public delegate void CompletedEventHandler(object sender, EventArgs e);
    public event CompletedEventHandler PinCompleted;
    public event EventHandler<TextChangedEventArgs> PinTextChanged;
    public CustomEntryPin()
    {
        InitializeComponent();
        txtEntry.TextChanged += OnTextChanged;
        txtEntry.Focused += TxtEntry_Focused;
        txtEntry.Unfocused += TxtEntry_Unfocused;
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        // Dispara el evento PinTextChanged
        PinTextChanged?.Invoke(this, e);

        if (e.NewTextValue.Length == MaxLength)
        {
            PinCompleted?.Invoke(this, EventArgs.Empty); // Disparar el evento al completar el PIN.
        }
    }

    public static readonly BindableProperty IsPasswordProperty = BindableProperty.Create(
       propertyName: nameof(IsPassword),
       returnType: typeof(bool),
       declaringType: typeof(CustomEntryPin),
       defaultValue: false,
       defaultBindingMode: BindingMode.TwoWay);

    public bool IsPassword
    {
        get => (bool)GetValue(IsPasswordProperty);
        set { SetValue(IsPasswordProperty, value); }
    }

    public static readonly BindableProperty KeyBoardTypeProperty = BindableProperty.Create(
    propertyName: nameof(KeyBoardType),
    returnType: typeof(Keyboard),
    declaringType: typeof(CustomEntryPin),
    defaultValue: Keyboard.Default,
    defaultBindingMode: BindingMode.TwoWay);

    public Keyboard KeyBoardType
    {
        get => (Keyboard)GetValue(KeyBoardTypeProperty);
        set { SetValue(KeyBoardTypeProperty, value); }
    }

    public static readonly BindableProperty ReturnTypeProperty = BindableProperty.Create(
       propertyName: nameof(ReturnType),
       returnType: typeof(ReturnType),
       declaringType: typeof(CustomEntryPin),
       defaultValue: null,
       defaultBindingMode: BindingMode.TwoWay);

    public ReturnType ReturnType
    {
        get => (ReturnType)GetValue(ReturnTypeProperty);
        set { SetValue(ReturnTypeProperty, value); }
    }

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
       propertyName: nameof(Text),
       returnType: typeof(string),
       declaringType: typeof(CustomEntryPin),
       defaultValue: null,
       defaultBindingMode: BindingMode.TwoWay);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set { SetValue(TextProperty, value); }
    }


    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
      propertyName: nameof(Placeholder),
      returnType: typeof(string),
      declaringType: typeof(CustomEntryPin),
      defaultValue: null,
      defaultBindingMode: BindingMode.OneWay);

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set { SetValue(PlaceholderProperty, value); }
    }

    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
      propertyName: nameof(FontSize),
      returnType: typeof(double),
      declaringType: typeof(CustomEntryPin),
      defaultValue: 12.0,
      defaultBindingMode: BindingMode.OneWay);

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set { SetValue(FontSizeProperty, value); }
    }

    public static readonly BindableProperty HorizontalTextAlignmentProperty = BindableProperty.Create(
      propertyName: nameof(HorizontalTextAlignment),
      returnType: typeof(TextAlignment),
      declaringType: typeof(CustomEntryPin),
      defaultValue: TextAlignment.Center,
      defaultBindingMode: BindingMode.OneWay);

    public TextAlignment HorizontalTextAlignment
    {
        get => (TextAlignment)GetValue(HorizontalTextAlignmentProperty);
        set { SetValue(HorizontalTextAlignmentProperty, value); }
    }


    public static readonly BindableProperty MaxLengthProperty = BindableProperty.Create(
        propertyName: nameof(MaxLength),
        returnType: typeof(int),
        declaringType: typeof(CustomEntryPin),
        defaultValue: 1,
        defaultBindingMode: BindingMode.OneWay);

    public int MaxLength
    {
        get => (int)GetValue(MaxLengthProperty);
        set { SetValue(MaxLengthProperty, value); }
    }

    public static readonly BindableProperty TitleTextProperty = BindableProperty.Create(
        propertyName: nameof(TitleText),
        returnType: typeof(string),
        declaringType: typeof(CustomEntryPin),
        defaultValue: null,
        defaultBindingMode: BindingMode.OneWay);

    public string TitleText
    {
        get => (string)GetValue(TitleTextProperty);
        set => SetValue(TitleTextProperty, value);
    }

    public delegate void FocusEventHandler(object sender, EventArgs e, bool isfocus);
    public event FocusEventHandler FocusChanged;
    private void TxtEntry_Focused(object sender, FocusEventArgs e)
    {
        EntryBorder.Stroke = Color.FromArgb("#6200E8");
        FocusChanged?.Invoke(this, e, true);
    }

    private void TxtEntry_Unfocused(object sender, FocusEventArgs e)
    {
        EntryBorder.Stroke = Colors.Gray;
        FocusChanged?.Invoke(this, e, false);
    }

    public void SetFocus()
    {
        txtEntry.Focus();
    }

    public void LostFocus()
    {
        txtEntry.IsEnabled = false;
        txtEntry.IsEnabled = true;
    }
}
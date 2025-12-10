using APP.Eds.Components.PopUp;
using APP.Eds.Helpers;
using APP.Eds.Models.Client;
using APP.Eds.Models.Rut;
using APP.Eds.Services.Client;
using APP.Eds.Services.Rut;
using CommunityToolkit.Maui.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Globalization;

namespace APP.Eds.Components.PopUp;

public partial class CreateClientFromRutPopup : Popup, INotifyPropertyChanged
{
    private readonly ClientService _clientService;
    private readonly RutParserService _rutParserService;
    private RutParseResponse? _rutData;

    // Properties for binding
    private string _selectedClientType = "Natural";
    public string SelectedClientType
    {
        get => _selectedClientType;
        set
        {
            _selectedClientType = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNaturalSelected));
            OnPropertyChanged(nameof(IsLegalSelected));
        }
    }

    public bool IsNaturalSelected => _selectedClientType == "Natural";
    public bool IsLegalSelected => _selectedClientType == "Legal";

    private string _documentNumber = string.Empty;
    public string DocumentNumber
    {
        get => _documentNumber;
        set
        {
            _documentNumber = value;
            OnPropertyChanged();
        }
    }

    private string _documentType = string.Empty;
    public string DocumentType
    {
        get => _documentType;
        set
        {
            _documentType = value;
            OnPropertyChanged();
        }
    }

    private string _fullName = string.Empty;
    public string FullName
    {
        get => _fullName;
        set
        {
            _fullName = value;
            OnPropertyChanged();
        }
    }

    private string _firstName = string.Empty;
    public string FirstName
    {
        get => _firstName;
        set
        {
            _firstName = value;
            OnPropertyChanged();
        }
    }

    private string _middleName = string.Empty;
    public string MiddleName
    {
        get => _middleName;
        set
        {
            _middleName = value;
            OnPropertyChanged();
        }
    }

    private string _lastName = string.Empty;
    public string LastName
    {
        get => _lastName;
        set
        {
            _lastName = value;
            OnPropertyChanged();
        }
    }

    private string _secondLastName = string.Empty;
    public string SecondLastName
    {
        get => _secondLastName;
        set
        {
            _secondLastName = value;
            OnPropertyChanged();
        }
    }

    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set
        {
            _email = value;
            OnPropertyChanged();
        }
    }

    private string _nit = string.Empty;
    public string Nit
    {
        get => _nit;
        set
        {
            _nit = value;
            OnPropertyChanged();
        }
    }

    private string _address = string.Empty;
    public string Address
    {
        get => _address;
        set
        {
            _address = value;
            OnPropertyChanged();
        }
    }

    private string _city = string.Empty;
    public string City
    {
        get => _city;
        set
        {
            _city = value;
            OnPropertyChanged();
        }
    }

    private string _department = string.Empty;
    public string Department
    {
        get => _department;
        set
        {
            _department = value;
            OnPropertyChanged();
        }
    }

    private string _postalCode = string.Empty;
    public string PostalCode
    {
        get => _postalCode;
        set
        {
            _postalCode = value;
            OnPropertyChanged();
        }
    }

    private string _economicActivities = string.Empty;
    public string EconomicActivities
    {
        get => _economicActivities;
        set
        {
            _economicActivities = value;
            OnPropertyChanged();
        }
    }

    private string _responsibilities = string.Empty;
    public string Responsibilities
    {
        get => _responsibilities;
        set
        {
            _responsibilities = value;
            OnPropertyChanged();
        }
    }

    private bool _isLoading = false;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNotLoading));
        }
    }

    public bool IsNotLoading => !IsLoading;

    private bool _hasRutData = false;
    public bool HasRutData
    {
        get => _hasRutData;
        set
        {
            _hasRutData = value;
            OnPropertyChanged();
        }
    }

    public CreateClientFromRutPopup()
    {
        _clientService = new ClientService();
        _rutParserService = new RutParserService(TokenHelper.LoadToken());
        
        InitializeComponent();
        BindingContext = this;
    }

    private void InitializeComponent()
    {
        Size = new Size(550, 750);
        Color = Colors.Transparent;

        var scrollView = new ScrollView
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Always
        };

        var mainFrame = new Frame
        {
            BackgroundColor = Colors.White,
            CornerRadius = 20,
            Padding = new Thickness(24),
            HasShadow = true
        };

        var stackLayout = new StackLayout
        {
            Spacing = 16
        };

        // Header with close button
        var headerGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        var headerLabel = new Label
        {
            Text = "Crear Tercero desde RUT",
            FontSize = 24,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#374151"),
            VerticalOptions = LayoutOptions.Center
        };
        headerGrid.Add(headerLabel, 0, 0);

        var closeButton = new Button
        {
            Text = "×",
            FontSize = 24,
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromArgb("#6B7280"),
            WidthRequest = 40,
            HeightRequest = 40,
            CornerRadius = 20,
            FontAttributes = FontAttributes.Bold
        };
        closeButton.Clicked += OnCloseClicked;
        headerGrid.Add(closeButton, 1, 0);

        stackLayout.Add(headerGrid);

        // Load RUT Button
        var loadRutButton = new Button
        {
            Text = "Seleccionar RUT PDF",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            BackgroundColor = Color.FromArgb("#3B82F6"),
            TextColor = Colors.White,
            CornerRadius = 12,
            HeightRequest = 50
        };
        loadRutButton.Clicked += OnLoadRutClicked;
        loadRutButton.SetBinding(Button.IsEnabledProperty, new Binding("IsNotLoading"));
        stackLayout.Add(loadRutButton);

        // Loading indicator
        var loadingFrame = new Frame
        {
            BackgroundColor = Color.FromArgb("#EFF6FF"),
            CornerRadius = 12,
            Padding = new Thickness(16),
            BorderColor = Color.FromArgb("#3B82F6")
        };
        loadingFrame.SetBinding(Frame.IsVisibleProperty, "IsLoading");

        var loadingStack = new StackLayout
        {
            Spacing = 12,
            HorizontalOptions = LayoutOptions.Center
        };

        var activityIndicator = new ActivityIndicator
        {
            Color = Color.FromArgb("#3B82F6"),
            HeightRequest = 40
        };
        activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, "IsLoading");
        loadingStack.Add(activityIndicator);

        var loadingLabel = new Label
        {
            Text = "Procesando RUT...",
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#3B82F6"),
            HorizontalOptions = LayoutOptions.Center
        };
        loadingStack.Add(loadingLabel);

        loadingFrame.Content = loadingStack;
        stackLayout.Add(loadingFrame);

        // Data section (visible after loading RUT)
        var dataFrame = new Frame
        {
            BackgroundColor = Colors.Transparent,
            CornerRadius = 0,
            Padding = new Thickness(0),
            HasShadow = false,
            BorderColor = Colors.Transparent
        };
        dataFrame.SetBinding(Frame.IsVisibleProperty, "HasRutData");

        var dataStack = new StackLayout
        {
            Spacing = 16
        };

        // Client Type Selector - IMPROVED
        var typeFrame = new Frame
        {
            BackgroundColor = Color.FromArgb("#F8FAFC"),
            CornerRadius = 12,
            Padding = new Thickness(16),
            BorderColor = Color.FromArgb("#E2E8F0")
        };

        var typeStack = new StackLayout { Spacing = 12 };

        var typeLabel = new Label
        {
            Text = "Tipo de Tercero:",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#374151")
        };
        typeStack.Add(typeLabel);

        var typeGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            },
            ColumnSpacing = 12
        };

        var naturalButton = new Button
        {
            Text = "Persona Natural",
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 10,
            HeightRequest = 50
        };
        naturalButton.SetBinding(Button.BackgroundColorProperty, new Binding("IsNaturalSelected",
            converter: new FuncConverter<bool, Color>(selected => selected ? Color.FromArgb("#10B981") : Color.FromArgb("#E5E7EB"))));
        naturalButton.SetBinding(Button.TextColorProperty, new Binding("IsNaturalSelected",
            converter: new FuncConverter<bool, Color>(selected => selected ? Colors.White : Color.FromArgb("#6B7280"))));
        naturalButton.Clicked += (s, e) => SelectedClientType = "Natural";
        typeGrid.Add(naturalButton, 0, 0);

        var legalButton = new Button
        {
            Text = "Persona Jurídica",
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 10,
            HeightRequest = 50
        };
        legalButton.SetBinding(Button.BackgroundColorProperty, new Binding("IsLegalSelected",
            converter: new FuncConverter<bool, Color>(selected => selected ? Color.FromArgb("#6366F1") : Color.FromArgb("#E5E7EB"))));
        legalButton.SetBinding(Button.TextColorProperty, new Binding("IsLegalSelected",
            converter: new FuncConverter<bool, Color>(selected => selected ? Colors.White : Color.FromArgb("#6B7280"))));
        legalButton.Clicked += (s, e) => SelectedClientType = "Legal";
        typeGrid.Add(legalButton, 1, 0);

        typeStack.Add(typeGrid);
        typeFrame.Content = typeStack;
        dataStack.Add(typeFrame);

        // EXPANDED: Identification Section
        dataStack.Add(CreateSectionHeader("Datos de Identificación"));
        dataStack.Add(CreateReadOnlyField("Tipo de Documento:", "DocumentType"));
        dataStack.Add(CreateReadOnlyField("Número de Documento:", "DocumentNumber"));
        dataStack.Add(CreateReadOnlyField("NIT:", "Nit"));

        // EXPANDED: Personal Information Section
        dataStack.Add(CreateSectionHeader("Información Personal"));
        dataStack.Add(CreateEditableField("Nombre Completo:", "FullName"));
        dataStack.Add(CreateReadOnlyField("Primer Nombre:", "FirstName"));
        dataStack.Add(CreateReadOnlyField("Segundo Nombre:", "MiddleName"));
        dataStack.Add(CreateReadOnlyField("Primer Apellido:", "LastName"));
        dataStack.Add(CreateReadOnlyField("Segundo Apellido:", "SecondLastName"));

        // EXPANDED: Contact Information Section
        dataStack.Add(CreateSectionHeader("Información de Contacto"));
        dataStack.Add(CreateEditableField("Email:", "Email"));

        // EXPANDED: Address Section
        dataStack.Add(CreateSectionHeader("Información de Ubicación"));
        dataStack.Add(CreateReadOnlyField("Dirección:", "Address"));
        dataStack.Add(CreateReadOnlyField("Ciudad:", "City"));
        dataStack.Add(CreateReadOnlyField("Departamento:", "Department"));
        dataStack.Add(CreateReadOnlyField("Código Postal:", "PostalCode"));

        // EXPANDED: Business Information Section
        dataStack.Add(CreateSectionHeader("Información Comercial"));
        dataStack.Add(CreateMultilineField("Actividades Económicas:", "EconomicActivities"));
        dataStack.Add(CreateMultilineField("Responsabilidades Fiscales:", "Responsibilities"));

        dataFrame.Content = dataStack;
        stackLayout.Add(dataFrame);

        // Create button
        var createButton = new Button
        {
            Text = "Crear Tercero",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            BackgroundColor = Color.FromArgb("#10B981"),
            TextColor = Colors.White,
            CornerRadius = 12,
            HeightRequest = 55,
            Margin = new Thickness(0, 10, 0, 0)
        };
        createButton.Clicked += OnCreateClientClicked;
        createButton.SetBinding(Button.IsEnabledProperty, "IsNotLoading");
        createButton.SetBinding(Button.IsVisibleProperty, "HasRutData");
        stackLayout.Add(createButton);

        mainFrame.Content = stackLayout;
        scrollView.Content = mainFrame;
        Content = scrollView;
    }

    private Frame CreateSectionHeader(string text)
    {
        var frame = new Frame
        {
            BackgroundColor = Color.FromArgb("#EFF6FF"),
            CornerRadius = 8,
            Padding = new Thickness(12, 10),
            HasShadow = false,
            BorderColor = Color.FromArgb("#DBEAFE"),
            Margin = new Thickness(0, 8, 0, 4)
        };

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            },
            ColumnSpacing = 10
        };

        // Icon based on section type
        var icon = new Label
        {
            FontSize = 18,
            TextColor = Color.FromArgb("#1E40AF"),
            VerticalOptions = LayoutOptions.Center
        };

        if (text.Contains("Identificación"))
            icon.Text = "?";
        else if (text.Contains("Personal"))
            icon.Text = "?";
        else if (text.Contains("Contacto"))
            icon.Text = "?";
        else if (text.Contains("Ubicación"))
            icon.Text = "?";
        else if (text.Contains("Comercial"))
            icon.Text = "?";

        grid.Add(icon, 0, 0);

        var label = new Label
        {
            Text = text,
            FontSize = 15,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#1E40AF"),
            VerticalOptions = LayoutOptions.Center
        };

        grid.Add(label, 1, 0);

        frame.Content = grid;
        return frame;
    }

    private Frame CreateReadOnlyField(string labelText, string bindingPath)
    {
        var frame = new Frame
        {
            BackgroundColor = Color.FromArgb("#F9FAFB"),
            CornerRadius = 8,
            Padding = new Thickness(12),
            HasShadow = false,
            BorderColor = Color.FromArgb("#E5E7EB")
        };

        var stack = new StackLayout { Spacing = 4 };

        var label = new Label
        {
            Text = labelText,
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#6B7280")
        };
        stack.Add(label);

        var valueLabel = new Label
        {
            FontSize = 14,
            TextColor = Color.FromArgb("#374151"),
            LineBreakMode = LineBreakMode.WordWrap
        };
        valueLabel.SetBinding(Label.TextProperty, bindingPath);
        stack.Add(valueLabel);

        frame.Content = stack;
        return frame;
    }

    private Frame CreateEditableField(string labelText, string bindingPath)
    {
        var frame = new Frame
        {
            BackgroundColor = Colors.White,
            CornerRadius = 8,
            Padding = new Thickness(12),
            HasShadow = false,
            BorderColor = Color.FromArgb("#3B82F6")
        };

        var stack = new StackLayout { Spacing = 4 };

        var label = new Label
        {
            Text = labelText,
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#3B82F6")
        };
        stack.Add(label);

        var entry = new Entry
        {
            FontSize = 14,
            TextColor = Color.FromArgb("#374151"),
            BackgroundColor = Colors.Transparent
        };
        entry.SetBinding(Entry.TextProperty, bindingPath);
        stack.Add(entry);

        frame.Content = stack;
        return frame;
    }

    private Frame CreateMultilineField(string labelText, string bindingPath)
    {
        var frame = new Frame
        {
            BackgroundColor = Color.FromArgb("#FFFBEB"),
            CornerRadius = 8,
            Padding = new Thickness(12),
            HasShadow = false,
            BorderColor = Color.FromArgb("#FDE68A")
        };

        var stack = new StackLayout { Spacing = 4 };

        var label = new Label
        {
            Text = labelText,
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#92400E")
        };
        stack.Add(label);

        var valueLabel = new Label
        {
            FontSize = 13,
            TextColor = Color.FromArgb("#78350F"),
            LineBreakMode = LineBreakMode.WordWrap
        };
        valueLabel.SetBinding(Label.TextProperty, bindingPath);
        stack.Add(valueLabel);

        frame.Content = stack;
        return frame;
    }

    private async void OnLoadRutClicked(object sender, EventArgs e)
    {
        try
        {
            IsLoading = true;

            // Open file picker for PDF
            var customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "com.adobe.pdf", "public.pdf" } },
                    { DevicePlatform.Android, new[] { "application/pdf" } },
                    { DevicePlatform.WinUI, new[] { ".pdf" } },
                    { DevicePlatform.macOS, new[] { "pdf" } },
                });

            var options = new PickOptions
            {
                PickerTitle = "Seleccione el archivo RUT en formato PDF",
                FileTypes = customFileType
            };

            var result = await FilePicker.Default.PickAsync(options);

            if (result == null)
            {
                IsLoading = false;
                return;
            }

            // Parse RUT
            _rutData = await _rutParserService.ParseRutFromPdfAsync(result.FullPath);

            if (_rutData == null)
            {
                await CustomAlert.ShowErrorAsync(
                    "No se pudo extraer la información del RUT.\n\n" +
                    "Verifique que el archivo sea un RUT válido de la DIAN.",
                    "Error al Procesar RUT");
                IsLoading = false;
                return;
            }

            // Check if client already exists
            var (exists, existingClient) = await _clientService.CheckClientExistsAsync(_rutData.DocumentNumber);

            if (exists && existingClient != null)
            {
                var useExisting = await Application.Current.MainPage.DisplayAlert(
                    "Cliente Ya Existe",
                    $"El cliente '{existingClient.Name}' con documento {existingClient.DocumentNumber} ya está registrado.\n\n" +
                    "¿Desea usar este cliente existente?",
                    "Usar Existente",
                    "Cancelar");

                if (useExisting)
                {
                    await CloseAsync(existingClient);
                    return;
                }
                else
                {
                    IsLoading = false;
                    return;
                }
            }

            // Load data into form with all details
            DocumentType = _rutData.DocumentType;
            DocumentNumber = _rutData.DocumentNumber;
            FullName = _rutData.FullName.Display;
            FirstName = _rutData.FullName.FirstName;
            MiddleName = _rutData.FullName.MiddleNames;
            LastName = _rutData.FullName.LastName;
            SecondLastName = _rutData.FullName.SecondLastName;
            Email = _rutData.Email;
            Nit = _rutData.Nit;
            Address = _rutData.Address;
            City = _rutData.City;
            Department = _rutData.Department;
            PostalCode = _rutData.PostalCode;

            // Format economic activities
            if (_rutData.EconomicActivities != null && _rutData.EconomicActivities.Any())
            {
                EconomicActivities = string.Join("\n", _rutData.EconomicActivities.Select(a => 
                    $"• {a.Code}: {a.Description}"));
            }
            else
            {
                EconomicActivities = "No especificadas";
            }

            // Format responsibilities
            if (_rutData.Responsibilities != null && _rutData.Responsibilities.Any())
            {
                Responsibilities = string.Join("\n", _rutData.Responsibilities.Select(r => 
                    $"• {r.Code}: {r.Description}"));
            }
            else
            {
                Responsibilities = "No especificadas";
            }

            HasRutData = true;

            await CustomAlert.ShowSuccessAsync(
                $"RUT Cargado Exitosamente\n\n" +
                $"Se han extraído todos los datos del RUT:\n\n" +
                $"Documento: {_rutData.DocumentNumber}\n" +
                $"Nombre: {_rutData.FullName.Display}\n" +
                $"Email: {_rutData.Email}\n" +
                $"Ciudad: {_rutData.City}\n" +
                $"Actividades: {_rutData.EconomicActivities?.Count ?? 0}\n" +
                $"Responsabilidades: {_rutData.Responsibilities?.Count ?? 0}\n\n" +
                $"Revise todos los datos y seleccione el tipo de tercero.",
                "Datos Extraídos");
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync(
                $"Error al cargar el RUT:\n\n{ex.Message}",
                "Error del Sistema");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async void OnCreateClientClicked(object sender, EventArgs e)
    {
        if (_rutData == null)
        {
            await CustomAlert.ShowErrorAsync(
                "No hay datos del RUT cargados.\n\nPor favor, seleccione un archivo RUT primero.",
                "Datos Faltantes");
            return;
        }

        // Validate required fields
        if (string.IsNullOrWhiteSpace(Email))
        {
            await CustomAlert.ShowErrorAsync(
                "El email es obligatorio.\n\nPor favor, ingrese un email válido.",
                "Email Requerido");
            return;
        }

        if (string.IsNullOrWhiteSpace(FullName))
        {
            await CustomAlert.ShowErrorAsync(
                "El nombre completo es obligatorio.",
                "Nombre Requerido");
            return;
        }

        try
        {
            IsLoading = true;

            ClientLegalModel? createdClient = null;

            if (SelectedClientType == "Natural")
            {
                // Create Natural Person
                var naturalClient = await _clientService.CreateNaturalClientFromRutAsync(_rutData);
                if (naturalClient != null)
                {
                    // Convert to ClientLegalModel for compatibility
                    createdClient = new ClientLegalModel
                    {
                        Id = naturalClient.Id,
                        Name = naturalClient.FullName,
                        DocumentNumber = naturalClient.DocumentNumber,
                        DocumentTypeId = naturalClient.DocumentTypeId,
                        Email = naturalClient.Email
                    };
                }
            }
            else
            {
                // Create Legal Person
                createdClient = await _clientService.CreateLegalClientFromRutAsync(_rutData);
            }

            if (createdClient != null)
            {
                await CustomAlert.ShowSuccessAsync(
                    $"Tercero Creado Exitosamente\n\n" +
                    $"Tipo: {(SelectedClientType == "Natural" ? "Persona Natural" : "Persona Jurídica")}\n" +
                    $"Nombre: {_rutData.FullName.Display}\n" +
                    $"Documento: {_rutData.DocumentNumber}\n" +
                    $"Email: {Email}\n\n" +
                    $"El tercero ha sido registrado y seleccionado correctamente.",
                    "Tercero Creado");

                await CloseAsync(createdClient);
            }
            else
            {
                await CustomAlert.ShowErrorAsync(
                    "No se pudo crear el tercero en el servidor.\n\n" +
                    "Verifique su conexión e intente nuevamente.\n" +
                    "Si el problema persiste, contacte al administrador.",
                    "Error al Crear");
            }
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync(
                $"Error al crear el tercero:\n\n{ex.Message}\n\n" +
                $"Stack trace: {ex.StackTrace?.Substring(0, Math.Min(200, ex.StackTrace?.Length ?? 0))}",
                "Error del Sistema");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        _ = CloseAsync(null);
    }

    private async Task CloseAsync(ClientLegalModel? result)
    {
        try
        {
            await Task.Delay(50);
            Close(result);
        }
        catch
        {
            // Ignore close errors
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

// Helper converter for button styling
public class FuncConverter<TSource, TTarget> : IValueConverter
{
    private readonly Func<TSource, TTarget> _convertFunc;

    public FuncConverter(Func<TSource, TTarget> convertFunc)
    {
        _convertFunc = convertFunc ?? throw new ArgumentNullException(nameof(convertFunc));
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TSource sourceValue)
        {
            return _convertFunc(sourceValue);
        }
        return default(TTarget);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

using APP.Eds.Components.PopUp;
using APP.Eds.Helpers;
using APP.Eds.Models.TypeOfCollection;
using APP.Eds.Services.Config;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows.Input;

namespace APP.Eds.Services.TypeOfCollection
{
    public class PaymentMethodItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public decimal ProcessingFee { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool RequiresAuth { get; set; }
        public bool IsDefault { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = "💳";
        
        // New property for handling specific payment amounts
        public decimal Amount { get; set; }
        
        // Property to show formatted amount for display
        public string FormattedAmount => $"${Amount:N2}";
        
        // Property to check if this payment method has an amount assigned
        public bool HasAmount => Amount > 0;
    }

    public class TypeOfCollectionService : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        
        // Collections
        public ObservableCollection<string> PaymentTypes { get; set; } = new();
        public ObservableCollection<string> PaymentMethods { get; set; } = new();
        public ObservableCollection<string> StatusOptions { get; set; } = new();
        public ObservableCollection<PaymentMethodItem> PaymentMethodsList { get; set; } = new();

        private TypeOfCollectionRequest Request { get; set; }
        private TypeOfCollectionModel _typeOfCollection;
        private string? _authToken;

        public TypeOfCollectionModel TypeOfCollection
        {
            get => _typeOfCollection;
            set
            {
                _typeOfCollection = value;
                OnPropertyChanged(nameof(TypeOfCollection));
            }
        }

        // Enhanced Form Properties
        private string _selectedPaymentType;
        public string SelectedPaymentType
        {
            get => _selectedPaymentType;
            set
            {
                _selectedPaymentType = value;
                OnPropertyChanged(nameof(SelectedPaymentType));
                UpdatePaymentMethodOptions();
            }
        }

        private string _selectedPaymentMethod;
        public string SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set
            {
                _selectedPaymentMethod = value;
                OnPropertyChanged(nameof(SelectedPaymentMethod));
            }
        }

        private string _paymentName;
        public string PaymentName
        {
            get => _paymentName;
            set
            {
                _paymentName = value;
                OnPropertyChanged(nameof(PaymentName));
            }
        }

        private string _paymentProvider;
        public string PaymentProvider
        {
            get => _paymentProvider;
            set
            {
                _paymentProvider = value;
                OnPropertyChanged(nameof(PaymentProvider));
            }
        }

        private string _processingFee;
        public string ProcessingFee
        {
            get => _processingFee;
            set
            {
                _processingFee = value;
                OnPropertyChanged(nameof(ProcessingFee));
            }
        }

        // ✨ NUEVO: Campo para el monto específico del método de pago
        private string _paymentAmount;
        public string PaymentAmount
        {
            get => _paymentAmount;
            set
            {
                _paymentAmount = value;
                OnPropertyChanged(nameof(PaymentAmount));
                OnPropertyChanged(nameof(PaymentAmountDecimal));
            }
        }

        // Propiedad calculada para obtener el monto como decimal
        public decimal PaymentAmountDecimal
        {
            get
            {
                if (decimal.TryParse(_paymentAmount?.Replace(",", ""), out decimal amount))
                    return amount;
                return 0m;
            }
        }

        private string _selectedStatus;
        public string SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                _selectedStatus = value;
                OnPropertyChanged(nameof(SelectedStatus));
            }
        }

        private bool _requiresAuth;
        public bool RequiresAuth
        {
            get => _requiresAuth;
            set
            {
                _requiresAuth = value;
                OnPropertyChanged(nameof(RequiresAuth));
            }
        }

        private bool _isDefault;
        public bool IsDefault
        {
            get => _isDefault;
            set
            {
                _isDefault = value;
                OnPropertyChanged(nameof(IsDefault));
            }
        }

        // Translation Properties (keeping for compatibility)
        private string _descriptionTitle;
        public string DescriptionTitle
        {
            get => _descriptionTitle;
            set
            {
                _descriptionTitle = value;
                OnPropertyChanged(nameof(DescriptionTitle));
            }
        }

        private string _descriptionPlaceHolder;
        public string DescriptionPlaceHolder
        {
            get => _descriptionPlaceHolder;
            set
            {
                _descriptionPlaceHolder = value;
                OnPropertyChanged(nameof(DescriptionPlaceHolder));
            }
        }

        private string _descriptionLabel;
        public string DescriptionLabel
        {
            get => _descriptionLabel;
            set
            {
                _descriptionLabel = value;
                OnPropertyChanged(nameof(DescriptionLabel));
            }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        private string _sendData;
        public string SendData
        {
            get => _sendData;
            set
            {
                _sendData = value;
                OnPropertyChanged(nameof(SendData));
            }
        }

        private string _errorEmpty;
        public string ErrorEmpty
        {
            get => _errorEmpty;
            set
            {
                _errorEmpty = value;
                OnPropertyChanged(nameof(ErrorEmpty));
            }
        }

        private string _errorCharacteres;
        public string ErrorCharacteres
        {
            get => _errorCharacteres;
            set
            {
                _errorCharacteres = value;
                OnPropertyChanged(nameof(ErrorCharacteres));
            }
        }

        // Statistics Properties
        private int _totalPaymentMethods;
        public int TotalPaymentMethods
        {
            get => _totalPaymentMethods;
            set
            {
                _totalPaymentMethods = value;
                OnPropertyChanged(nameof(TotalPaymentMethods));
            }
        }

        private int _activePaymentMethods;
        public int ActivePaymentMethods
        {
            get => _activePaymentMethods;
            set
            {
                _activePaymentMethods = value;
                OnPropertyChanged(nameof(ActivePaymentMethods));
            }
        }

        private int _digitalPaymentMethods;
        public int DigitalPaymentMethods
        {
            get => _digitalPaymentMethods;
            set
            {
                _digitalPaymentMethods = value;
                OnPropertyChanged(nameof(DigitalPaymentMethods));
            }
        }

        private int _cashPaymentMethods;
        public int CashPaymentMethods
        {
            get => _cashPaymentMethods;
            set
            {
                _cashPaymentMethods = value;
                OnPropertyChanged(nameof(CashPaymentMethods));
            }
        }

        // Filter Properties
        private Color _filterAllColor = Color.FromArgb("#4CAF50");
        public Color FilterAllColor
        {
            get => _filterAllColor;
            set
            {
                _filterAllColor = value;
                OnPropertyChanged(nameof(FilterAllColor));
            }
        }

        private Color _filterActiveColor = Color.FromArgb("#9E9E9E");
        public Color FilterActiveColor
        {
            get => _filterActiveColor;
            set
            {
                _filterActiveColor = value;
                OnPropertyChanged(nameof(FilterActiveColor));
            }
        }

        private Color _filterDigitalColor = Color.FromArgb("#9E9E9E");
        public Color FilterDigitalColor
        {
            get => _filterDigitalColor;
            set
            {
                _filterDigitalColor = value;
                OnPropertyChanged(nameof(FilterDigitalColor));
            }
        }

        // Commands
        public ICommand GetByIdTypeOfCollectionDataCommand { get; private set; }
        public ICommand SaveTypeOfCollectionDataCommand { get; private set; }
        public ICommand FilterAllCommand { get; private set; }
        public ICommand FilterActiveCommand { get; private set; }
        public ICommand FilterDigitalCommand { get; private set; }
        public ICommand EditPaymentMethodCommand { get; private set; }
        public ICommand DeletePaymentMethodCommand { get; private set; }
        public ICommand ValidatePaymentCompletionCommand { get; private set; }
        public ICommand ShowPaymentMethodsPopupCommand { get; private set; }
        public ICommand CheckBeforeAddPaymentMethodCommand { get; private set; } // ✨ NUEVO: Comando para validar antes de agregar

        public TypeOfCollectionService()
        {
            InitializeCommands();
            InitializeOptions();
            _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
            LoadTranslationsAsync();
        }

        private void InitializeCommands()
        {
            GetByIdTypeOfCollectionDataCommand = new Command<int>(async (typeOfCollectionId) => await GetByIdTypeOfCollectionDataAsync(typeOfCollectionId));
            SaveTypeOfCollectionDataCommand = new Command(async () => await SaveTypeOfCollectionDataAsync());
            FilterAllCommand = new Command(() => FilterPaymentMethods("all"));
            FilterActiveCommand = new Command(() => FilterPaymentMethods("active"));
            FilterDigitalCommand = new Command(() => FilterPaymentMethods("digital"));
            EditPaymentMethodCommand = new Command<PaymentMethodItem>(async (method) => await EditPaymentMethodAsync(method));
            DeletePaymentMethodCommand = new Command<PaymentMethodItem>(async (method) => await DeletePaymentMethodAsync(method));
            ValidatePaymentCompletionCommand = new Command(async () => await ValidatePaymentCompletionAsync());
            ShowPaymentMethodsPopupCommand = new Command(async () => await ShowPaymentMethodsPopupAsync());
            CheckBeforeAddPaymentMethodCommand = new Command(async () => await CheckBeforeAddPaymentMethodAsync()); // ✨ NUEVO: Inicializar comando
        }

        private void InitializeOptions()
        {
            // Payment Types
            PaymentTypes.Clear();
            PaymentTypes.Add("Efectivo");
            PaymentTypes.Add("Tarjeta de Debito");
            PaymentTypes.Add("Tarjeta de Credito");
            PaymentTypes.Add("Transferencia Bancaria");
            PaymentTypes.Add("Pago Electronico");
            PaymentTypes.Add("Billetera Digital");
            PaymentTypes.Add("Criptomoneda");
            PaymentTypes.Add("Cheque");

            // Status Options
            StatusOptions.Clear();
            StatusOptions.Add("Activo");
            StatusOptions.Add("Inactivo");
            StatusOptions.Add("En Pruebas");
            StatusOptions.Add("Mantenimiento");
        }

        private void UpdatePaymentMethodOptions()
        {
            PaymentMethods.Clear();
            
            switch (SelectedPaymentType)
            {
                case "Efectivo":
                    PaymentMethods.Add("Efectivo Pesos");
                    PaymentMethods.Add("Efectivo Dolares");
                    break;
                case "Tarjeta de Debito":
                case "Tarjeta de Credito":
                    PaymentMethods.Add("Visa");
                    PaymentMethods.Add("MasterCard");
                    PaymentMethods.Add("American Express");
                    PaymentMethods.Add("Diners Club");
                    break;
                case "Transferencia Bancaria":
                    PaymentMethods.Add("PSE");
                    PaymentMethods.Add("Transferencia ACH");
                    PaymentMethods.Add("SWIFT");
                    break;
                case "Pago Electronico":
                    PaymentMethods.Add("PayPal");
                    PaymentMethods.Add("Stripe");
                    PaymentMethods.Add("Mercado Pago");
                    break;
                case "Billetera Digital":
                    PaymentMethods.Add("Nequi");
                    PaymentMethods.Add("Daviplata");
                    PaymentMethods.Add("Tpaga");
                    PaymentMethods.Add("Apple Pay");
                    PaymentMethods.Add("Google Pay");
                    break;
                case "Criptomoneda":
                    PaymentMethods.Add("Bitcoin");
                    PaymentMethods.Add("Ethereum");
                    PaymentMethods.Add("Litecoin");
                    break;
                case "Cheque":
                    PaymentMethods.Add("Cheque Personal");
                    PaymentMethods.Add("Cheque Empresarial");
                    PaymentMethods.Add("Cheque de Gerencia");
                    break;
                default:
                    PaymentMethods.Add("Generico");
                    break;
            }
        }

        public async Task InitializeAsync()
        {
            await LoadPaymentMethodsAsync();
            UpdateStatistics();
            FilterPaymentMethods("all"); // Default filter
        }

        private async Task LoadPaymentMethodsAsync()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return;
            }

            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                
                // Call the type-of-collection API endpoint to get payment methods
                var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/type-of-collection");
                
                var apiResponse = JsonSerializer.Deserialize<TypeOfCollectionApiResponse>(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                PaymentMethodsList.Clear();
                
                if (apiResponse?.Data != null)
                {
                    foreach (var item in apiResponse.Data.OrderBy(x => x.Description))
                    {
                        // Parse the description to extract payment method details
                        var paymentMethod = ParsePaymentMethodFromDescription(item.IdTypeOfCollection, item.Description);
                        if (paymentMethod != null)
                        {
                            PaymentMethodsList.Add(paymentMethod);
                        }
                    }
                }

                // If no payment methods are loaded from API, add some default sample data as fallback
                if (!PaymentMethodsList.Any())
                {
                    await LoadSamplePaymentMethodsAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading payment methods from API: {ex.Message}");
                
                // Fallback to sample data if API call fails
                await LoadSamplePaymentMethodsAsync();
                
                // Show error message to user
                await Application.Current.MainPage.DisplayAlert("Advertencia", 
                    $"No se pudieron cargar los métodos de pago desde el servidor. Se muestran datos de ejemplo.\nError: {ex.Message}", "OK");
            }
        }

        private PaymentMethodItem ParsePaymentMethodFromDescription(int id, string description)
        {
            try
            {
                // Example description format: "[Efectivo Pesos] Tipo: Efectivo, Metodo: Efectivo Pesos, Proveedor: N/A, Comision: 0%, Estado: Activo - Pago en efectivo moneda nacional"
                
                if (string.IsNullOrWhiteSpace(description))
                {
                    return new PaymentMethodItem 
                    { 
                        Id = id, 
                        Name = "Método sin nombre", 
                        Type = "Generico", 
                        Provider = "N/A", 
                        ProcessingFee = 0, 
                        Status = "Activo", 
                        Description = description ?? "", 
                        Icon = "💳" 
                    };
                }

                var paymentMethod = new PaymentMethodItem { Id = id, Description = description };

                // Extract name from brackets [Name]
                var nameMatch = System.Text.RegularExpressions.Regex.Match(description, @"\[(.*?)\]");
                paymentMethod.Name = nameMatch.Success ? nameMatch.Groups[1].Value : "Método desconocido";

                // Extract type
                var typeMatch = System.Text.RegularExpressions.Regex.Match(description, @"Tipo:\s*([^,]+)");
                paymentMethod.Type = typeMatch.Success ? typeMatch.Groups[1].Value.Trim() : "Generico";

                // Extract provider
                var providerMatch = System.Text.RegularExpressions.Regex.Match(description, @"Proveedor:\s*([^,]+)");
                paymentMethod.Provider = providerMatch.Success ? providerMatch.Groups[1].Value.Trim() : "N/A";

                // Extract processing fee
                var feeMatch = System.Text.RegularExpressions.Regex.Match(description, @"Comision:\s*([0-9.,]+)");
                if (feeMatch.Success && decimal.TryParse(feeMatch.Groups[1].Value.Replace("%", "").Trim(), out decimal fee))
                {
                    paymentMethod.ProcessingFee = fee;
                }

                // Extract status
                var statusMatch = System.Text.RegularExpressions.Regex.Match(description, @"Estado:\s*([^-]+)");
                paymentMethod.Status = statusMatch.Success ? statusMatch.Groups[1].Value.Trim() : "Activo";

                // Set icon based on type
                paymentMethod.Icon = GetPaymentIcon(paymentMethod.Type);

                // Set authentication and default flags based on type
                paymentMethod.RequiresAuth = paymentMethod.Type != "Efectivo";
                paymentMethod.IsDefault = paymentMethod.Type == "Efectivo" && paymentMethod.Name.Contains("Pesos");

                return paymentMethod;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing payment method description: {ex.Message}");
                
                // Return a basic payment method if parsing fails
                return new PaymentMethodItem 
                { 
                    Id = id, 
                    Name = description?.Length > 50 ? description.Substring(0, 50) + "..." : description ?? "Método desconocido", 
                    Type = "Generico", 
                    Provider = "N/A", 
                    ProcessingFee = 0, 
                    Status = "Activo", 
                    Description = description ?? "", 
                    Icon = "💳" 
                };
            }
        }

        private async Task LoadSamplePaymentMethodsAsync()
        {
            try
            {
                // Create sample payment methods as fallback
                var sampleMethods = new List<PaymentMethodItem>
                {
                    new PaymentMethodItem { Id = 1, Name = "Efectivo Pesos", Type = "Efectivo", Provider = "N/A", ProcessingFee = 0, Status = "Activo", RequiresAuth = false, IsDefault = true, Description = "Pago en efectivo moneda nacional", Icon = "💵" },
                    new PaymentMethodItem { Id = 2, Name = "Tarjeta Visa Debito", Type = "Tarjeta de Debito", Provider = "Visa", ProcessingFee = 2.5m, Status = "Activo", RequiresAuth = true, IsDefault = false, Description = "Tarjeta debito red Visa", Icon = "💳" },
                    new PaymentMethodItem { Id = 3, Name = "PSE Transferencia", Type = "Transferencia Bancaria", Provider = "PSE", ProcessingFee = 1.8m, Status = "Activo", RequiresAuth = true, IsDefault = false, Description = "Transferencia electronica PSE", Icon = "🏦" },
                    new PaymentMethodItem { Id = 4, Name = "Nequi", Type = "Billetera Digital", Provider = "Bancolombia", ProcessingFee = 1.2m, Status = "En Pruebas", RequiresAuth = true, IsDefault = false, Description = "Billetera digital Nequi", Icon = "📱" }
                };

                PaymentMethodsList.Clear();
                foreach (var method in sampleMethods.OrderBy(x => x.Name))
                {
                    PaymentMethodsList.Add(method);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error cargando formas de pago de ejemplo: {ex.Message}", "OK");
            }
        }

        private void FilterPaymentMethods(string filter)
        {
            // Reset filter button colors
            FilterAllColor = Color.FromArgb("#9E9E9E");
            FilterActiveColor = Color.FromArgb("#9E9E9E");
            FilterDigitalColor = Color.FromArgb("#9E9E9E");

            // Set active button color and apply filter logic
            switch (filter)
            {
                case "all":
                    FilterAllColor = Color.FromArgb("#4CAF50");
                    // Show all payment methods (no filtering needed for ObservableCollection display)
                    break;
                case "active":
                    FilterActiveColor = Color.FromArgb("#4CAF50");
                    // Filter active payment methods (implementation would filter the collection)
                    break;
                case "digital":
                    FilterDigitalColor = Color.FromArgb("#4CAF50");
                    // Filter digital payment methods
                    break;
            }
        }

        private async Task EditPaymentMethodAsync(PaymentMethodItem method)
        {
            try
            {
                // Load payment method data into form for editing
                PaymentName = method.Name;
                SelectedPaymentType = method.Type;
                PaymentProvider = method.Provider;
                ProcessingFee = method.ProcessingFee.ToString();
                SelectedStatus = method.Status;
                RequiresAuth = method.RequiresAuth;
                IsDefault = method.IsDefault;
                Description = method.Description;

                await Application.Current.MainPage.DisplayAlert("Modo Edicion", $"Datos de '{method.Name}' cargados para edicion", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error editando forma de pago: {ex.Message}", "OK");
            }
        }

        private async Task DeletePaymentMethodAsync(PaymentMethodItem method)
        {
            try
            {
                var result = await Application.Current.MainPage.DisplayAlert(
                    "Confirmar eliminacion",
                    $"¿Esta seguro de eliminar la forma de pago '{method.Name}'?\nEsta accion no se puede deshacer.",
                    "Eliminar",
                    "Cancelar");

                if (result)
                {
                    PaymentMethodsList.Remove(method);
                    UpdateStatistics();
                    await Application.Current.MainPage.DisplayAlert("Exito", "Forma de pago eliminada correctamente", "OK");
                }
            }   
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error eliminando forma de pago: {ex.Message}", "OK");
            }
        }

        public async Task<Dictionary<string, string>> GetTranslationsByLanguageAsync(string languageTag)
        {
            _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
            if (string.IsNullOrWhiteSpace(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Authentication token is missing", "OK");
                return new Dictionary<string, string>();
            }

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/translations");
            var data = JsonSerializer.Deserialize<TranslationsResponse>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return data.Translations.TryGetValue(languageTag, out var translations) ? translations
                : new Dictionary<string, string>();
        }

        public async Task GetByIdTypeOfCollectionDataAsync(int typeOfCollectionId)
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return;
            }
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/type-of-collection/{typeOfCollectionId}");

                TypeOfCollection = JsonSerializer.Deserialize<TypeOfCollectionModel>(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar el dato: {ex.Message}", "OK");
            }
        }

        public async Task SaveTypeOfCollectionDataAsync()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await CustomAlert.ShowErrorAsync("No se encontró el token de autenticación", "Error de Autenticación");
                return;
            }

            try
            {
                // ✨ VALIDACIÓN: Verificar si ya se completaron todos los pagos ANTES de permitir agregar más
                CalculateTotalPaymentMethodsAmount();
                
                if (IsPaymentComplete && TotalSalesAmount > 0)
                {
                    await CustomAlert.ShowSuccessAsync(
                        $"✅ Pagos Completos\n\n" +
                        $"Ya se han registrado todos los métodos de pago necesarios:\n\n" +
                        $"• Total de ventas: ${TotalSalesAmount:N2}\n" +
                        $"• Total pagos registrados: ${TotalPaymentMethodsAmount:N2}\n\n" +
                        $"No es necesario agregar más métodos de pago.\n" +
                        $"Los pagos han sido registrados correctamente.",
                        "Pagos Ya Registrados");
                    return; // ✨ IMPORTANTE: Salir sin guardar más métodos
                }

                // ✨ VALIDACIÓN: Verificar que el monto no exceda el total de ventas
                if (TotalSalesAmount > 0 && PaymentAmountDecimal > 0)
                {
                    var totalAfterAddition = TotalPaymentMethodsAmount + PaymentAmountDecimal;
                    if (totalAfterAddition > TotalSalesAmount)
                    {
                        var excedente = totalAfterAddition - TotalSalesAmount;
                        await CustomAlert.ShowWarningAsync(
                            $"⚠️ Monto Excedente\n\n" +
                            $"El monto que intenta agregar excede el total de ventas:\n\n" +
                            $"• Total de ventas: ${TotalSalesAmount:N2}\n" +
                            $"• Pagos actuales: ${TotalPaymentMethodsAmount:N2}\n" +
                            $"• Monto a agregar: ${PaymentAmountDecimal:N2}\n" +
                            $"• Excedente: ${excedente:N2}\n\n" +
                            $"El monto máximo que puede agregar es: ${TotalSalesAmount - TotalPaymentMethodsAmount:N2}",
                            "Monto Excede Total");
                        return;
                    }
                }

                // Validar que se haya ingresado un monto
                if (PaymentAmountDecimal <= 0)
                {
                    await CustomAlert.ShowErrorAsync(
                        "Debe ingresar un monto válido para el método de pago.\n\n" +
                        "El monto debe ser mayor a cero.",
                        "Monto Requerido");
                    return;
                }

                // Create comprehensive payment method description including amount
                var fullDescription = $"[{PaymentName}] Tipo: {SelectedPaymentType}, Metodo: {SelectedPaymentMethod}, Proveedor: {PaymentProvider}, Monto: ${PaymentAmountDecimal:N2}, Comision: {ProcessingFee}%, Estado: {SelectedStatus} - {Description}";

                TypeOfCollection = new TypeOfCollectionModel
                {
                    Description = fullDescription
                };

                Request = new TypeOfCollectionRequest
                {
                    Request = TypeOfCollection
                };

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var json = JsonSerializer.Serialize(Request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/type-of-collection", content);

                if (response.IsSuccessStatusCode)
                {
                    // ✨ CREAR Y AGREGAR el método con el monto específico a la lista local
                    var newPaymentMethod = new PaymentMethodItem
                    {
                        Id = PaymentMethodsList.Count + 1, // Temporal ID
                        Name = PaymentName,
                        Type = SelectedPaymentType,
                        Provider = PaymentProvider,
                        ProcessingFee = decimal.TryParse(ProcessingFee, out decimal fee) ? fee : 0,
                        Status = SelectedStatus,
                        RequiresAuth = RequiresAuth,
                        IsDefault = IsDefault,
                        Description = Description,
                        Icon = GetPaymentIcon(SelectedPaymentType),
                        Amount = PaymentAmountDecimal // ✨ IMPORTANTE: Asignar el monto específico
                    };

                    PaymentMethodsList.Add(newPaymentMethod);

                    await CustomAlert.ShowSuccessAsync(
                        $"Método de pago registrado exitosamente:\n\n" +
                        $"• Nombre: {PaymentName}\n" +
                        $"• Tipo: {SelectedPaymentType}\n" +
                        $"• Método: {SelectedPaymentMethod}\n" +
                        $"• Monto: ${PaymentAmountDecimal:N2}\n" +
                        $"• Proveedor: {PaymentProvider}\n" +
                        $"• Estado: {SelectedStatus}", 
                        "Método de Pago Registrado");

                    // Reload payment methods from API to get the latest data
                    await LoadPaymentMethodsAsync();
                    UpdateStatistics();
                    
                    // ✨ VALIDACIÓN AUTOMÁTICA después de guardar
                    await ValidatePaymentCompletionAsync();

                    // ⬇️⬇️ Resetea el “Total del Día” a cero
                    TotalSalesAmount = 0m;

                    // Clear form fields
                    PaymentName = string.Empty;
                    SelectedPaymentType = string.Empty;
                    SelectedPaymentMethod = string.Empty;
                    PaymentProvider = string.Empty;
                    ProcessingFee = string.Empty;
                    PaymentAmount = string.Empty; // ✨ Limpiar también el monto
                    SelectedStatus = string.Empty;
                    Description = string.Empty;
                    RequiresAuth = false;
                    IsDefault = false;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await CustomAlert.ShowErrorAsync(
                        $"No se pudo registrar la forma de pago: {response.StatusCode}\n{error}", 
                        "Error del Servidor");
                }
            }
            catch (Exception ex)
            {
                await CustomAlert.ShowErrorAsync(
                    $"Error al registrar la forma de pago: {ex.Message}", 
                    "Error del Sistema");
            }
        }

        public async Task LoadTranslationsAsync()
        {
            try
            {
                var result = await GetTranslationsByLanguageAsync("es-CO");
                GlobalTranslations.SetTranslations(result ?? []);
                DescriptionTitle = GlobalTranslations.Get("DescriptionTitle");
                DescriptionLabel = GlobalTranslations.Get("DescriptionLabel");
                DescriptionPlaceHolder = GlobalTranslations.Get("DescriptionPlaceHolder");
                SendData = GlobalTranslations.Get("SendData");
                ErrorEmpty = GlobalTranslations.Get("ErrorEmpty");
                ErrorCharacteres = GlobalTranslations.Get("ErrorCharacteres");
            }
            catch (Exception ex)
            {
                // Fallback to default values if translation fails
                Console.WriteLine($"Translation error: {ex.Message}");
            }
        }

        // Sales and Payment Validation Properties
        private decimal _totalSalesAmount;
        public decimal TotalSalesAmount
        {
            get => _totalSalesAmount;
            set
            {
                _totalSalesAmount = value;
                OnPropertyChanged(nameof(TotalSalesAmount));
                OnPropertyChanged(nameof(RemainingAmount));
                OnPropertyChanged(nameof(IsPaymentComplete));
            }
        }

        private decimal _totalPaymentMethodsAmount;
        public decimal TotalPaymentMethodsAmount
        {
            get => _totalPaymentMethodsAmount;
            set
            {
                _totalPaymentMethodsAmount = value;
                OnPropertyChanged(nameof(TotalPaymentMethodsAmount));
                OnPropertyChanged(nameof(RemainingAmount));
                OnPropertyChanged(nameof(IsPaymentComplete));
            }
        }

        public decimal RemainingAmount => TotalSalesAmount - TotalPaymentMethodsAmount;

        public bool IsPaymentComplete => Math.Abs(RemainingAmount) < 0.01m; // Using small tolerance for decimal comparison

        private bool _showPaymentMethodsPopup;
        public bool ShowPaymentMethodsPopup
        {
            get => _showPaymentMethodsPopup;
            set
            {
                _showPaymentMethodsPopup = value;
                OnPropertyChanged(nameof(ShowPaymentMethodsPopup));
            }
        }

        private void UpdateStatistics()
        {
            TotalPaymentMethods = PaymentMethodsList.Count;
            ActivePaymentMethods = PaymentMethodsList.Count(x => x.Status == "Activo");
            DigitalPaymentMethods = PaymentMethodsList.Count(x => x.Type.Contains("Digital") || x.Type.Contains("Electronico") || x.Type.Contains("Transferencia"));
            CashPaymentMethods = PaymentMethodsList.Count(x => x.Type.Contains("Efectivo"));
        }

        private string GetPaymentIcon(string paymentType)
        {
            return paymentType switch
            {
                "Efectivo" => "💵",
                "Tarjeta de Debito" => "💳",
                "Tarjeta de Credito" => "💎",
                "Transferencia Bancaria" => "🏦",
                "Pago Electronico" => "🌐",
                "Billetera Digital" => "📱",
                "Criptomoneda" => "₿",
                "Cheque" => "📝",
                _ => "💳"
            };
        }

        private async Task ValidatePaymentCompletionAsync()
        {
            try
            {
                // Recalculate total payment methods amount
                CalculateTotalPaymentMethodsAmount();

                if (IsPaymentComplete)
                {
                    await CustomAlert.ShowSuccessAsync(
                        $"¡Felicitaciones!\n\n" +
                        $"Se han agregado todas las formas de pago necesarias:\n\n" +
                        $"• Total de ventas: ${TotalSalesAmount:N2}\n" +
                        $"• Total métodos de pago: ${TotalPaymentMethodsAmount:N2}\n" +
                        $"• Diferencia: ${RemainingAmount:N2}\n\n" +
                        $"La totalidad de los pagos ha sido registrada correctamente.",
                        "Pagos Completos");

                    // Show payment methods popup after validation
                    await ShowPaymentMethodsPopupAsync();
                }
                else if (TotalPaymentMethodsAmount > TotalSalesAmount)
                {
                    await CustomAlert.ShowWarningAsync(
                        $"⚠️ Atención\n\n" +
                        $"El total de métodos de pago excede las ventas:\n\n" +
                        $"• Total de ventas: ${TotalSalesAmount:N2}\n" +
                        $"• Total métodos de pago: ${TotalPaymentMethodsAmount:N2}\n" +
                        $"• Excedente: ${Math.Abs(RemainingAmount):N2}\n\n" +
                        $"Por favor, revise los montos ingresados.",
                        "Exceso en Pagos");
                }
                else
                {
                    await CustomAlert.ShowInfoAsync(
                        $"📋 Estado de Pagos\n\n" +
                        $"Aún faltan métodos de pago por agregar:\n\n" +
                        $"• Total de ventas: ${TotalSalesAmount:N2}\n" +
                        $"• Total métodos de pago: ${TotalPaymentMethodsAmount:N2}\n" +
                        $"• Pendiente: ${RemainingAmount:N2}\n\n" +
                        $"Continue agregando formas de pago para completar el total.",
                        "Pagos Pendientes");
                }
            }
            catch (Exception ex)
            {
                await CustomAlert.ShowErrorAsync(
                    $"Error al validar la completitud de pagos:\n\n{ex.Message}",
                    "Error de Validación");
            }
        }

        public async Task ShowPaymentMethodsPopupAsync()
        {
            try
            {
                ShowPaymentMethodsPopup = true;
                
                // Here you can implement the logic to show the actual popup
                // This could be navigation to a popup page or showing a modal
                await Application.Current.MainPage.DisplayAlert(
                    "Métodos de Pago",
                    $"Mostrando popup con {PaymentMethodsList.Count} métodos de pago disponibles.\n\n" +
                    $"• Total configurado: ${TotalPaymentMethodsAmount:N2}\n" +
                    $"• Métodos activos: {ActivePaymentMethods}\n" +
                    $"• Métodos digitales: {DigitalPaymentMethods}",
                    "Cerrar");
                
                ShowPaymentMethodsPopup = false;
            }
            catch (Exception ex)
            {
                await CustomAlert.ShowErrorAsync(
                    $"Error al mostrar popup de métodos de pago:\n\n{ex.Message}",
                    "Error del Sistema");
            }
        }

        private void CalculateTotalPaymentMethodsAmount()
        {
            try
            {
                // Calculate total based on the Amount property of active payment methods
                decimal total = 0;

                foreach (var method in PaymentMethodsList.Where(x => x.Status == "Activo"))
                {
                    // Use Amount property if available, otherwise fall back to ProcessingFee
                    total += method.HasAmount ? method.Amount : method.ProcessingFee;
                }

                TotalPaymentMethodsAmount = total;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating total payment methods amount: {ex.Message}");
                TotalPaymentMethodsAmount = 0;
            }
        }

        public void SetSalesTotal(decimal salesAmount)
        {
            TotalSalesAmount = salesAmount;
            CalculateTotalPaymentMethodsAmount();
        }

        // Method to update sales total from court service or external source
        public async Task UpdateSalesTotalFromSourceAsync()
        {
            try
            {
                // Here you can implement logic to get sales total from court service or other source
                // For now, this is a placeholder method
                
                // Example: Get total from court service if available
                // var courtService = DependencyService.Get<ICourtService>();
                // if (courtService != null)
                // {
                //     TotalSalesAmount = await courtService.GetTotalSalesAmountAsync();
                // }
                
                CalculateTotalPaymentMethodsAmount();
                
                // Auto-validate after updating totals
                if (TotalSalesAmount > 0)
                {
                    await ValidatePaymentCompletionAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating sales total: {ex.Message}");
                await CustomAlert.ShowErrorAsync(
                    $"Error al actualizar el total de ventas:\n\n{ex.Message}",
                    "Error del Sistema");
            }
        }

        // Method to manually trigger validation
        public async Task TriggerPaymentValidationAsync()
        {
            try
            {
                CalculateTotalPaymentMethodsAmount();
                await ValidatePaymentCompletionAsync();
            }
            catch (Exception ex)
            {
                await CustomAlert.ShowErrorAsync(
                    $"Error al validar pagos:\n\n{ex.Message}",
                    "Error de Validación");
            }
        }

        public async Task AddPaymentMethodWithAmountAsync(PaymentMethodItem method, decimal amount)
        {
            try
            {
                // Set the specific amount for this payment method
                method.Amount = amount;
                PaymentMethodsList.Add(method);
                
                // Recalculate totals
                UpdateStatistics();
                CalculateTotalPaymentMethodsAmount();
                
                // Auto-validate if payment is complete
                if (IsPaymentComplete)
                {
                    await ValidatePaymentCompletionAsync();
                }
            }
            catch (Exception ex)
            {
                await CustomAlert.ShowErrorAsync(
                    $"Error al agregar método de pago:\n\n{ex.Message}",
                    "Error del Sistema");
            }
        }

        // Comando para verificar antes de agregar un método de pago
        private async Task CheckBeforeAddPaymentMethodAsync()
        {
            try
            {
                // Calcular el monto total de los métodos de pago activos
                CalculateTotalPaymentMethodsAmount();

                if (IsPaymentComplete)
                {
                    await CustomAlert.ShowSuccessAsync(
                        $"✅ Verificación Exitosa\n\n" +
                        $"Todos los métodos de pago necesarios ya están registrados.\n\n" +
                        $"• Total de ventas: ${TotalSalesAmount:N2}\n" +
                        $"• Total pagos registrados: ${TotalPaymentMethodsAmount:N2}\n\n" +
                        $"No es necesario agregar más métodos de pago.",
                        "Información");
                }
                else
                {
                    await CustomAlert.ShowWarningAsync(
                        $"⚠️ Atención Requerida\n\n" +
                        $"Aún faltan métodos de pago por agregar:\n\n" +
                        $"• Total de ventas: ${TotalSalesAmount:N2}\n" +
                        $"• Total métodos de pago: ${TotalPaymentMethodsAmount:N2}\n" +
                        $"• Pendiente: ${RemainingAmount:N2}\n\n" +
                        $"Por favor, continúe agregando formas de pago.",
                        "Verificación Pendiente");
                }
            }
            catch (Exception ex)
            {
                await CustomAlert.ShowErrorAsync(
                    $"Error al verificar métodos de pago:\n\n{ex.Message}",
                    "Error de Validación");
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // API response models
    public class TypeOfCollectionApiResponse
    {
        public List<TypeOfCollectionData> Data { get; set; } = new();
    }

    public class TypeOfCollectionData
    {
        public int IdTypeOfCollection { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class TranslationsResponse
    {
        public Dictionary<string, Dictionary<string, string>> Translations { get; set; } = new();
    }
}

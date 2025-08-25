using APP.Eds.Models.TypeOfCollection;
using System.ComponentModel;
using System.Text.Json;
using System.Text;
using System.Windows.Input;
using APP.Eds.Services.Config;
using APP.Eds.Helpers;
using System.Net.Http.Headers;
using APP.Eds.Models.Translations;
using System.Collections.ObjectModel;

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
            try
            {
                // Create sample payment methods
                // In real implementation, this would come from the API
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
                await Application.Current.MainPage.DisplayAlert("Error", $"Error cargando formas de pago: {ex.Message}", "OK");
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

        private void UpdateStatistics()
        {
            TotalPaymentMethods = PaymentMethodsList.Count;
            ActivePaymentMethods = PaymentMethodsList.Count(x => x.Status == "Activo");
            DigitalPaymentMethods = PaymentMethodsList.Count(x => x.Type.Contains("Digital") || x.Type.Contains("Electronico") || x.Type.Contains("Transferencia"));
            CashPaymentMethods = PaymentMethodsList.Count(x => x.Type.Contains("Efectivo"));
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
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return;
            }
            try
            {
                // Create comprehensive payment method description
                var fullDescription = $"[{PaymentName}] Tipo: {SelectedPaymentType}, Metodo: {SelectedPaymentMethod}, Proveedor: {PaymentProvider}, Comision: {ProcessingFee}%, Estado: {SelectedStatus} - {Description}";

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
                    await Application.Current.MainPage.DisplayAlert("Exito", "Forma de pago registrada correctamente", "OK");
                    
                    // Add to local list
                    var icon = GetPaymentIcon(SelectedPaymentType);
                    decimal.TryParse(ProcessingFee, out decimal fee);
                    
                    var newMethod = new PaymentMethodItem
                    {
                        Id = PaymentMethodsList.Count + 1,
                        Name = PaymentName,
                        Type = SelectedPaymentType,
                        Provider = PaymentProvider,
                        ProcessingFee = fee,
                        Status = SelectedStatus,
                        RequiresAuth = RequiresAuth,
                        IsDefault = IsDefault,
                        Description = Description,
                        Icon = icon
                    };
                    
                    PaymentMethodsList.Insert(0, newMethod);
                    UpdateStatistics();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await Application.Current.MainPage.DisplayAlert("Error", 
                        $"No se pudo registrar la forma de pago: {response.StatusCode}\n{error}", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", 
                    $"Error al registrar la forma de pago: {ex.Message}", "OK");
            }
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

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

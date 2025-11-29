using APP.Eds.Helpers;
using APP.Eds.Models.Category;
using APP.Eds.Models.Translations;
using APP.Eds.Services.Config;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows.Input;

namespace APP.Eds.Services.Category
{
    public class CategoryService : INotifyPropertyChanged
    {
        private string? _authToken;
        public event PropertyChangedEventHandler? PropertyChanged;
        private CategoryRequest Request { get; set; }
        private CategoryModel _category;
        public CategoryModel Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged(nameof(Category));
            }
        }

        // Default category options
        public ObservableCollection<string> DefaultCategories { get; set; } = new ObservableCollection<string>
        {
            "Subsidiado",
            "Nacional",
            "Personalizado..." // Option to add custom category
        };

        private string _selectedDefaultCategory;
        public string SelectedDefaultCategory
        {
            get => _selectedDefaultCategory;
            set
            {
                _selectedDefaultCategory = value;
                OnPropertyChanged(nameof(SelectedDefaultCategory));
                OnPropertyChanged(nameof(IsCustomCategoryVisible));
                
                // If not "Personalizado", set as description
                if (value != null && value != "Personalizado...")
                {
                    Description = value;
                }
            }
        }

        // Property to control visibility of custom input
        public bool IsCustomCategoryVisible => SelectedDefaultCategory == "Personalizado...";

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
        public ICommand GetByIdCategoryDataCommand { get; }
        public ICommand SaveCategoryDataCommand { get; }


        private string _CategoryTranslation = string.Empty;
        public string CategoryTranslation
        {
            get => _CategoryTranslation;
            set
            {
                if (_CategoryTranslation != value)
                {
                    _CategoryTranslation = value;
                    OnPropertyChanged(nameof(CategoryTranslation));
                }
            }
        }

        private string _SendData = string.Empty;
        public string SendData
        {
            get => _SendData;
            set
            {
                if (_SendData != value)
                {
                    _SendData = value;
                    OnPropertyChanged(nameof(SendData));
                }
            }
        }

        private string _EnterCategory = string.Empty;

        public string EnterCategory
        {
            get => _EnterCategory;
            set
            {
                if (_EnterCategory != value)
                {
                    _EnterCategory = value;
                    OnPropertyChanged(nameof(EnterCategory));
                }
            }
        }    

        private string _CategoryManagement = string.Empty;

        public string CategoryManagement
        {
            get => _CategoryManagement;
            set
            {
                if (_CategoryManagement != value)
                {
                    _CategoryManagement = value;
                    OnPropertyChanged(nameof(CategoryManagement));
                }
            }
        }
        public  CategoryService()
        {
            _authToken = TokenHelper.LoadToken();
            GetByIdCategoryDataCommand = new Command<int>(async (CategoryId) => await GetByIdDispenserTypeDataAsync(CategoryId));
            SaveCategoryDataCommand = new Command(async () => await SaveCategoryDataAsync());
            LoadTranslationsAsync();
        }
        public async Task LoadTranslationsAsync()
        {
            var result = await GetTranslationsByLanguageAsync("es-CO");
            GlobalTranslations.SetTranslations(result ?? []);
            CategoryTranslation = GlobalTranslations.Get("Category");
            SendData = GlobalTranslations.Get("SendData");
            EnterCategory = GlobalTranslations.Get("EnterCategory");
            CategoryManagement = GlobalTranslations.Get("CategoryManagement");

        }
        public async Task<Dictionary<string, string>> GetTranslationsByLanguageAsync(string languageTag)
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return new Dictionary<string, string>();
            }
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/translations");
            var data = JsonSerializer.Deserialize<TranslationsResponse>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return data.Translations.TryGetValue(languageTag, out var translations)
                ? translations
                : new Dictionary<string, string>();

        }
        public async Task GetByIdDispenserTypeDataAsync(int CategoryId)
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
                var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/category/{CategoryId}");
                Console.WriteLine(response);

                Category = JsonSerializer.Deserialize<CategoryModel>(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar el dato: {ex.Message}", "OK");
            }
        }

        public async Task<bool> SaveCategoryDataAsync()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return false;
            }
            try
            {
                // Validate before making the request
                if (string.IsNullOrWhiteSpace(Description))
                {
                    await Application.Current.MainPage.DisplayAlert("Error de Validación", "Debe ingresar los datos en el campo categoría.", "Aceptar");
                    return false; 
                }

                Category = new CategoryModel
                {
                    Description = Description
                };

                Request = new CategoryRequest
                {
                    Request = Category
                };

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var json = JsonSerializer.Serialize(Request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/category", content);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var serverError = await response.Content.ReadAsStringAsync();
                    
                    // Debug logging
                    System.Diagnostics.Debug.WriteLine($"Error Status Code: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"Error Response: {serverError}");
                    
                    var userFriendlyError = TranslateCategoryError(serverError, Description, response.StatusCode);
                    await Application.Current.MainPage.DisplayAlert("Error", userFriendlyError, "OK");
                    return false;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error al enviar los datos: {ex.Message}", "OK");
                return false;
            }
        }

        private string TranslateCategoryError(string serverError, string categoryName, System.Net.HttpStatusCode statusCode)
        {
            // If server error is empty but we have a 500 status, assume it's likely a duplicate error
            if (string.IsNullOrEmpty(serverError))
            {
                // Check if it's a 500 error which is common for constraint violations
                if (statusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    return $"⚠️ Categoría Ya Existente\n\n" +
                           $"Ya existe una categoría con el nombre '{categoryName}' en el sistema.\n\n" +
                           $"🚫 No se pueden registrar categorías duplicadas\n\n" +
                           $"💡 Soluciones disponibles:\n\n" +
                           $"✅ Cambiar el nombre de la categoría:\n" +
                           $"   • Usar una variación del nombre\n" +
                           $"   • Agregar un número o identificador\n" +
                           $"   • Ejemplo: '{categoryName} 2' o '{categoryName} Especial'\n\n" +
                           $"✅ Verificar categorías existentes:\n" +
                           $"   • La categoría podría ya estar registrada\n" +
                           $"   • Revisar la lista de categorías actuales\n\n" +
                           $"📞 Si necesita ayuda, contacte al soporte técnico.";
                }
                
                return "❌ Error Desconocido\n\nNo se pudo guardar la categoría. El servidor no proporcionó detalles del error.\n\nPor favor, intente de nuevo más tarde.";
            }

            var errorLower = serverError.ToLowerInvariant();

            // Detectar errores de categoría duplicada
            if (errorLower.Contains("duplicate") ||
                errorLower.Contains("already exists") ||
                errorLower.Contains("unique constraint") ||
                errorLower.Contains("duplicado") ||
                errorLower.Contains("ya existe") ||
                errorLower.Contains("category_unique") ||
                errorLower.Contains("violates unique constraint") ||
                errorLower.Contains("duplicate key") ||
                errorLower.Contains("unique key constraint") ||
                errorLower.Contains("cannot insert duplicate key") ||
                errorLower.Contains("duplicate entry") ||
                errorLower.Contains("constraint violation") ||
                errorLower.Contains("ix_") ||  // Índices únicos SQL Server
                errorLower.Contains("uc_") ||  // Unique constraints
                errorLower.Contains("pk_") ||  // Primary key violations
                errorLower.Contains("23505") || // PostgreSQL unique violation
                errorLower.Contains("2627") ||  // SQL Server unique constraint violation
                errorLower.Contains("1062") ||  // MySQL duplicate entry
                (errorLower.Contains("status") && errorLower.Contains("500") && errorLower.Contains("processing")) ||
                (errorLower.Contains("internalservererror") && (errorLower.Contains("category") || errorLower.Contains("description"))))
            {
                return $"⚠️ Categoría Ya Existente\n\n" +
                       $"Ya existe una categoría con el nombre '{categoryName}' en el sistema.\n\n" +
                       $"🚫 No se pueden registrar categorías duplicadas\n\n" +
                       $"💡 Soluciones disponibles:\n\n" +
                       $"✅ Cambiar el nombre de la categoría:\n" +
                       $"   • Usar una variación del nombre\n" +
                       $"   • Agregar un número o identificador\n" +
                       $"   • Ejemplo: '{categoryName} 2' o '{categoryName} Especial'\n\n" +
                       $"✅ Verificar categorías existentes:\n" +
                       $"   • La categoría podría ya estar registrada\n" +
                       $"   • Revisar la lista de categorías actuales\n\n" +
                       $"📞 Si necesita ayuda, contacte al soporte técnico.";
            }

            // Detectar errores de campos requeridos
            if (errorLower.Contains("description") && (errorLower.Contains("required") || errorLower.Contains("null")))
            {
                return "📝 Descripción Obligatoria\n\n" +
                       "La descripción de la categoría es obligatoria y no puede estar vacía.\n\n" +
                       "Por favor, seleccione una categoría del menú o ingrese un nombre personalizado.";
            }

            // Detectar errores de validación
            if (errorLower.Contains("validation") || errorLower.Contains("invalid"))
            {
                return $"❌ Error de Validación\n\n" +
                       $"La categoría no cumple con los requisitos del sistema:\n\n" +
                       $"✅ Requisitos:\n" +
                       $"• Descripción: obligatoria, entre 3 y 50 caracteres\n" +
                       $"• Solo letras y espacios permitidos\n\n" +
                       $"💡 Ejemplo de categoría válida: '{categoryName}'\n\n" +
                       $"🔧 Para soporte técnico, detalle del error:\n" +
                       $"'{serverError.Substring(0, Math.Min(serverError.Length, 150))}'";
            }

            // Detectar errores de autorización
            if (errorLower.Contains("unauthorized") || errorLower.Contains("forbidden") || errorLower.Contains("401") || errorLower.Contains("403"))
            {
                return "🔐 Sin Autorización\n\n" +
                       "No tiene permisos suficientes para registrar categorías.\n\n" +
                       "Contacte al administrador del sistema para obtener los permisos necesarios.";
            }

            // Detectar errores de servidor interno (500) genéricos
            if ((errorLower.Contains("internalservererror") || errorLower.Contains("internal server error") ||
                 errorLower.Contains("status") && errorLower.Contains("500")) &&
                !errorLower.Contains("duplicate") && !errorLower.Contains("unique"))
            {
                return $"⚠️ Error del Servidor\n\n" +
                       $"Se produjo un error interno en el servidor al procesar su solicitud.\n\n" +
                       $"🔄 Posibles causas:\n" +
                       $"• Problemas temporales del servidor\n" +
                       $"• Sobrecarga del sistema\n" +
                       $"• Error en el procesamiento de datos\n\n" +
                       $"💡 Recomendaciones:\n" +
                       $"• Espere unos minutos e intente nuevamente\n" +
                       $"• Verifique que la categoría sea válida\n" +
                       $"• Si persiste, contacte al soporte técnico\n\n" +
                       $"🔧 Si necesita ayuda inmediata, proporcione estos detalles al soporte:\n" +
                       $"'{serverError.Substring(0, Math.Min(serverError.Length, 200))}'";
            }

            // Error genérico mejorado para cualquier otro caso
            return $"❗ Error Inesperado\n\n" +
                   $"Se produjo un error al guardar la categoría que no pudimos identificar específicamente.\n\n" +
                   $"🔄 Recomendaciones:\n" +
                   $"• Verifique que la información esté correcta\n" +
                   $"• Intente guardar la categoría nuevamente\n" +
                   $"• Si el error persiste, contacte al soporte técnico\n\n" +
                   $"🔧 Información del error para soporte técnico:\n" +
                   $"'{serverError.Substring(0, Math.Min(serverError.Length, 200))}'\n\n" +
                   $"📞 Al contactar soporte, proporcione esta información junto con:\n" +
                   $"• Nombre de la categoría: '{categoryName}'\n" +
                   $"• Fecha y hora del intento";
        }


        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

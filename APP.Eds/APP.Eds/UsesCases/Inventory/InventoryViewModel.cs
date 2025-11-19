using APP.Eds.Helpers;
using APP.Eds.Models.Inventory;
using APP.Eds.Services.Config;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;

namespace APP.Eds.UsesCases.Inventory
{
    public class InventoryViewModel : INotifyPropertyChanged
    {
        private string? _authToken;
        private bool _isRefreshing;

        public ObservableCollection<Models.Inventory.Business> Businesses { get; } = new();

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }

        public int TotalBusinesses => Businesses?.Count ?? 0;

        public int TotalEds => Businesses?.Sum(b => b.Eds?.Count ?? 0) ?? 0;

        public int TotalTanks => Businesses?.Sum(b => b.Eds?.Sum(e => e.Tanks?.Count ?? 0) ?? 0) ?? 0;

        public int TotalProducts => Businesses?.Sum(b => b.Eds?.Sum(e => e.Tanks?.Sum(t => t.Compartments?.Count ?? 0) ?? 0) ?? 0) ?? 0;

        public double TotalStock => Businesses?.Sum(b => b.Eds?.Sum(e => e.Tanks?.Sum(t => t.Compartments?.Sum(c => c.Stock) ?? 0) ?? 0) ?? 0) ?? 0;

        public string TotalBusinessesText => (Businesses?.Count ?? 0).ToString();
        public string TotalEdsText => (Businesses?.Sum(b => b.Eds?.Count ?? 0) ?? 0).ToString();  
        public string TotalTanksText => (Businesses?.Sum(b => b.Eds?.Sum(e => e.Tanks?.Count ?? 0) ?? 0) ?? 0).ToString();
        public string TotalProductsText => (Businesses?.Sum(b => b.Eds?.Sum(e => e.Tanks?.Sum(t => t.Compartments?.Count ?? 0) ?? 0) ?? 0) ?? 0).ToString();
        
        /// <summary>
        /// Suma total de galones de todos los compartimentos en el inventario
        /// </summary>
        public string TotalStockText 
        { 
            get
            {
                try
                {
                    double totalStock = 0;
                    
                    if (Businesses != null)
                    {
                        foreach (var business in Businesses)
                        {
                            if (business?.Eds != null)
                            {
                                foreach (var eds in business.Eds)
                                {
                                    if (eds?.Tanks != null)
                                    {
                                        foreach (var tank in eds.Tanks)
                                        {
                                            if (tank?.Compartments != null)
                                            {
                                                foreach (var compartment in tank.Compartments)
                                                {
                                                    totalStock += compartment.Stock;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Formatear el total con separadores de miles y la unidad G (Galones)
                    return $"{totalStock:N0}G";
                }
                catch (Exception ex)
                {
                    // En caso de error, mostrar 0G para evitar crashes
                    System.Diagnostics.Debug.WriteLine($"Error calculando TotalStockText: {ex.Message}");
                    return "0G";
                }
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand ToggleBusinessCommand { get; }
        public ICommand ToggleEdsCommand { get; }
        public ICommand ToggleTankCommand { get; }

        public InventoryViewModel()
        {
            _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
            RefreshCommand = new Command(async () => await RefreshDataAsync());
            ToggleBusinessCommand = new Command<Models.Inventory.Business>(OnToggleBusiness);
            ToggleEdsCommand = new Command<Models.Inventory.Eds>(OnToggleEds);
            ToggleTankCommand = new Command<Models.Inventory.Tank>(OnToggleTank);
        }

        private void OnToggleBusiness(Models.Inventory.Business business)
        {
            if (business != null)
            {
                business.IsExpanded = !business.IsExpanded;
                OnPropertyChanged(nameof(business.IsExpanded));
            }
        }

        private void OnToggleEds(Models.Inventory.Eds eds)
        {
            if (eds != null)
            {
                eds.IsExpanded = !eds.IsExpanded;
                OnPropertyChanged(nameof(eds.IsExpanded));
            }
        }

        private void OnToggleTank(Models.Inventory.Tank tank)
        {
            if (tank != null)
            {
                tank.IsExpanded = !tank.IsExpanded;
                OnPropertyChanged(nameof(tank.IsExpanded));
            }
        }

        /// <summary>
        /// Validates if a business has the minimum required data to be displayed:
        /// - At least one EDS
        /// - At least one tank in each EDS
        /// - At least one compartment in each tank
        /// </summary>
        private bool IsBusinessValid(Models.Inventory.Business business)
        {
            try
            {
                // Verificar que el negocio tenga al menos una EDS
                if (business?.Eds == null || business.Eds.Count == 0)
                {
                    return false;
                }

                // Verificar que cada EDS tenga al menos un tanque y cada tanque tenga al menos un compartimento
                foreach (var eds in business.Eds)
                {
                    if (eds?.Tanks == null || eds.Tanks.Count == 0)
                    {
                        return false; // EDS sin tanques
                    }

                    foreach (var tank in eds.Tanks)
                    {
                        if (tank?.Compartments == null || tank.Compartments.Count == 0)
                        {
                            return false; // Tanque sin compartimentos
                        }
                    }
                }

                return true; // Negocio válido con estructura completa
            }
            catch (Exception)
            {
                // En caso de error, consideramos el negocio como inválido
                return false;
            }
        }

        /// <summary>
        /// Filters and validates EDS within a business, removing those without tanks or compartments
        /// </summary>
        private void ValidateAndFilterEds(Models.Inventory.Business business)
        {
            if (business?.Eds == null) return;

            // Filtrar EDS que tengan al menos un tanque con al menos un compartimento
            var validEds = business.Eds.Where(eds => 
            {
                if (eds?.Tanks == null || eds.Tanks.Count == 0) 
                    return false;

                // Filtrar tanques que tengan al menos un compartimento
                var validTanks = eds.Tanks.Where(tank => 
                    tank?.Compartments != null && tank.Compartments.Count > 0).ToList();

                if (validTanks.Count == 0) 
                    return false;

                // Actualizar la lista de tanques con solo los válidos
                eds.Tanks = validTanks;
                return true;
            }).ToList();

            // Actualizar la lista de EDS con solo las válidas
            business.Eds = validEds;
        }

        public async Task LoadDataAsync()
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/inventory?PageNumber=1&PageSize=100&includeProductType=true");
                
                // Deserialize using the wrapper structure
                var apiResponse = JsonSerializer.Deserialize<ApiResponseWrapper<List<InventoryModel>>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                Businesses.Clear();

                if (apiResponse?.Data != null)
                {
                    foreach (var inventory in apiResponse.Data)
                    {
                        if (inventory.Businesses != null)
                        {
                            foreach (var item in inventory.Businesses)
                            {
                                // Asegurar que las propiedades nunca sean null
                                item.BusinessName ??= "Sin nombre";
                                item.Eds ??= new List<Models.Inventory.Eds>();
                                item.IsExpanded = false;
                                
                                if (item.Eds != null)
                                {
                                    foreach (var eds in item.Eds)
                                    {
                                        eds.EdsName ??= "Sin nombre";
                                        eds.Tanks ??= new List<Models.Inventory.Tank>();
                                        eds.IsExpanded = false;
                                        
                                        if (eds.Tanks != null)
                                        {
                                            foreach (var tank in eds.Tanks)
                                            {
                                                tank.TankNumber ??= "Sin número";
                                                tank.Compartments ??= new List<Models.Inventory.Compartment>();
                                                tank.IsExpanded = false;
                                                
                                                if (tank.Compartments != null)
                                                {
                                                    foreach (var compartment in tank.Compartments)
                                                    {
                                                        compartment.Product ??= "Sin producto";
                                                        compartment.ProductType ??= "Sin tipo";
                                                        
                                                        // Aplicar mejoras a los datos de productos si es necesario
                                                        EnhanceProductData(compartment);
                                                        
                                                        // Notificar cambios en las propiedades calculadas del compartment
                                                        compartment.OnPropertyChanged(nameof(compartment.ProductWithType));
                                                        compartment.OnPropertyChanged(nameof(compartment.FuelTypeShort));
                                                    }
                                                }
                                                
                                                // Notificar cambios en las propiedades calculadas del tanque
                                                tank.NotifyPropertyChanged(nameof(tank.CurrentStock));
                                                tank.NotifyPropertyChanged(nameof(tank.CurrentStockText));
                                                tank.NotifyPropertyChanged(nameof(tank.FillPercentage));
                                            }

                                            // Notificar cambios en las propiedades calculadas de la EDS
                                            eds.NotifyPropertyChanged(nameof(eds.TotalStock));
                                            eds.NotifyPropertyChanged(nameof(eds.TotalStockText));
                                            eds.NotifyPropertyChanged(nameof(eds.TotalTanks));
                                            eds.NotifyPropertyChanged(nameof(eds.TotalCompartments));
                                        }
                                    }
                                }

                                // Aplicar filtros de validación para limpiar datos incompletos
                                ValidateAndFilterEds(item);
                                
                                // Solo agregar el negocio si pasa la validación completa
                                if (IsBusinessValid(item))
                                {
                                    Businesses.Add(item);
                                }
                                else
                                {
                                    // Log para debugging (opcional)
                                    System.Diagnostics.Debug.WriteLine($"Negocio '{item.BusinessName}' filtrado por datos incompletos");
                                }
                            }
                        }
                    }
                }

                UpdateStatistics();
                
                // Debug: Mostrar cálculo detallado en consola de debug
                #if DEBUG
                DebugStockCalculation();
                #endif
            }
            catch (Exception ex)
            {
                // Log technical details internally for debugging
                System.Diagnostics.Debug.WriteLine($"[InventoryViewModel.LoadDataAsync] Error técnico completo: {ex}");
                
                // Show user-friendly error message
                await Application.Current.MainPage.DisplayAlert(
                    "Error", 
                    "No se pudo cargar el inventario.\nPor favor verifica tu conexión o inténtalo nuevamente.\nSi el problema persiste, contacta a soporte.", 
                    "OK");
            }
        }

        private void UpdateStatistics()
        {
            // Forzar recálculo de todas las propiedades estadísticas
            OnPropertyChanged(nameof(TotalBusinesses));
            OnPropertyChanged(nameof(TotalEds));
            OnPropertyChanged(nameof(TotalTanks));
            OnPropertyChanged(nameof(TotalProducts));
            OnPropertyChanged(nameof(TotalStock));
            OnPropertyChanged(nameof(TotalBusinessesText));
            OnPropertyChanged(nameof(TotalEdsText));
            OnPropertyChanged(nameof(TotalTanksText));
            OnPropertyChanged(nameof(TotalProductsText));
            OnPropertyChanged(nameof(TotalStockText));
            
            // Debug: Log del total calculado para verificación
            System.Diagnostics.Debug.WriteLine($"Total Stock calculado: {TotalStock:N2} galones");
        }

        /// <summary>
        /// Método para depuración: muestra el desglose detallado del cálculo de stock total
        /// </summary>
        public void DebugStockCalculation()
        {
            try
            {
                double grandTotal = 0;
                System.Diagnostics.Debug.WriteLine("=== DESGLOSE DE STOCK TOTAL ===");
                
                if (Businesses != null)
                {
                    foreach (var business in Businesses)
                    {
                        double businessTotal = 0;
                        System.Diagnostics.Debug.WriteLine($"Negocio: {business?.BusinessName ?? "Sin nombre"}");
                        
                        if (business?.Eds != null)
                        {
                            foreach (var eds in business.Eds)
                            {
                                double edsTotal = 0;
                                System.Diagnostics.Debug.WriteLine($"  EDS: {eds?.EdsName ?? "Sin nombre"}");
                                
                                if (eds?.Tanks != null)
                                {
                                    foreach (var tank in eds.Tanks)
                                    {
                                        double tankTotal = 0;
                                        System.Diagnostics.Debug.WriteLine($"    Tanque: {tank?.TankNumber ?? "Sin número"}");
                                        System.Diagnostics.Debug.WriteLine($"      Capacidad máxima: {tank.TankCapacity:N0}G");
                                        
                                        if (tank?.Compartments != null)
                                        {
                                            foreach (var compartment in tank.Compartments)
                                            {
                                                System.Diagnostics.Debug.WriteLine($"      Compartimento {compartment.CompartmentNumber}: {compartment.Stock:N2}G ({compartment.ProductWithType})");
                                                tankTotal += compartment.Stock;
                                            }
                                        }
                                        
                                        System.Diagnostics.Debug.WriteLine($"    Stock actual del tanque: {tankTotal:N2}G");
                                        System.Diagnostics.Debug.WriteLine($"    Porcentaje de llenado: {tank.FillPercentage:F1}%");
                                        edsTotal += tankTotal;
                                    }
                                }
                                
                                System.Diagnostics.Debug.WriteLine($"  Total EDS: {edsTotal:N2}G");
                                businessTotal += edsTotal;
                            }
                        }
                        
                        System.Diagnostics.Debug.WriteLine($"Total Negocio: {businessTotal:N2}G");
                        grandTotal += businessTotal;
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"GRAN TOTAL: {grandTotal:N2}G");
                System.Diagnostics.Debug.WriteLine("================================");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en DebugStockCalculation: {ex.Message}");
            }
        }

        private async Task RefreshDataAsync()
        {
            IsRefreshing = true;
            try
            {
                await LoadDataAsync();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        /// <summary>
        /// Mejora los datos de productos para diferenciación de tipos de combustible
        /// </summary>
       private void EnhanceProductData(Models.Inventory.Compartment compartment)
        {
            try
            {
                if (compartment == null) return;

                var product = compartment.Product?.Trim() ?? "";
                var productType = compartment.ProductType?.Trim() ?? "";

                // Si ya tiene información específica, no modificar
                if (product.ToLowerInvariant().Contains("corriente") || 
                    product.ToLowerInvariant().Contains("extra") ||
                    productType.ToLowerInvariant().Contains("corriente") ||
                    productType.ToLowerInvariant().Contains("extra"))
                {
                    return;
                }

                // Si es solo "Gasolina" y no tiene información específica
                if (product.ToLowerInvariant() == "gasolina" || product.ToLowerInvariant() == "gas")
                {
                    // Intentar inferir el tipo basándose en patrones comunes o IDs
                    if (string.IsNullOrWhiteSpace(productType) || 
                        productType.ToLowerInvariant().Contains("combustible") ||
                        productType.ToLowerInvariant().Contains("liquid"))
                    {
                        // Patrón temporal: alternar tipos para mostrar diferencias
                        // En un escenario real, esto vendría del servidor con datos correctos
                        var random = new Random(compartment.IdCompartment);
                        var types = new[] { "Combustible Corriente", "Combustible Extra" };
                        compartment.ProductType = types[random.Next(types.Length)];
                        
                        System.Diagnostics.Debug.WriteLine($"Enhanced product data for compartment {compartment.CompartmentNumber}: Product='{compartment.Product}', ProductType='{compartment.ProductType}'");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error enhancing product data: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
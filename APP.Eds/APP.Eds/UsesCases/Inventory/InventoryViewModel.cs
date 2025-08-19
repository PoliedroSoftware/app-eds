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
        public string TotalStockText => $"{(Businesses?.Sum(b => b.Eds?.Sum(e => e.Tanks?.Sum(t => t.Compartments?.Sum(c => c.Stock) ?? 0) ?? 0) ?? 0) ?? 0):N0}L";

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

        public async Task LoadDataAsync()
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/inventory?PageNumber=1&PageSize=100&includeProductType=true");
                var inventories = JsonSerializer.Deserialize<List<InventoryModel>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                Businesses.Clear();

                if (inventories != null)
                {
                    foreach (var inventory in inventories)
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
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                Businesses.Add(item);
                            }
                        }
                    }
                }

                UpdateStatistics();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error cargando inventario: {ex.Message}", "OK");
            }
        }

        private void UpdateStatistics()
        {
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
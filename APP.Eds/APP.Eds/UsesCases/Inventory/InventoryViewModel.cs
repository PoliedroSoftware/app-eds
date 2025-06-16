using APP.Eds.Models.Inventory;
using APP.Eds.Services.Config;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;

namespace APP.Eds.UsesCases.Inventory
{
    public class InventoryViewModel
    {
        public ObservableCollection<Models.Inventory.Business> Businesses { get; } = new();

        public ICommand ToggleExpandBusinessCommand { get; }
        public ICommand ToggleExpandEdsCommand { get; }
        public ICommand ToggleExpandTankCommand { get; }
        public ICommand ToggleExpandCompartmentCommand { get; }

        public InventoryViewModel()
        {
            ToggleExpandBusinessCommand = new Command<Models.Inventory.Business>(b =>
            {
                if (b != null)
                    b.IsExpanded = !b.IsExpanded;
            });

            ToggleExpandEdsCommand = new Command<Models.Inventory.Eds>(e =>
            {
                if (e != null)
                    e.IsExpanded = !e.IsExpanded;
            });

            ToggleExpandTankCommand = new Command<Models.Inventory.Tank>(t =>
            {
                if (t != null)
                    t.IsExpanded = !t.IsExpanded;
            });

            ToggleExpandCompartmentCommand = new Command<Compartment>(c =>
            {
                if (c != null)
                    c.IsExpanded = !c.IsExpanded;
            });

            Task.Run(async () => await LoadDataAsync());
        }

        public async Task LoadDataAsync()
        {
            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/inventory?PageNumber=1&PageSize=100");
                var inventories = JsonSerializer.Deserialize<List<InventoryModel>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                Businesses.Clear();

                if (inventories != null)
                {
                    foreach (var inventory in inventories)
                    {
                        foreach (var item in inventory.Businesses)
                        {
                            Businesses.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejar error con logging o UI
                throw;
            }
        }
    }

}
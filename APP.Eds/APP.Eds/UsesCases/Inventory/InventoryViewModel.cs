using APP.Eds.Helpers;
using APP.Eds.Models.Inventory;
using APP.Eds.Services.Config;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Text.Json;

namespace APP.Eds.UsesCases.Inventory
{
    public class InventoryViewModel
    {
        private string? _authToken;
        public ObservableCollection<Models.Inventory.Business> Businesses { get; } = new();

        public InventoryViewModel()
        {
            _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
        }

        public async Task LoadDataAsync()
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
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
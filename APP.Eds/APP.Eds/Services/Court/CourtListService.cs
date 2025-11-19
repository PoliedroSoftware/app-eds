using APP.Eds.Helpers;
using APP.Eds.Models.Court;
using APP.Eds.Services.Config;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;

namespace APP.Eds.Services.Court;

public class CourtListService : INotifyPropertyChanged
{
    private string? _authToken;

    public ObservableCollection<CourtListItemModel> CourtList { get; set; } = new();

    public ICommand OpenCourtDetailCommand => new Command<CourtListItemModel>(OpenCourtDetail);

    public CourtListService()
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
    }

    public async Task LoadAllCourtListAsync()
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

            System.Diagnostics.Debug.WriteLine("CourtListService.LoadAllCourtListAsync - Loading court list from API...");
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/court?PageNumber=1&PageSize=10");

            // Log the raw response for debugging
            System.Diagnostics.Debug.WriteLine($"CourtListService.LoadAllCourtListAsync - API Response: {response.Substring(0, Math.Min(500, response.Length))}...");

            // Deserialize using the wrapper structure
            var apiResponse = JsonSerializer.Deserialize<APP.Eds.Models.Inventory.ApiResponseWrapper<List<CourtListItemModel>>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            CourtList.Clear();

            if (apiResponse?.Data != null)
            {
                System.Diagnostics.Debug.WriteLine($"CourtListService.LoadAllCourtListAsync - Loaded {apiResponse.Data.Count} courts from API");

                foreach (var court in apiResponse.Data)
                {
                    // Simplemente agregar el court tal como viene de la API
                    System.Diagnostics.Debug.WriteLine($"CourtListService - Court {court.Id}: Collections count = {court.Collections?.Count ?? 0}");
                    CourtList.Add(court);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("CourtListService.LoadAllCourtListAsync - No courts returned from API");
            }
        }
        catch (Exception ex)
        {
            // Log technical details internally for debugging
            System.Diagnostics.Debug.WriteLine($"[CourtListService.LoadAllCourtListAsync] Error técnico completo: {ex}");
            
            // Show user-friendly error message
            await Application.Current.MainPage.DisplayAlert(
                "Error", 
                "No se pudo cargar el historial de cortes.\nInténtalo nuevamente. Si continúa el error, comunícate con soporte.", 
                "OK");
        }
    }

    /// <summary>
    /// Valida la consistencia de datos entre las ventas y las formas de pago
    /// </summary>
    public void ValidateCourtCollectionsConsistency(CourtListItemModel court)
    {
        if (court == null) return;

        System.Diagnostics.Debug.WriteLine($"CourtListService.ValidateCourtCollectionsConsistency - Validating court {court.Id}...");

        // 1. Verificar que hay formas de pago si hay ventas
        if (court.TotalAccumulatedAmount > 0)
        {
            if (court.Collections == null || !court.Collections.Any())
            {
                System.Diagnostics.Debug.WriteLine($"❌ CRITICAL ISSUE: Court {court.Id} has sales (${court.TotalAccumulatedAmount:C}) but NO payment methods!");
                System.Diagnostics.Debug.WriteLine("   This violates business rules - every sale must have a payment method!");
            }
            else
            {
                // 2. Verificar que la suma de Collections coincide con el total
                var totalCollections = court.Collections.Sum(c => c.Amount);
                var difference = Math.Abs(totalCollections - court.TotalAccumulatedAmount);

                if (difference > 0.01) // Permitir pequeñas diferencias de redondeo
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ WARNING: Court {court.Id} payment methods total (${totalCollections:C}) doesn't match sales total (${court.TotalAccumulatedAmount:C})");
                    System.Diagnostics.Debug.WriteLine($"   Difference: ${difference:C}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Court {court.Id} payment methods are consistent with sales total");
                }
            }
        }
        else if (court.Collections != null && court.Collections.Any())
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ WARNING: Court {court.Id} has payment methods but no sales total");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"ℹ️ Court {court.Id} has no sales and no payment methods (may be valid if no transactions occurred)");
        }
    }

    private async void OpenCourtDetail(CourtListItemModel selectedCourt)
    {
        if (selectedCourt == null) return;

        System.Diagnostics.Debug.WriteLine($"CourtListService.OpenCourtDetail - Opening court {selectedCourt.Id}");

        // **PASO 1**: Validar consistencia de datos ANTES de abrir el detalle
        ValidateCourtCollectionsConsistency(selectedCourt);

        // **PASO 2**: Intentar cargar Collections si están vacíos pero hay ventas
        if (selectedCourt.TotalAccumulatedAmount > 0 &&
        (selectedCourt.Collections == null || !selectedCourt.Collections.Any()))
        {
            System.Diagnostics.Debug.WriteLine($"CourtListService.OpenCourtDetail - Collections missing for court {selectedCourt.Id}, attempting to load...");
        }

        // **PASO 3**: Asignar traducciones
        selectedCourt.DateTranslation = GlobalTranslations.Get("Date");
        selectedCourt.ConsecutiveTranslation = GlobalTranslations.Get("Consecutive");
        selectedCourt.IslanderTranslation = GlobalTranslations.Get("Islander");
        selectedCourt.CourtDetailTranslation = GlobalTranslations.Get("CourtDetail");
        selectedCourt.ShiftTranslation = GlobalTranslations.Get("Shift");
        selectedCourt.TotalsTranslation = GlobalTranslations.Get("Totals");
        selectedCourt.AccumulatedAmountTranslation = GlobalTranslations.Get("AccumulatedAmount");
        selectedCourt.AccumulatedGallonsTranslations = GlobalTranslations.Get("AccumulatedGallons");
        selectedCourt.DistincTranslation = GlobalTranslations.Get("Distinc");
        selectedCourt.DispensersTranslation = GlobalTranslations.Get("Dispensers");
        selectedCourt.LastAccumulatedAmountTranslation = GlobalTranslations.Get("LastAccumulatedAmount");
        selectedCourt.LastAccumulatedGallonsTranslation = GlobalTranslations.Get("LastAccumulatedGallons");
        selectedCourt.DocumentsTranslation = GlobalTranslations.Get("Documents");
        selectedCourt.ExpendituresTranslation = GlobalTranslations.Get("Expenditures");
        selectedCourt.CourtTranslation = GlobalTranslations.Get("Court");
        selectedCourt.ThereIsNoImageTranslation = GlobalTranslations.Get("ThereIsNoImage");
        selectedCourt.ExpenditureTranslation = GlobalTranslations.Get("Expenditure");

        // **PASO 4**: Asignar traducciones a Collections
        if (selectedCourt.Collections != null)
        {
            System.Diagnostics.Debug.WriteLine($"CourtListService.OpenCourtDetail - Applying translations to {selectedCourt.Collections.Count} collections");

            foreach (var collection in selectedCourt.Collections)
            {
                collection.DateTranslation = GlobalTranslations.Get("Date");
                collection.CollectionTranslation = GlobalTranslations.Get("Collection");
                collection.AmountTranslation = GlobalTranslations.Get("Amount");
                collection.DescriptionTranslation = GlobalTranslations.Get("Description");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("⚠️ CourtListService.OpenCourtDetail - Collections is still null after loading attempt");
        }

        // **PASO 5**: Asignar traducciones a otros elementos
        if (selectedCourt.Dispensers != null)
        {
            foreach (var dispenser in selectedCourt.Dispensers)
            {
                dispenser.DispenserTranslation = GlobalTranslations.Get("Dispenser");
                dispenser.NumberHoseTranslation = GlobalTranslations.Get("NumberHose");
                dispenser.ProductTranslation = GlobalTranslations.Get("Product");
                dispenser.PriceTranslation = GlobalTranslations.Get("Price");
                dispenser.StarttimeTranslation = GlobalTranslations.Get("Starttime");
                dispenser.EndtimeTranslation = GlobalTranslations.Get("Endtime");
                dispenser.AccumulatedAmountTranslation = GlobalTranslations.Get("AccumulatedAmount");
                dispenser.AccumulatedGallonsTranslations = GlobalTranslations.Get("AccumulatedGallons");
                dispenser.LastAccumulatedAmountTranslation = GlobalTranslations.Get("LastAccumulatedAmount");
                dispenser.LastAccumulatedGallonsTranslation = GlobalTranslations.Get("LastAccumulatedGallons");
            }
        }

        if (selectedCourt.Documents != null)
        {
            foreach (var document in selectedCourt.Documents)
            {
                document.CourtTranslation = GlobalTranslations.Get("Court");
                document.ThereIsNoImageTranslation = GlobalTranslations.Get("ThereIsNoImage");
            }
        }

        if (selectedCourt.Expenditures != null)
        {
            foreach (var expenditure in selectedCourt.Expenditures)
            {
                expenditure.DateTranslation = GlobalTranslations.Get("Date");
                expenditure.ExpenditureTranslation = GlobalTranslations.Get("Expenditure");
                expenditure.AmountTranslation = GlobalTranslations.Get("Amount");
                expenditure.DescriptionTranslation = GlobalTranslations.Get("Description");
            }
        }

        // **PASO 6**: Log final de estado antes de navegar
        System.Diagnostics.Debug.WriteLine($"CourtListService.OpenCourtDetail - Final state for court {selectedCourt.Id}:");
        System.Diagnostics.Debug.WriteLine($"  - Total Amount: ${selectedCourt.TotalAccumulatedAmount:C}");
        System.Diagnostics.Debug.WriteLine($"  - Collections Count: {selectedCourt.Collections?.Count ?? 0}");
        System.Diagnostics.Debug.WriteLine($"  - Dispensers Count: {selectedCourt.Dispensers?.Count ?? 0}");
        System.Diagnostics.Debug.WriteLine($"  - Expenditures Count: {selectedCourt.Expenditures?.Count ?? 0}");

        if (selectedCourt.Collections != null && selectedCourt.Collections.Any())
        {
            var totalFromCollections = selectedCourt.Collections.Sum(c => c.Amount);
            System.Diagnostics.Debug.WriteLine($"  - Total from Collections: ${totalFromCollections:C}");

            foreach (var collection in selectedCourt.Collections)
            {
                System.Diagnostics.Debug.WriteLine($"    * {collection.Collection}: ${collection.Amount:C}");
            }
        }

        // **PASO 7**: Navegar a la página de detalle
        try
        {
            var detailPage = new APP.Eds.UsesCases.Court.CourtDetailPage(selectedCourt);
            await Application.Current.MainPage.Navigation.PushAsync(detailPage);

            System.Diagnostics.Debug.WriteLine($"✅ Successfully navigated to court detail page for court {selectedCourt.Id}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error navigating to court detail page: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Error",
                "No se pudo abrir el detalle del corte. Por favor, intente nuevamente.", "OK");
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

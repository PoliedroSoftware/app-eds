using APP.Eds.Helpers;
using APP.Eds.Models.StrongBox;
using APP.Eds.Services.Config;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using APP.Eds.Models.Court;

namespace APP.Eds.Services.StrongBox;

public class StrongBoxService : INotifyPropertyChanged
{
    private string? _authToken;
    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<StrongBoxListModel> Movements { get; } = [];

    private StrongBoxGetLastBalanceModel _currentBalance;
    public StrongBoxGetLastBalanceModel CurrentBalance
    {
        get => _currentBalance;
        set
        {
            _currentBalance = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanWithdraw));
            OnPropertyChanged(nameof(CanWithdrawAll));
            (WithdrawCommand as Command)?.ChangeCanExecute();
            (WithdrawAllCommand as Command)?.ChangeCanExecute();
        }
    }

    private double _withdrawAmount;
    public double WithdrawAmount
    {
        get => _withdrawAmount;
        set
        {
            _withdrawAmount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanWithdraw));
            (WithdrawCommand as Command)?.ChangeCanExecute();
        }
    }

    public bool CanWithdraw =>
        CurrentBalance != null &&
        CurrentBalance.Saldo > 0 &&
        WithdrawAmount > 0 &&
        WithdrawAmount <= CurrentBalance.Saldo;

    public bool CanWithdrawAll =>
        CurrentBalance != null &&
        CurrentBalance.Saldo > 0;

    private int _currentPage = 1;
    private const int PageSize = 20;

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    private bool _isLoadingCourtDetails;
    public bool IsLoadingCourtDetails
    {
        get => _isLoadingCourtDetails;
        set { _isLoadingCourtDetails = value; OnPropertyChanged(); }
    }

    public ICommand LoadDataCommand { get; }
    public ICommand WithdrawCommand { get; }
    public ICommand WithdrawAllCommand { get; }
    public ICommand LoadMoreMovementsCommand { get; }
    public ICommand ViewCourtDetailCommand { get; }

    public StrongBoxService()
    {
        _authToken = TokenHelper.LoadToken();

        LoadDataCommand = new Command(async () => await LoadDataAsync());
        WithdrawCommand = new Command(async () => await WithdrawAsync(), () => CanWithdraw);
        WithdrawAllCommand = new Command(async () => await WithdrawAllAsync(), () => CanWithdrawAll);
        LoadMoreMovementsCommand = new Command(async () => await LoadMoreMovementsAsync());
        ViewCourtDetailCommand = new Command<StrongBoxListModel>(async (movement) => await ViewCourtDetailAsync(movement));
    }

    public async Task LoadDataAsync()
    {
        // NO mostrar loading para la carga inicial de datos - esto debe ser silencioso
        try
        {
            await GetCurrentBalanceAsync();
            _currentPage = 1;
            await GetMovementsAsync(_currentPage, PageSize);
        }
        finally
        {
            OnPropertyChanged(nameof(CurrentBalance));
            OnPropertyChanged(nameof(CanWithdraw));
            OnPropertyChanged(nameof(CanWithdrawAll));
            (WithdrawCommand as Command)?.ChangeCanExecute();
            (WithdrawAllCommand as Command)?.ChangeCanExecute();
        }
    }

    public async Task LoadMoreMovementsAsync()
    {
        _currentPage++;
        await GetMovementsAsync(_currentPage, PageSize);
    }

    public async Task WithdrawAsync()
    {
        if (!CanWithdraw)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Monto invalido para retiro.", "OK");
            return;
        }

        // Mostrar loading durante el retiro normal
        IsLoading = true;

        try
        {
            var ok = await SendWithdrawAsync(WithdrawAmount);
            if (ok)
            {
                WithdrawAmount = 0;
                await LoadDataAsync(); // Esto no muestra loading porque LoadDataAsync ya no tiene loading
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task WithdrawAllAsync()
    {
        if (!CanWithdrawAll)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No hay saldo disponible para retirar.", "OK");
            return;
        }

        var totalAmount = CurrentBalance.Saldo;
        
        // Confirmacion especial para retiro total
        bool confirm = await Application.Current.MainPage.DisplayAlert(
            "Confirmar Retiro Total",
            $"¿Esta seguro de que desea retirar todo el saldo disponible?\n\n" +
            $"Monto a retirar: ${totalAmount:N2}\n" +
            $"El saldo quedara en $0.00\n\n" +
            $"Esta accion no se puede deshacer.",
            "Retirar Todo",
            "Cancelar");

        if (!confirm) return;

        // Mostrar loading durante el retiro total
        IsLoading = true;

        try
        {
            var ok = await SendWithdrawAsync(totalAmount);
            if (ok)
            {
                WithdrawAmount = 0;
                await LoadDataAsync();
                
                await Application.Current.MainPage.DisplayAlert(
                    "Retiro Exitoso", 
                    $"Se ha retirado la totalidad del saldo: ${totalAmount:N2}\n\nEl saldo actual es: $0.00", 
                    "OK");
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task ViewCourtDetailAsync(StrongBoxListModel movement)
    {
        if (movement == null) return;
        
        System.Diagnostics.Debug.WriteLine($"ViewCourtDetailAsync called for movement: {movement.Type}, IdCorte: {movement.IdCorte}");
        
        // Solo permitir navegacion para movimientos de tipo CORTE que tengan IdCorte
        if (movement.Type != "CORTE" || movement.IdCorte == null)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Información", 
                "Este movimiento no tiene detalles de corte disponibles.", 
                "OK");
            return;
        }

        // Mostrar loading específico para cargar detalles del corte
        IsLoadingCourtDetails = true;

        // Usar CancellationToken para permitir cancelación si toma demasiado tiempo
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        try
        {
            System.Diagnostics.Debug.WriteLine($"Starting to load court details for movement type: {movement.Type}, IdCorte: {movement.IdCorte}");
            
            // Cargar detalles del corte desde el API usando el mismo endpoint que CourtService
            var courtDetails = await GetCourtDetailsAsync(movement.IdCorte.Value);
            
            if (courtDetails != null)
            {
                System.Diagnostics.Debug.WriteLine($"Successfully loaded court details, navigating to detail page");
                
                // Asegurar que las traducciones estén configuradas antes de navegar
                await SetCourtTranslationsAsync(courtDetails);
                
                // Navegar a la pagina de detalles del corte usando la misma navegacion que CourtService
                var detailPage = new APP.Eds.UsesCases.Court.CourtDetailPage(courtDetails);
                await Application.Current.MainPage.Navigation.PushAsync(detailPage);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Court details returned null");
                await Application.Current.MainPage.DisplayAlert(
                    "Error", 
                    "No se pudieron cargar los detalles del corte. Es posible que el corte no exista o haya un problema de conectividad.", 
                    "OK");
            }
        }
        catch (OperationCanceledException)
        {
            System.Diagnostics.Debug.WriteLine("Court details loading was cancelled due to timeout");
            await Application.Current.MainPage.DisplayAlert(
                "Tiempo agotado", 
                "La carga de detalles del corte está tomando demasiado tiempo. Verifique su conexión a internet e intente nuevamente.", 
                "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Exception loading court details: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert(
                "Error", 
                $"Error al cargar los detalles del corte: {ex.Message}", 
                "OK");
        }
        finally
        {
            // Ocultar loading específico
            IsLoadingCourtDetails = false;
            System.Diagnostics.Debug.WriteLine("Finished loading court details operation");
        }
    }

    // Nuevo método para configurar traducciones
    private async Task SetCourtTranslationsAsync(CourtListItemModel courtDetails)
    {
        try
        {
            // Configurar traducciones principales
            courtDetails.DateTranslation = "Fecha";
            courtDetails.ConsecutiveTranslation = "Consecutivo";
            courtDetails.IslanderTranslation = "Islero";
            courtDetails.CourtDetailTranslation = "Detalle del Corte";
            courtDetails.ShiftTranslation = "Turno";
            courtDetails.TotalsTranslation = "Totales";
            courtDetails.AccumulatedAmountTranslation = "Monto Acumulado";
            courtDetails.AccumulatedGallonsTranslations = "Galones Acumulados";
            courtDetails.DistincTranslation = "Distinc";
            courtDetails.DispensersTranslation = "Dispensadores";
            courtDetails.LastAccumulatedAmountTranslation = "Último Monto Acumulado";
            courtDetails.LastAccumulatedGallonsTranslation = "Últimos Galones Acumulados";
            courtDetails.DocumentsTranslation = "Documentos";
            courtDetails.ExpendituresTranslation = "Gastos";
            courtDetails.CourtTranslation = "Corte";
            courtDetails.ThereIsNoImageTranslation = "No hay imagen";
            courtDetails.ExpenditureTranslation = "Gasto";

            // Configurar traducciones para colecciones
            if (courtDetails.Collections != null)
            {
                foreach (var collection in courtDetails.Collections)
                {
                    collection.DateTranslation = "Fecha";
                    collection.CollectionTranslation = "Cobro";
                    collection.AmountTranslation = "Monto";
                    collection.DescriptionTranslation = "Descripción";
                }
            }

            // Configurar traducciones para dispensadores
            if (courtDetails.Dispensers != null)
            {
                foreach (var dispenser in courtDetails.Dispensers) 
                {
                    dispenser.DispenserTranslation = "Dispensador";
                    dispenser.NumberHoseTranslation = "Número de Manguera";
                    dispenser.ProductTranslation = "Producto";
                    dispenser.PriceTranslation = "Precio";
                    dispenser.StarttimeTranslation = "Hora de Inicio";
                    dispenser.EndtimeTranslation = "Hora de Fin";
                    dispenser.AccumulatedAmountTranslation = "Monto Acumulado";
                    dispenser.AccumulatedGallonsTranslations = "Galones Acumulados";
                    dispenser.LastAccumulatedAmountTranslation = "Último Monto Acumulado";
                    dispenser.LastAccumulatedGallonsTranslation = "Últimos Galones Acumulados";
                }
            }

            // Configurar traducciones para documentos
            if (courtDetails.Documents != null)
            {
                foreach (var document in courtDetails.Documents) 
                {
                    document.CourtTranslation = "Corte";
                    document.ThereIsNoImageTranslation = "No hay imagen";
                }
            }

            // Configurar traducciones para gastos
            if (courtDetails.Expenditures != null)
            {
                foreach (var expenditure in courtDetails.Expenditures) 
                {
                    expenditure.DateTranslation = "Fecha";
                    expenditure.ExpenditureTranslation = "Gasto";
                    expenditure.AmountTranslation = "Monto";
                    expenditure.DescriptionTranslation = "Descripción";
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error setting translations: {ex.Message}");
        }
    }

    private async Task<CourtListItemModel> GetCourtDetailsAsync(long courtId)
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            System.Diagnostics.Debug.WriteLine("No auth token available");
            return null;
        }

        try
        {
            // OPTIMIZACIÓN: Usar endpoint específico para obtener un court individual
            // En lugar de cargar todos los courts, hacer consulta directa por ID
            string url = $"{Configuration.BaseUrl}/api/v1/court/{courtId}";
            using var httpClient = new HttpClient();
            
            // Configurar timeout más corto para mejor UX
            httpClient.Timeout = TimeSpan.FromSeconds(15);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            System.Diagnostics.Debug.WriteLine($"Loading court details for ID: {courtId} from URL: {url}");
            var startTime = DateTime.Now;

            var response = await httpClient.GetAsync(url);
            
            var elapsed = DateTime.Now - startTime;
            System.Diagnostics.Debug.WriteLine($"Court API call took: {elapsed.TotalMilliseconds}ms, Status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting court details: {response.StatusCode} - {response.ReasonPhrase}");
                
                // Intentar leer el contenido del error para más detalles
                try
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Error content: {errorContent}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Could not read error content: {ex.Message}");
                }
                
                // Si el endpoint individual falla, intentar con el método de fallback
                System.Diagnostics.Debug.WriteLine("Trying fallback method...");
                return await GetCourtDetailsFallbackAsync(courtId);
            }

            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Received JSON response length: {json?.Length ?? 0} characters");
            
            if (string.IsNullOrEmpty(json))
            {
                System.Diagnostics.Debug.WriteLine("Received empty JSON response");
                return await GetCourtDetailsFallbackAsync(courtId);
            }
            
            // Deserialize using the wrapper structure
            var apiResponse = JsonSerializer.Deserialize<APP.Eds.Models.Inventory.ApiResponseWrapper<CourtListItemModel>>(json, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (apiResponse?.Data == null)
            {
                System.Diagnostics.Debug.WriteLine("Deserialization returned null");
                return await GetCourtDetailsFallbackAsync(courtId);
            }

            System.Diagnostics.Debug.WriteLine($"Successfully loaded court details for ID: {courtId}, Court: {apiResponse.Data.Id}");
            return apiResponse.Data;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            System.Diagnostics.Debug.WriteLine($"Timeout loading court details: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert(
                "Tiempo agotado", 
                "La carga de detalles está tomando más tiempo del esperado. Verifique su conexión.", 
                "OK");
            return null;
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"HTTP error getting court details: {ex.Message}");
            
            // Intentar el método de fallback en caso de error HTTP
            return await GetCourtDetailsFallbackAsync(courtId);
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"JSON deserialization error: {ex.Message}");
            
            // Intentar el método de fallback en caso de error de JSON
            return await GetCourtDetailsFallbackAsync(courtId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected error getting court details: {ex.Message}");
            
            // Fallback: Si el endpoint individual no funciona, usar el método anterior pero con timeout corto
            return await GetCourtDetailsFallbackAsync(courtId);
        }
    }

    // Método de fallback en caso de que el endpoint individual no esté disponible
    private async Task<CourtListItemModel> GetCourtDetailsFallbackAsync(long courtId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Using fallback method for court ID: {courtId}");
            
            string url = $"{Configuration.BaseUrl}/api/v1/court?PageNumber=1&PageSize=50"; // Aumentar un poco el tamaño para mejor cobertura
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(15); // Timeout un poco más largo para el fallback
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            System.Diagnostics.Debug.WriteLine($"Fallback: Loading from URL: {url}");
            var response = await httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"Fallback: HTTP error {response.StatusCode} - {response.ReasonPhrase}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Fallback: Received JSON response length: {json?.Length ?? 0} characters");
            
            if (string.IsNullOrEmpty(json))
            {
                System.Diagnostics.Debug.WriteLine("Fallback: Received empty JSON response");
                return null;
            }

            // Deserialize using the wrapper structure
            var apiResponse = JsonSerializer.Deserialize<APP.Eds.Models.Inventory.ApiResponseWrapper<List<CourtListItemModel>>>(json, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (apiResponse?.Data == null || !apiResponse.Data.Any())
            {
                System.Diagnostics.Debug.WriteLine("Fallback: No courts found in response");
                return null;
            }

            System.Diagnostics.Debug.WriteLine($"Fallback: Found {apiResponse.Data.Count} courts, searching for ID: {courtId}");
            
            var result = apiResponse.Data.FirstOrDefault(c => c.Id == courtId);
            System.Diagnostics.Debug.WriteLine($"Fallback method found court: {result != null}");
            
            if (result == null)
            {
                System.Diagnostics.Debug.WriteLine($"Fallback: Court with ID {courtId} not found in the first {apiResponse.Data.Count} courts");
                
                // Intentar con más páginas si no se encuentra en la primera
                for (int page = 2; page <= 3; page++) // Intentar hasta 3 páginas
                {
                    try
                    {
                        string pageUrl = $"{Configuration.BaseUrl}/api/v1/court?PageNumber={page}&PageSize=50";
                        System.Diagnostics.Debug.WriteLine($"Fallback: Trying page {page} - {pageUrl}");
                        
                        var pageResponse = await httpClient.GetAsync(pageUrl);
                        if (pageResponse.IsSuccessStatusCode)
                        {
                            var pageJson = await pageResponse.Content.ReadAsStringAsync();
                            var pageApiResponse = JsonSerializer.Deserialize<APP.Eds.Models.Inventory.ApiResponseWrapper<List<CourtListItemModel>>>(pageJson, 
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            
                            if (pageApiResponse?.Data?.Any() == true)
                            {
                                result = pageApiResponse.Data.FirstOrDefault(c => c.Id == courtId);
                                if (result != null)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Fallback: Found court in page {page}");
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Fallback: Error loading page {page}: {ex.Message}");
                    }
                }
            }
            
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in fallback court details: {ex.Message}");
            return null;
        }
    }

    public async Task GetCurrentBalanceAsync()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontro el token de autenticacion", "OK");
            return;
        }

        try
        {
            string url = $"{Configuration.BaseUrl}/api/v1/strongbox/balance";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            var response = await httpClient.GetStringAsync(url);

            var result = JsonSerializer.Deserialize<StrongBoxBalanceResponse>(
                response,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (result?.Data != null)
                CurrentBalance = result.Data;
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo obtener el saldo: {ex.Message}", "OK");
        }
    }

    // 2) Obtener lista de movimientos (paginación simple)
    public async Task GetMovementsAsync(int pageNumber = 1, int pageSize = 10)
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontro el token de autenticacion", "OK");
            return;
        }

        try
        {
            string url = $"{Configuration.BaseUrl}/api/v1/strongbox?PageNumber={pageNumber}&PageSize={pageSize}";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return; 
                var error = await response.Content.ReadAsStringAsync();
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo obtener los movimientos: {response.StatusCode}\n{error}", "OK");
                return;
            }
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<StrongBoxMovementsResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (pageNumber == 1) Movements.Clear();
            if (result?.Data != null)
            {
                foreach (var item in result.Data)
                    Movements.Add(item);
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo obtener los movimientos: {ex.Message}", "OK");
        }
    }

    // 3) Realizar retiro (POST)
    private async Task<bool> SendWithdrawAsync(double ammount)
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontro el token de autenticacion", "OK");
            return false;
        }
        if (CurrentBalance == null || ammount > CurrentBalance.Saldo)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "El monto de retiro no puede ser mayor al saldo currente.", "OK");
            return false;
        }
        if (CurrentBalance.Saldo == 0)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "El saldo es 0, no se puede realizar el retiro.", "OK");
            return false;
        }

        try
        {
            string url = $"{Configuration.BaseUrl}/api/v1/strongbox";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            var strongBoxModel = new StrongBoxModel
            {
                Type = "RETIRO",   
                Ammount = ammount,  
                Note = ammount == CurrentBalance.Saldo ? "Retiro total de caja fuerte" : ""
            };

            var request = new StrongBoxRequest { Request = strongBoxModel };
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                await GetCurrentBalanceAsync();
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo realizar el retiro: {response.StatusCode}\n{error}", "OK");
                return false;
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al realizar el retiro: {ex.Message}", "OK");
            return false;
        }
    }
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

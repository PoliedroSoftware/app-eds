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
using APP.Eds.Models.Eds;

namespace APP.Eds.Services.StrongBox;

public class StrongBoxService : INotifyPropertyChanged
{
    private string? _authToken;
    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<StrongBoxListModel> Movements { get; } = [];
    public ObservableCollection<EdsResponse> EdsList { get; } = [];

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

    private EdsResponse _selectedEds;
    public EdsResponse SelectedEds
    {
        get => _selectedEds;
        set
        {
            if (_selectedEds != value)
            {
                var previousEds = _selectedEds;
                _selectedEds = value;
                
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedEds));
                OnPropertyChanged(nameof(HasNoSelectedEds));
                
                System.Diagnostics.Debug.WriteLine($"SelectedEds changed from {previousEds?.Name ?? "null"} (ID: {previousEds?.IdEds ?? 0}) to {_selectedEds?.Name ?? "null"} (ID: {_selectedEds?.IdEds ?? 0})");
                
                // Limpiar datos inmediatamente para feedback instantáneo
                ClearCurrentData();
                
                // Actualizar estados de botones inmediatamente
                OnPropertyChanged(nameof(CanWithdraw));
                OnPropertyChanged(nameof(CanWithdrawAll));
                (WithdrawCommand as Command)?.ChangeCanExecute();
                (WithdrawAllCommand as Command)?.ChangeCanExecute();
                
                if (_selectedEds != null)
                {
                    // Mostrar loading y mensaje de estado
                    IsLoadingEdsData = true;
                    EdsStatusMessage = $"Cargando datos de {_selectedEds.Name}...";
                    
                    // Cargar datos de forma asíncrona
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            System.Diagnostics.Debug.WriteLine($"Starting to load data for EDS ID: {_selectedEds.IdEds} - {_selectedEds.Name}");
                            await LoadDataForSelectedEdsAsync();
                            
                            await MainThread.InvokeOnMainThreadAsync(() =>
                            {
                                if (HasData)
                                {
                                    EdsStatusMessage = $"Datos de {_selectedEds.Name} cargados correctamente";
                                    System.Diagnostics.Debug.WriteLine($"Data loaded successfully for EDS {_selectedEds.Name}. Balance: {CurrentBalance?.Saldo ?? 0}, Movements: {Movements.Count}");
                                }
                                else
                                {
                                    EdsStatusMessage = $"No hay registros para {_selectedEds.Name}";
                                    System.Diagnostics.Debug.WriteLine($"No data found for EDS {_selectedEds.Name}");
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            await MainThread.InvokeOnMainThreadAsync(() =>
                            {
                                EdsStatusMessage = $"Error al cargar datos de {_selectedEds.Name}";
                            });
                            System.Diagnostics.Debug.WriteLine($"Error loading data for EDS {_selectedEds?.Name}: {ex.Message}");
                        }
                        finally
                        {
                            await MainThread.InvokeOnMainThreadAsync(() =>
                            {
                                IsLoadingEdsData = false;
                                // Limpiar mensaje después de unos segundos
                                _ = Task.Delay(3000).ContinueWith(_ =>
                                {
                                    MainThread.InvokeOnMainThreadAsync(() =>
                                    {
                                        EdsStatusMessage = "";
                                    });
                                });
                            });
                        }
                    });
                }
                else
                {
                    EdsStatusMessage = "";
                    IsLoadingEdsData = false;
                    System.Diagnostics.Debug.WriteLine("No EDS selected, cleared data");
                }
            }
        }
    }

    private void ClearCurrentData()
    {
        CurrentBalance = null;
        Movements.Clear();
        WithdrawAmount = 0;
        _currentPage = 1;
        HasData = false;
        DataLoaded = false;
        NoDataMessage = "";
        System.Diagnostics.Debug.WriteLine("Cleared current data");
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
        WithdrawAmount <= CurrentBalance.Saldo &&
        SelectedEds != null;

    public bool CanWithdrawAll =>
        CurrentBalance != null &&
        CurrentBalance.Saldo > 0 &&
        SelectedEds != null;

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

    private bool _isLoadingEdsData;
    public bool IsLoadingEdsData
    {
        get => _isLoadingEdsData;
        set 
        { 
            _isLoadingEdsData = value; 
            OnPropertyChanged(); 
        }
    }

    private string _edsStatusMessage;
    public string EdsStatusMessage
    {
        get => _edsStatusMessage;
        set 
        { 
            _edsStatusMessage = value; 
            OnPropertyChanged(); 
        }
    }

    public bool HasSelectedEds => SelectedEds != null;
    public bool HasNoSelectedEds => SelectedEds == null;

    private bool _hasData;
    public bool HasData
    {
        get => _hasData;
        set 
        { 
            _hasData = value; 
            OnPropertyChanged(); 
            OnPropertyChanged(nameof(HasNoData));
        }
    }

    public bool HasNoData => !HasData;

    private bool _dataLoaded;
    public bool DataLoaded
    {
        get => _dataLoaded;
        set 
        { 
            _dataLoaded = value; 
            OnPropertyChanged(); 
        }
    }

    private string _noDataMessage = "";
    public string NoDataMessage
    {
        get => _noDataMessage;
        set 
        { 
            _noDataMessage = value; 
            OnPropertyChanged(); 
        }
    }

    public ICommand LoadDataCommand { get; }
    public ICommand WithdrawCommand { get; }
    public ICommand WithdrawAllCommand { get; }
    public ICommand LoadMoreMovementsCommand { get; }
    public ICommand ViewCourtDetailCommand { get; }
    public ICommand RefreshDataCommand { get; }

    public StrongBoxService()
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);

        LoadDataCommand = new Command(async () => await LoadDataAsync());
        WithdrawCommand = new Command(async () => await WithdrawAsync(), () => CanWithdraw);
        WithdrawAllCommand = new Command(async () => await WithdrawAllAsync(), () => CanWithdrawAll);
        LoadMoreMovementsCommand = new Command(async () => await LoadMoreMovementsAsync());
        ViewCourtDetailCommand = new Command<StrongBoxListModel>(async (movement) => await ViewCourtDetailAsync(movement));
        RefreshDataCommand = new Command(async () => await RefreshDataAsync());

        // Load EDS list on initialization
        _ = Task.Run(async () => await LoadEdsListAsync());
    }

    public async Task RefreshDataAsync()
    {
        if (SelectedEds == null) return;

        System.Diagnostics.Debug.WriteLine($"RefreshDataAsync: Manual refresh requested for EDS {SelectedEds.Name} (ID: {SelectedEds.IdEds})");
        
        IsLoadingEdsData = true;
        EdsStatusMessage = $"Actualizando datos de {SelectedEds.Name}...";
        
        try
        {
            // Forzar limpieza y recarga completa
            ClearCurrentData();
            await LoadDataForSelectedEdsAsync();
            
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                EdsStatusMessage = HasData 
                    ? $"Datos actualizados para {SelectedEds.Name}" 
                    : $"No hay registros para {SelectedEds.Name}";
            });
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                EdsStatusMessage = $"Error al actualizar datos de {SelectedEds.Name}";
            });
            System.Diagnostics.Debug.WriteLine($"RefreshDataAsync error: {ex.Message}");
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                IsLoadingEdsData = false;
                // Limpiar mensaje después de unos segundos
                _ = Task.Delay(3000).ContinueWith(_ =>
                {
                    MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        EdsStatusMessage = "";
                    });
                });
            });
        }
    }

    public async Task LoadEdsListAsync()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontro el token de autenticacion", "OK");
            return;
        }

        try
        {
            string url = $"{Configuration.BaseUrl}/api/v1/eds";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            httpClient.Timeout = TimeSpan.FromSeconds(15);

            System.Diagnostics.Debug.WriteLine($"Loading EDS list from: {url}");
            var response = await httpClient.GetStringAsync(url);

            var result = JsonSerializer.Deserialize<EdsApiResponse>(
                response,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                EdsList.Clear();
                if (result?.Data != null)
                {
                    foreach (var eds in result.Data.OrderBy(e => e.Name))
                    {
                        EdsList.Add(eds);
                    }
                    System.Diagnostics.Debug.WriteLine($"Loaded {EdsList.Count} EDS stations");
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading EDS list: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar la lista de EDS: {ex.Message}", "OK");
        }
    }

    public async Task LoadDataAsync()
    {
        // Este método ahora delega al método específico para la EDS seleccionada
        await LoadDataForSelectedEdsAsync();
    }

    private async Task LoadDataForSelectedEdsAsync()
    {
        if (SelectedEds == null)
        {
            // Clear data if no EDS is selected
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                ClearCurrentData();
            });
            return;
        }

        System.Diagnostics.Debug.WriteLine($"LoadDataForSelectedEdsAsync: Starting for EDS ID {SelectedEds.IdEds} - {SelectedEds.Name}");

        // Marcar que se está cargando y limpiar estado previo
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            DataLoaded = false;
            HasData = false;
            NoDataMessage = "";
        });

        try
        {
            // Cargar balance y movimientos en paralelo para mejor rendimiento
            var balanceTask = GetCurrentBalanceAsync();
            var movementsTask = GetMovementsAsync(1, PageSize);
            
            await Task.WhenAll(balanceTask, movementsTask);
            
            _currentPage = 1;

            // Verificar si tenemos datos después de cargar
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                bool hasBalance = CurrentBalance != null;
                bool hasMovements = Movements.Any();
                
                HasData = hasBalance || hasMovements;
                DataLoaded = true;

                if (!HasData)
                {
                    NoDataMessage = $"No hay registros de caja fuerte para la EDS {SelectedEds.Name}";
                }
                else
                {
                    NoDataMessage = "";
                }

                // Debug info
                System.Diagnostics.Debug.WriteLine($"LoadDataForSelectedEdsAsync completed - EDS: {SelectedEds.Name} (ID: {SelectedEds.IdEds}), HasBalance: {hasBalance}, Balance: {CurrentBalance?.Saldo ?? 0}, HasMovements: {hasMovements}, MovementsCount: {Movements.Count}");
            });
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                HasData = false;
                DataLoaded = true;
                NoDataMessage = $"Error al cargar los datos de {SelectedEds?.Name}: {ex.Message}";
                
                System.Diagnostics.Debug.WriteLine($"Error in LoadDataForSelectedEdsAsync: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", 
                    $"Error al cargar los datos de la EDS {SelectedEds?.Name}: {ex.Message}", "OK");
            });
        }
        finally
        {
            // Notificar cambios en la UI
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                OnPropertyChanged(nameof(CurrentBalance));
                OnPropertyChanged(nameof(CanWithdraw));
                OnPropertyChanged(nameof(CanWithdrawAll));
                (WithdrawCommand as Command)?.ChangeCanExecute();
                (WithdrawAllCommand as Command)?.ChangeCanExecute();
            });
        }
    }

    public async Task LoadMoreMovementsAsync()
    {
        if (SelectedEds == null) return;
        
        _currentPage++;
        await GetMovementsAsync(_currentPage, PageSize);
    }

    public async Task WithdrawAsync()
    {
        if (!CanWithdraw)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Monto invalido para retiro o EDS no seleccionada.", "OK");
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
                await LoadDataForSelectedEdsAsync(); // Usar el método específico para la EDS seleccionada
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
            await Application.Current.MainPage.DisplayAlert("Error", "No hay saldo disponible para retirar o EDS no seleccionada.", "OK");
            return;
        }

        var totalAmount = CurrentBalance.Saldo;
        
        // Confirmacion especial para retiro total
        bool confirm = await Application.Current.MainPage.DisplayAlert(
            "Confirmar Retiro Total",
            $"¿Esta seguro de que desea retirar todo el saldo disponible de {SelectedEds.Name}?\n\n" +
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
                await LoadDataForSelectedEdsAsync();
                
                await Application.Current.MainPage.DisplayAlert(
                    "Retiro Exitoso", 
                    $"Se ha retirado la totalidad del saldo de {SelectedEds.Name}: ${totalAmount:N2}\n\nEl saldo actual es: $0.00", 
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
            
            // Intentar deserializar como objeto único primero
            var courtDetails = JsonSerializer.Deserialize<CourtListItemModel>(json, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (courtDetails == null)
            {
                System.Diagnostics.Debug.WriteLine("Deserialization returned null");
                return await GetCourtDetailsFallbackAsync(courtId);
            }

            System.Diagnostics.Debug.WriteLine($"Successfully loaded court details for ID: {courtId}, Court: {courtDetails.Id}");
            return courtDetails;
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

            var courts = JsonSerializer.Deserialize<List<CourtListItemModel>>(json, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (courts == null || !courts.Any())
            {
                System.Diagnostics.Debug.WriteLine("Fallback: No courts found in response");
                return null;
            }

            System.Diagnostics.Debug.WriteLine($"Fallback: Found {courts.Count} courts, searching for ID: {courtId}");
            
            var result = courts.FirstOrDefault(c => c.Id == courtId);
            System.Diagnostics.Debug.WriteLine($"Fallback method found court: {result != null}");
            
            if (result == null)
            {
                System.Diagnostics.Debug.WriteLine($"Fallback: Court with ID {courtId} not found in the first {courts.Count} courts");
                
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
                            var pageCourts = JsonSerializer.Deserialize<List<CourtListItemModel>>(pageJson, 
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            
                            if (pageCourts?.Any() == true)
                            {
                                result = pageCourts.FirstOrDefault(c => c.Id == courtId);
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
        if (string.IsNullOrEmpty(_authToken) || SelectedEds == null)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                CurrentBalance = null;
            });
            return;
        }

        try
        {
            // CAMBIO: Usar endpoint más específico para obtener balance filtrado por EDS
            string url = $"{Configuration.BaseUrl}/api/v1/strongbox/balance/eds/{SelectedEds.IdEds}";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            System.Diagnostics.Debug.WriteLine($"GetCurrentBalanceAsync: Fetching balance for EDS {SelectedEds.IdEds} - {SelectedEds.Name} from URL: {url}");

            var response = await httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"GetCurrentBalanceAsync: Received response: {json}");
                
                var result = JsonSerializer.Deserialize<StrongBoxBalanceResponse>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    // CAMBIO: Verificación adicional para asegurar que el balance pertenece a la EDS correcta
                    if (result?.Data != null && (result.Data.IdEds == null || result.Data.IdEds == SelectedEds.IdEds))
                    {
                        CurrentBalance = result.Data;
                        System.Diagnostics.Debug.WriteLine($"GetCurrentBalanceAsync: Set CurrentBalance - Balance: {CurrentBalance.Saldo} for EDS {SelectedEds.Name} (ID: {SelectedEds.IdEds})");
                    }
                    else
                    {
                        CurrentBalance = null;
                        System.Diagnostics.Debug.WriteLine($"GetCurrentBalanceAsync: Balance does not match selected EDS. Expected EDS ID: {SelectedEds.IdEds}, Got: {result?.Data?.IdEds}");
                    }
                });
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // FALLBACK: Si el endpoint específico no existe, usar el método anterior con validación adicional
                await GetCurrentBalanceFallbackAsync();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"GetCurrentBalanceAsync: HTTP Error {response.StatusCode}: {error}");
                
                // FALLBACK: En caso de error, intentar con el método anterior
                await GetCurrentBalanceFallbackAsync();
            }
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetCurrentBalanceAsync: HTTP Request Exception: {ex.Message}");
            // FALLBACK: En caso de error de conectividad, intentar con el método anterior
            await GetCurrentBalanceFallbackAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetCurrentBalanceAsync: Exception: {ex.Message}");
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                CurrentBalance = null;
            });
        }
    }

    // NUEVO: Método de fallback que usa el endpoint original pero con filtrado del lado del cliente
    private async Task GetCurrentBalanceFallbackAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"GetCurrentBalanceFallbackAsync: Using fallback method for EDS {SelectedEds.IdEds} - {SelectedEds.Name}");
            
            string url = $"{Configuration.BaseUrl}/api/v1/strongbox/balance?idEds={SelectedEds.IdEds}";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            var response = await httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"GetCurrentBalanceFallbackAsync: Received response: {json}");
                
                // Intentar deserializar como objeto único primero
                try
                {
                    var singleResult = JsonSerializer.Deserialize<StrongBoxBalanceResponse>(
                        json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (singleResult?.Data != null)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            // Verificar que el balance pertenece a la EDS correcta
                            if (singleResult.Data.IdEds == null || singleResult.Data.IdEds == SelectedEds.IdEds)
                            {
                                CurrentBalance = singleResult.Data;
                                System.Diagnostics.Debug.WriteLine($"GetCurrentBalanceFallbackAsync: Single object - Set CurrentBalance: {CurrentBalance.Saldo} for EDS {SelectedEds.Name}");
                            }
                            else
                            {
                                CurrentBalance = null;
                                System.Diagnostics.Debug.WriteLine($"GetCurrentBalanceFallbackAsync: Single object - EDS ID mismatch. Expected: {SelectedEds.IdEds}, Got: {singleResult.Data.IdEds}");
                            }
                        });
                        return;
                    }
                }
                catch (JsonException)
                {
                    System.Diagnostics.Debug.WriteLine("GetCurrentBalanceFallbackAsync: Failed to deserialize as single object, trying as array");
                }

                // Si falla como objeto único, intentar como lista y filtrar
                try
                {
                    var listResult = JsonSerializer.Deserialize<List<StrongBoxGetLastBalanceModel>>(
                        json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (listResult?.Any() == true)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            // Filtrar por EDS específica del lado del cliente
                            var filteredBalance = listResult.FirstOrDefault(b => 
                                b.IdEds == null || b.IdEds == SelectedEds.IdEds);
                            
                            if (filteredBalance != null)
                            {
                                CurrentBalance = filteredBalance;
                                System.Diagnostics.Debug.WriteLine($"GetCurrentBalanceFallbackAsync: Array - Set CurrentBalance: {CurrentBalance.Saldo} for EDS {SelectedEds.Name} (filtered from {listResult.Count} balances)");
                            }
                            else
                            {
                                CurrentBalance = null;
                                System.Diagnostics.Debug.WriteLine($"GetCurrentBalanceFallbackAsync: Array - No balance found for EDS {SelectedEds.IdEds} in {listResult.Count} balances");
                            }
                        });
                        return;
                    }
                }
                catch (JsonException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"GetCurrentBalanceFallbackAsync: Failed to deserialize as array: {ex.Message}");
                }

                // Si ningún método de deserialización funciona
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    CurrentBalance = null;
                    System.Diagnostics.Debug.WriteLine("GetCurrentBalanceFallbackAsync: Could not deserialize response as single object or array");
                });
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // No hay balance para esta EDS - esto es normal para EDS sin registros
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    CurrentBalance = null;
                    System.Diagnostics.Debug.WriteLine($"GetCurrentBalanceFallbackAsync: No balance found for EDS {SelectedEds.IdEds} - {SelectedEds.Name}");
                });
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"GetCurrentBalanceFallbackAsync: HTTP Error {response.StatusCode}: {error}");
                
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    CurrentBalance = null;
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetCurrentBalanceFallbackAsync: Exception: {ex.Message}");
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                CurrentBalance = null;
            });
        }
    }

    // 2) Obtener lista de movimientos (paginación simple)
    public async Task GetMovementsAsync(int pageNumber = 1, int pageSize = 10)
    {
        if (string.IsNullOrEmpty(_authToken) || SelectedEds == null)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (pageNumber == 1) Movements.Clear();
            });
            return;
        }

        try
        {
            // Usar el endpoint específico para obtener movimientos filtrados por EDS
            string url = $"{Configuration.BaseUrl}/api/v1/strongbox/eds/{SelectedEds.IdEds}";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            System.Diagnostics.Debug.WriteLine($"GetMovementsAsync: Fetching movements for EDS {SelectedEds.IdEds} - {SelectedEds.Name}, Page: {pageNumber} from URL: {url}");

            var response = await httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"GetMovementsAsync: Received response length: {json?.Length ?? 0} chars");
                
                var result = JsonSerializer.Deserialize<StrongBoxMovementsResponse>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (pageNumber == 1) Movements.Clear();
                    
                    if (result?.Data != null)
                    {
                        int addedCount = 0;
                        foreach (var item in result.Data)
                        {
                            // Verificación adicional: solo agregar movimientos que pertenecen a la EDS seleccionada
                            if (item.IdEds == SelectedEds.IdEds || item.IdEds == null)
                            {
                                Movements.Add(item);
                                addedCount++;
                                System.Diagnostics.Debug.WriteLine($"GetMovementsAsync: Added movement - ID: {item.Id}, Type: {item.Type}, Amount: {item.Ammount}, IdEds: {item.IdEds}");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"GetMovementsAsync: Filtered out movement - ID: {item.Id}, Type: {item.Type}, Amount: {item.Ammount}, IdEds: {item.IdEds} (Expected: {SelectedEds.IdEds})");
                            }
                        }
                        
                        System.Diagnostics.Debug.WriteLine($"GetMovementsAsync: Added {addedCount} movements for EDS {SelectedEds.Name}, Total in collection: {Movements.Count}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"GetMovementsAsync: No movements data in response for EDS {SelectedEds.Name}");
                    }
                });
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // No hay movimientos para esta EDS - esto es normal para EDS sin registros
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (pageNumber == 1) Movements.Clear();
                    System.Diagnostics.Debug.WriteLine($"GetMovementsAsync: No movements found for EDS {SelectedEds.IdEds} - {SelectedEds.Name}");
                });
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"GetMovementsAsync: HTTP Error {response.StatusCode}: {error}");
                
                if (pageNumber == 1)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        Movements.Clear();
                    });
                }
            }
        }
        catch (HttpRequestException ex)
        {
            // Error de conectividad - manejar silenciosamente para la primera página
            System.Diagnostics.Debug.WriteLine($"GetMovementsAsync: HTTP Request Exception: {ex.Message}");
            if (pageNumber == 1)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Movements.Clear();
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetMovementsAsync: Exception: {ex.Message}");
            if (pageNumber == 1)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Movements.Clear();
                });
            }
        }
    }

    // 3) Realizar retiro (POST)
    private async Task<bool> SendWithdrawAsync(double ammount)
    {
        if (string.IsNullOrEmpty(_authToken) || SelectedEds == null)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontro el token de autenticacion o EDS no seleccionada", "OK");
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
                IdEds = SelectedEds.IdEds,
                Note = ammount == CurrentBalance.Saldo ? $"Retiro total de caja fuerte - {SelectedEds.Name}" : $"Retiro de caja fuerte - {SelectedEds.Name}"
            };

            var request = new StrongBoxRequest { Request = strongBoxModel };
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            System.Diagnostics.Debug.WriteLine($"SendWithdrawAsync: Sending withdraw request for EDS {SelectedEds.IdEds} - {SelectedEds.Name}, Amount: {ammount}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"SendWithdrawAsync: Withdraw successful for EDS {SelectedEds.Name}");
                await GetCurrentBalanceAsync();
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"SendWithdrawAsync: Error {response.StatusCode} - {error}");
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo realizar el retiro: {response.StatusCode}\n{error}", "OK");
                return false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SendWithdrawAsync: Exception - {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al realizar el retiro: {ex.Message}", "OK");
            return false;
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

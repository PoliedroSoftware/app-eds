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
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);

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
        
        // Solo permitir navegacion para movimientos de tipo CORTE que tengan IdCorte
        if (movement.Type != "CORTE" || movement.IdCorte == null)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Informacion", 
                "Este movimiento no tiene detalles de corte disponibles.", 
                "OK");
            return;
        }

        // Mostrar loading específico para cargar detalles del corte
        IsLoadingCourtDetails = true;

        try
        {
            // Cargar detalles del corte desde el API usando el mismo endpoint que CourtService
            var courtDetails = await GetCourtDetailsAsync(movement.IdCorte.Value);
            
            if (courtDetails != null)
            {
                // Navegar a la pagina de detalles del corte usando la misma navegacion que CourtService
                var detailPage = new APP.Eds.UsesCases.Court.CourtDetailPage(courtDetails);
                await Application.Current.MainPage.Navigation.PushAsync(detailPage);
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error", 
                    "No se pudieron cargar los detalles del corte.", 
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error", 
                $"Error al cargar los detalles del corte: {ex.Message}", 
                "OK");
        }
        finally
        {
            // Ocultar loading específico
            IsLoadingCourtDetails = false;
        }
    }

    private async Task<CourtListItemModel> GetCourtDetailsAsync(long courtId)
    {
        if (string.IsNullOrEmpty(_authToken))
            return null;

        try
        {
            // Usar el mismo endpoint que CourtService: /api/v1/court?PageNumber=1&PageSize=100
            string url = $"{Configuration.BaseUrl}/api/v1/court?PageNumber=1&PageSize=100";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting court details: {response.StatusCode}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            
            // Usar el mismo deserializador que CourtService para obtener una lista de courts
            var courts = JsonSerializer.Deserialize<List<CourtListItemModel>>(json, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Buscar el court específico por Id
            var courtDetails = courts?.FirstOrDefault(c => c.Id == courtId);

            return courtDetails;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting court details: {ex.Message}");
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

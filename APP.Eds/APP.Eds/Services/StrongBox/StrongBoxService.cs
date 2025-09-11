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
            (WithdrawCommand as Command)?.ChangeCanExecute();
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

    private int _currentPage = 1;
    private const int PageSize = 20;

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    public ICommand LoadDataCommand { get; }
    public ICommand WithdrawCommand { get; }
    public ICommand LoadMoreMovementsCommand { get; }

    public StrongBoxService()
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);

        LoadDataCommand = new Command(async () => await LoadDataAsync());
        WithdrawCommand = new Command(async () => await WithdrawAsync(), () => CanWithdraw);
        LoadMoreMovementsCommand = new Command(async () => await LoadMoreMovementsAsync());
    }

    public async Task LoadDataAsync()
    {
        IsLoading = true;
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
            (WithdrawCommand as Command)?.ChangeCanExecute();
            IsLoading = false;
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
            await Application.Current.MainPage.DisplayAlert("Error", "Monto inválido para retiro.", "OK");
            return;
        }

        var ok = await SendWithdrawAsync(WithdrawAmount);
        if (ok)
        {
            WithdrawAmount = 0;
            await LoadDataAsync();
        }
    }

    public async Task GetCurrentBalanceAsync()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
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
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
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
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return false;
        }
        if (CurrentBalance == null || ammount > CurrentBalance.Saldo)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "El monto de retiro no puede ser mayor al saldo actual.", "OK");
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
                Note = ""
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

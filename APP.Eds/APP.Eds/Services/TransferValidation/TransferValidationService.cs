using APP.Eds.Helpers;
using APP.Eds.Models.TransferValidation;
using APP.Eds.Services.Config;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;

namespace APP.Eds.Services.TransferValidation;

/// <summary>
/// Servicio para gestionar las validaciones de transferencias QR
/// </summary>
public class TransferValidationService : INotifyPropertyChanged
{
    private readonly string? _authToken;
    private bool _isLoading;
    private bool _hasData;
    private string _searchText = string.Empty;
    private string _selectedStatusFilter = "TODOS";

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Lista de todas las validaciones de transferencia
    /// </summary>
    public ObservableCollection<TransferValidationModel> TransferValidations { get; set; } = new();

    /// <summary>
    /// Lista filtrada de validaciones de transferencia
    /// </summary>
    public ObservableCollection<TransferValidationModel> FilteredTransferValidations { get; set; } = new();

    /// <summary>
    /// Opciones de filtro por estado
    /// </summary>
    public ObservableCollection<string> StatusFilterOptions { get; set; } = new()
    {
     "TODOS",
        "CONFIRMADA",
     "PENDIENTE",
      "RECHAZADA"
    };

    /// <summary>
    /// Comando para refrescar datos
    /// </summary>
    public ICommand? RefreshCommand { get; set; }

    /// <summary>
    /// Indica si se están cargando datos
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNotLoading));
        }
    }

    /// <summary>
    /// Indica si NO se están cargando datos (para binding inverso)
    /// </summary>
    public bool IsNotLoading => !IsLoading;

    /// <summary>
    /// Indica si hay datos disponibles
    /// </summary>
    public bool HasData
    {
        get => _hasData;
        set
        {
            _hasData = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Texto de búsqueda
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();
            ApplyFilters();
        }
    }

    /// <summary>
    /// Filtro seleccionado por estado
    /// </summary>
    public string SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set
        {
            _selectedStatusFilter = value;
            OnPropertyChanged();
            ApplyFilters();
        }
    }

    // Statistics Properties
    private int _totalTransactions;
    public int TotalTransactions
    {
        get => _totalTransactions;
        set
        {
            _totalTransactions = value;
            OnPropertyChanged();
        }
    }

    private int _confirmedTransactions;
    public int ConfirmedTransactions
    {
        get => _confirmedTransactions;
        set
        {
            _confirmedTransactions = value;
            OnPropertyChanged();
        }
    }

    private int _pendingTransactions;
    public int PendingTransactions
    {
        get => _pendingTransactions;
        set
        {
            _pendingTransactions = value;
            OnPropertyChanged();
        }
    }

    private double _totalAmount;
    public double TotalAmount
    {
        get => _totalAmount;
        set
        {
            _totalAmount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FormattedTotalAmount));
        }
    }

    public string FormattedTotalAmount => $"${TotalAmount:N0}";

    public TransferValidationService()
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
        RefreshCommand = new Command(async () => await RefreshDataAsync());
    }

    /// <summary>
    /// Obtiene todas las validaciones de transferencia desde la API
    /// </summary>
    public async Task GetTransferValidationsAsync()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }

        IsLoading = true;
        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            string url = $"{Configuration.BaseUrl}/api/v1/transfer-validation";
            var response = await httpClient.GetStringAsync(url);

            var validationResponse = JsonSerializer.Deserialize<TransferValidationResponse>(
                  response,
                           new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (validationResponse?.Success == true && validationResponse.Data != null)
            {
                TransferValidations.Clear();
                foreach (var validation in validationResponse.Data.OrderByDescending(v => v.IdTransferValidation))
                {
                    TransferValidations.Add(validation);
                }

                UpdateStatistics();
                ApplyFilters();
                HasData = TransferValidations.Any();
            }
            else
            {
                HasData = false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error cargando validaciones de transferencia: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert(
             "Error",
              $"No se pudieron cargar las validaciones de transferencia: {ex.Message}",
                "OK"
      );
            HasData = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Aplica los filtros de búsqueda y estado
    /// </summary>
    private void ApplyFilters()
    {
        var filtered = TransferValidations.AsEnumerable();

        // Filtrar por estado
        if (SelectedStatusFilter != "TODOS")
        {
            filtered = filtered.Where(v => v.Status == SelectedStatusFilter);
        }

        // Filtrar por texto de búsqueda
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLower();
            filtered = filtered.Where(v =>
          v.CustomerName.ToLower().Contains(searchLower) ||
             v.UniqueId.ToLower().Contains(searchLower) ||
                v.ConfirmedBy.ToLower().Contains(searchLower)
         );
        }

        FilteredTransferValidations.Clear();
        foreach (var item in filtered)
        {
            FilteredTransferValidations.Add(item);
        }

        HasData = FilteredTransferValidations.Any();
    }

    /// <summary>
    /// Actualiza las estadísticas
    /// </summary>
    private void UpdateStatistics()
    {
        TotalTransactions = TransferValidations.Count;
        ConfirmedTransactions = TransferValidations.Count(v => v.Status == "CONFIRMADA");
        PendingTransactions = TransferValidations.Count(v => v.Status == "PENDIENTE");
        TotalAmount = TransferValidations.Sum(v => v.TransactionAmount);
    }

    /// <summary>
    /// Refresca los datos desde la API
    /// </summary>
    public async Task RefreshDataAsync()
    {
        await GetTransferValidationsAsync();
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

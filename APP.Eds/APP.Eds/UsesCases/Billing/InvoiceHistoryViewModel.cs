using APP.Eds.Models.Billing;
using APP.Eds.Services.Billing;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace APP.Eds.UsesCases.Billing;

public class InvoiceHistoryViewModel : INotifyPropertyChanged
{
    private readonly ElectronicBillingService _billingService;
    private bool _isLoading;
    private bool _isEmpty;
    private ElectronicInvoiceModel _selectedInvoice;
    private InvoiceFilterModel _currentFilter;
    private ObservableCollection<ElectronicInvoiceModel> _filteredInvoices;
    private string _userRole;
    private string _currentUserId;
    private string _currentUserName;

    // Filter properties
    private string _selectedEdsId;
    private string _selectedEdsName;
    private string _selectedIslanderId;
    private string _selectedIslanderName;
    private string _selectedStatus;
    private DateTime? _dateFrom;
    private DateTime? _dateTo;

    public InvoiceHistoryViewModel()
    {
        _billingService = new ElectronicBillingService();
        
        // Get current user information
        _userRole = Preferences.Get("userRole", "");
        _currentUserId = Preferences.Get("userId", "");
        _currentUserName = Preferences.Get("userName", "Usuario");

        System.Diagnostics.Debug.WriteLine($"📋 InvoiceHistoryViewModel iniciado");
        System.Diagnostics.Debug.WriteLine($"   - Rol: {_userRole}");
        System.Diagnostics.Debug.WriteLine($"   - Usuario ID: {_currentUserId}");
        System.Diagnostics.Debug.WriteLine($"   - Usuario: {_currentUserName}");

        // Initialize filter based on role
        InitializeFilter();

        // Commands
        ViewPdfCommand = new Command<ElectronicInvoiceModel>(async (invoice) => await ViewPdf(invoice));
        SharePdfCommand = new Command<ElectronicInvoiceModel>(async (invoice) => await SharePdf(invoice));
        CreditNoteCommand = new Command<ElectronicInvoiceModel>(async (invoice) => await GenerateCreditNote(invoice));
        RefreshCommand = new Command(async () => await Refresh());
        ApplyFiltersCommand = new Command(ApplyFilters);
        ClearFiltersCommand = new Command(ClearFilters);

        // Load initial data
        LoadInvoices();
    }

    #region Properties

    public ObservableCollection<ElectronicInvoiceModel> Invoices => _filteredInvoices ?? new ObservableCollection<ElectronicInvoiceModel>();

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsEmpty
    {
        get => _isEmpty;
        set => SetProperty(ref _isEmpty, value);
    }

    public ElectronicInvoiceModel SelectedInvoice
    {
        get => _selectedInvoice;
        set => SetProperty(ref _selectedInvoice, value);
    }

    // Role and permissions
    public bool IsAdmin => _userRole == "Admin";
    public bool IsIslander => _userRole == "User";
    public bool CanEditFilters => IsAdmin; // Solo admin puede editar filtros EDS/Islero
    public string UserDisplayName => _currentUserName;

    // Filter properties
    public string SelectedEdsId
    {
        get => _selectedEdsId;
        set => SetProperty(ref _selectedEdsId, value);
    }

    public string SelectedEdsName
    {
        get => _selectedEdsName;
        set => SetProperty(ref _selectedEdsName, value);
    }

    public string SelectedIslanderId
    {
        get => _selectedIslanderId;
        set => SetProperty(ref _selectedIslanderId, value);
    }

    public string SelectedIslanderName
    {
        get => _selectedIslanderName;
        set => SetProperty(ref _selectedIslanderName, value);
    }

    public string SelectedStatus
    {
        get => _selectedStatus;
        set => SetProperty(ref _selectedStatus, value);
    }

    public DateTime? DateFrom
    {
        get => _dateFrom;
        set => SetProperty(ref _dateFrom, value);
    }

    public DateTime? DateTo
    {
        get => _dateTo;
        set => SetProperty(ref _dateTo, value);
    }

    // Available options for pickers
    public List<(string Id, string Name)> AvailableEds => _billingService.GetAvailableEds();
    public List<(string Id, string Name)> AvailableIslanders => _billingService.GetAvailableIslanders();
    
    public List<string> AvailableStatuses => new List<string>
    {
        "TODOS",
        "EMITIDA",
        "ANULADA",
        "SIN_EMITIR"
    };

    #endregion

    #region Commands

    public ICommand ViewPdfCommand { get; }
    public ICommand SharePdfCommand { get; }
    public ICommand CreditNoteCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ApplyFiltersCommand { get; }
    public ICommand ClearFiltersCommand { get; }

    #endregion

    #region Private Methods

    private void InitializeFilter()
    {
        if (IsIslander)
        {
            // Para isleros, crear filtro automático con sus datos
            _currentFilter = InvoiceFilterModel.CreateForIslander(_currentUserId, _currentUserName);
            _selectedIslanderId = _currentUserId;
            _selectedIslanderName = _currentUserName;
            _selectedStatus = "EMITIDA";
            System.Diagnostics.Debug.WriteLine($"🔒 Filtro de Islero aplicado automáticamente");
        }
        else
        {
            // Para admin, crear filtro por defecto
            _currentFilter = InvoiceFilterModel.CreateDefault();
            _selectedStatus = "TODOS";
            System.Diagnostics.Debug.WriteLine($"🔓 Filtro de Admin - acceso completo");
        }

        // Set default date range (last 30 days)
        _dateFrom = DateTime.Now.AddDays(-30);
        _dateTo = DateTime.Now;
        
        _currentFilter.DateFrom = _dateFrom;
        _currentFilter.DateTo = _dateTo;
        _currentFilter.UserRole = _userRole;
        _currentFilter.CurrentUserId = _currentUserId;
    }

    private void LoadInvoices()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"📂 Cargando facturas con filtro actual...");
            
            // Apply current filter
            _filteredInvoices = _billingService.FilterInvoices(_currentFilter);
            
            // Update empty state
            IsEmpty = !_filteredInvoices.Any();
            
            OnPropertyChanged(nameof(Invoices));
            OnPropertyChanged(nameof(AvailableEds));
            OnPropertyChanged(nameof(AvailableIslanders));
            
            System.Diagnostics.Debug.WriteLine($"✅ {_filteredInvoices.Count} facturas cargadas");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error cargando facturas: {ex.Message}");
            _filteredInvoices = new ObservableCollection<ElectronicInvoiceModel>();
            IsEmpty = true;
        }
    }

    private void ApplyFilters()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🔍 Aplicando filtros...");

            // Update filter model
            _currentFilter.EdsId = _selectedEdsId;
            _currentFilter.EdsName = _selectedEdsName;
            
            // For islanders, force their own ID
            if (IsIslander)
            {
                _currentFilter.IslanderId = _currentUserId;
                _currentFilter.IslanderName = _currentUserName;
                _currentFilter.Status = "EMITIDA";
            }
            else
            {
                _currentFilter.IslanderId = _selectedIslanderId;
                _currentFilter.IslanderName = _selectedIslanderName;
                _currentFilter.Status = _selectedStatus ?? "TODOS";
            }
            
            _currentFilter.DateFrom = _dateFrom;
            _currentFilter.DateTo = _dateTo;
            _currentFilter.UserRole = _userRole;
            _currentFilter.CurrentUserId = _currentUserId;

            // Reload invoices with new filter
            LoadInvoices();

            System.Diagnostics.Debug.WriteLine($"✅ Filtros aplicados correctamente");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error aplicando filtros: {ex.Message}");
        }
    }

    private void ClearFilters()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🔄 Limpiando filtros...");

            if (IsIslander)
            {
                // For islanders, can only clear date range
                _dateFrom = DateTime.Now.AddDays(-30);
                _dateTo = DateTime.Now;
            }
            else
            {
                // For admin, clear all filters
                _selectedEdsId = null;
                _selectedEdsName = null;
                _selectedIslanderId = null;
                _selectedIslanderName = null;
                _selectedStatus = "TODOS";
                _dateFrom = DateTime.Now.AddDays(-30);
                _dateTo = DateTime.Now;
            }

            OnPropertyChanged(nameof(SelectedEdsId));
            OnPropertyChanged(nameof(SelectedEdsName));
            OnPropertyChanged(nameof(SelectedIslanderId));
            OnPropertyChanged(nameof(SelectedIslanderName));
            OnPropertyChanged(nameof(SelectedStatus));
            OnPropertyChanged(nameof(DateFrom));
            OnPropertyChanged(nameof(DateTo));

            // Reapply with cleared filters
            ApplyFilters();

            System.Diagnostics.Debug.WriteLine($"✅ Filtros limpiados");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error limpiando filtros: {ex.Message}");
        }
    }

    #endregion

    #region Command Implementations

    private async Task ViewPdf(ElectronicInvoiceModel invoice)
    {
        if (invoice == null || !invoice.IsPdfAvailable)
        {
            await Application.Current.MainPage.DisplayAlert(
             "PDF No Disponible",
             "El PDF de esta factura no está disponible",
             "OK");
            return;
        }

        IsLoading = true;
        try
        {
            // ✨ NUEVO: Usar PDF mock si el modo mock está activo
            if (ElectronicBillingService.UseMockData)
            {
                await MockInvoiceService.OpenMockPdfAsync(invoice.FullInvoiceNumber);
            }
            else
            {
                await _billingService.OpenInvoicePdfAsync(invoice.InvoiceHash, invoice.FullInvoiceNumber);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error viewing PDF: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert(
               "Error",
                            $"No se pudo abrir el PDF: {ex.Message}",
                      "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SharePdf(ElectronicInvoiceModel invoice)
    {
        if (invoice == null || !invoice.IsPdfAvailable)
        {
            await Application.Current.MainPage.DisplayAlert(
            "PDF No Disponible",
            "El PDF de esta factura no está disponible para compartir",
            "OK");
            return;
        }

        IsLoading = true;
        try
        {
            // ✨ NUEVO: Usar PDF mock si el modo mock está activo
            if (ElectronicBillingService.UseMockData)
            {
                await MockInvoiceService.ShareMockPdfAsync(invoice.FullInvoiceNumber);
            }
            else
            {
                await _billingService.ShareInvoicePdfAsync(invoice.InvoiceHash, invoice.FullInvoiceNumber);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error sharing PDF: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert(
                "Error",
                $"No se pudo compartir el PDF: {ex.Message}",
                "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ✨ NEW: Generate Credit Note
    private async Task GenerateCreditNote(ElectronicInvoiceModel invoice)
    {
        if (invoice == null)
        {
            return;
        }

        try
        {
            // Confirmar acción con el usuario
            var confirmed = await Application.Current.MainPage.DisplayAlert(
                "Generar Nota Crédito",
                $"¿Está seguro de generar una nota crédito para la factura {invoice.FullInvoiceNumber}?\n\n" +
                $"Cliente: {invoice.ClientName}\n" +
                $"Total: {invoice.TotalAmountFormatted}\n\n" +
                $"Esta acción anulará la factura seleccionada.",
                "Generar",
                "Cancelar");

            if (!confirmed)
            {
                return;
            }

            IsLoading = true;

            // Solicitar motivo de la nota crédito
            var reason = await Application.Current.MainPage.DisplayPromptAsync(
                "Motivo de la Nota Crédito",
                "Por favor, indique el motivo de la nota crédito:",
                placeholder: "Ej: Error en la facturación, Devolución de producto, etc.",
                maxLength: 200);

            if (string.IsNullOrWhiteSpace(reason))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Motivo Requerido",
                    "Debe proporcionar un motivo para generar la nota crédito.",
                    "OK");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"📝 Generando nota crédito para factura {invoice.FullInvoiceNumber}");
            System.Diagnostics.Debug.WriteLine($"   Motivo: {reason}");

            // Generar nota crédito
            var result = await _billingService.GenerateCreditNoteAsync(invoice, reason);

            if (result.Success)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "✅ Nota Crédito Generada",
                    $"La nota crédito ha sido generada exitosamente.\n\n" +
                    $"📄 Número: {result.CreditNoteNumber}\n" +
                    $"💰 Monto: {invoice.TotalAmountFormatted}\n" +
                    $"📅 Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}\n\n" +
                    $"La factura {invoice.FullInvoiceNumber} ha sido anulada.",
                    "OK");

                // Recargar lista de facturas
                await Refresh();
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert(
                    "⚠️ Error al Generar Nota Crédito",
                    $"No se pudo generar la nota crédito.\n\n" +
                    $"Error: {result.Message}\n\n" +
                    $"Por favor, intente nuevamente o contacte soporte técnico.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error generando nota crédito: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert(
                "Error del Sistema",
                $"Ocurrió un error al generar la nota crédito.\n\n" +
                $"Error: {ex.Message}",
                "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task Refresh()
    {
        IsLoading = true;
        try
        {
            await Task.Delay(500); // Simular carga
            LoadInvoices();
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

    #region INotifyPropertyChanged Implementation

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion
}

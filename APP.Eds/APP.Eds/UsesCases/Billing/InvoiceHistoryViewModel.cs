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

    public InvoiceHistoryViewModel()
    {
        _billingService = new ElectronicBillingService();

        // Comandos
        ViewPdfCommand = new Command<ElectronicInvoiceModel>(async (invoice) => await ViewPdf(invoice));
        SharePdfCommand = new Command<ElectronicInvoiceModel>(async (invoice) => await SharePdf(invoice));
        RefreshCommand = new Command(async () => await Refresh());

        // Cargar datos iniciales
        LoadInvoices();
    }

    public ObservableCollection<ElectronicInvoiceModel> Invoices => _billingService.InvoiceHistory;

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

    public ICommand ViewPdfCommand { get; }
    public ICommand SharePdfCommand { get; }
    public ICommand RefreshCommand { get; }

    private void LoadInvoices()
    {
        // Actualizar estado de vacío
        IsEmpty = !Invoices.Any();
        OnPropertyChanged(nameof(Invoices));
    }

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
            await _billingService.OpenInvoicePdfAsync(invoice.InvoiceHash, invoice.FullInvoiceNumber);
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
            await _billingService.ShareInvoicePdfAsync(invoice.InvoiceHash, invoice.FullInvoiceNumber);
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
}

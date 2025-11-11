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
        CreditNoteCommand = new Command<ElectronicInvoiceModel>(async (invoice) => await GenerateCreditNote(invoice)); // ✨ NEW
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
    public ICommand CreditNoteCommand { get; } // ✨ NEW
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

using APP.Eds.Services.Court;
using CommunityToolkit.Maui.Views;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using APP.Eds.Models.Court;

namespace APP.Eds.Components.PopUp;

public partial class AddCourtTypeOfCollection : Popup
{
    private readonly CourtService courtService;

    public class PaymentOption : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public required TypeOfCollectionCourtModel Type { get; init; }

        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set { _isSelected = value; PropertyChanged?.Invoke(this, new(nameof(IsSelected))); } }

        decimal _amount;
        public decimal Amount { get => _amount; set { _amount = value; PropertyChanged?.Invoke(this, new(nameof(Amount))); } }

        string _notes = string.Empty;
        public string Notes { get => _notes; set { _notes = value; PropertyChanged?.Invoke(this, new(nameof(Notes))); } }
    }

    public ObservableCollection<PaymentOption> PaymentOptions { get; } = new();

    public AddCourtTypeOfCollection(CourtService courtService)
    {
        InitializeComponent();
        this.courtService = courtService;

        if (courtService.TypeOfCollectionList is not null)
        {
            foreach (var t in courtService.TypeOfCollectionList)
                PaymentOptions.Add(new PaymentOption { Type = t, IsSelected = false, Amount = 0 });
        }
    }

    private void OnCloseTapped(object sender, EventArgs e)
    {
        Close();
    }

    private void Amount_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry)
        {
            var newText = e.NewTextValue ?? string.Empty;
            if (string.IsNullOrWhiteSpace(newText)) return;

            // Permitir formato local (ej: es-CO)
            if (!decimal.TryParse(newText, NumberStyles.Number, new CultureInfo("es-CO"), out _))
                entry.Text = e.OldTextValue; // revertir
        }
    }

    // Borrar todo (desmarca y limpia)
    private void Clear_All(object sender, EventArgs e)
    {
        foreach (var p in PaymentOptions)
        {
            p.IsSelected = false;
            p.Amount = 0m;
            p.Notes = string.Empty;
        }
    }

    // Guardar/Agregar
    private async void Add_Selected(object sender, EventArgs e)
    {
        var selected = PaymentOptions.Where(p => p.IsSelected).ToList();

        if (selected.Count == 0)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Selecciona al menos un método de pago.", "OK");
            return;
        }

        // Validaciones por cada método
        foreach (var p in selected)
        {
            if (p.Amount <= 0m)
            {
                await Application.Current.MainPage.DisplayAlert("Error",
                    $"El monto para '{p.Type.Description}' debe ser mayor a 0.", "OK");
                return;
            }
        }

        foreach (var p in selected)
        {
            courtService.SelectedTypeOfCollection = p.Type;
            courtService.CourtTypeOfCollectionAmount = (double)p.Amount;
            courtService.CourtTypeOfCollectionDescription = p.Notes;

            await courtService.AddCourtTypeOfCollectionFromPopup();
        }

        await CloseAsync();

    }
}

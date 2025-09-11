using APP.Eds.Services.StrongBox;
using Microsoft.Maui.Controls;
using System.ComponentModel;

namespace APP.Eds.UsesCases.StrongBox;

public partial class StrongBoxView : ContentPage
{
    private StrongBoxService _strongBoxService;

    public StrongBoxView()
    {
        InitializeComponent();
        _strongBoxService = new StrongBoxService();
        BindingContext = _strongBoxService;
        
        // Suscribirse a los cambios de IsLoading para mostrar/ocultar el loading
        _strongBoxService.PropertyChanged += OnStrongBoxServicePropertyChanged;
    }

    private void OnStrongBoxServicePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(StrongBoxService.IsLoading))
        {
            if (_strongBoxService.IsLoading)
            {
                LoadingOverlay.ShowLoading("Procesando retiro...", "Realizando operación en caja fuerte");
            }
            else
            {
                LoadingOverlay.HideLoading();
            }
        }
        else if (e.PropertyName == nameof(StrongBoxService.IsLoadingCourtDetails))
        {
            if (_strongBoxService.IsLoadingCourtDetails)
            {
                LoadingOverlay.ShowLoading("Cargando detalles del corte...", "Obteniendo información detallada del movimiento");
            }
            else
            {
                LoadingOverlay.HideLoading();
            }
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _strongBoxService?.LoadDataCommand.Execute(null);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Desuscribirse del evento para evitar memory leaks
        if (_strongBoxService != null)
        {
            _strongBoxService.PropertyChanged -= OnStrongBoxServicePropertyChanged;
        }
    }
}
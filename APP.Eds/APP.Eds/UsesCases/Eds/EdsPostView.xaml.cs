using System.Net;
using APP.Eds.Services.Eds;

namespace APP.Eds.UsesCases.Eds;

public partial class EdsPostView : ContentPage
{
    private EdsService _edsService;
    public EdsPostView()
    {
        InitializeComponent();
        _edsService = new EdsService();
        BindingContext = _edsService;
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        if (BindingContext is EdsService vm && vm.SelectedBusiness is not null)
        {
            try
            {
                LoadingOverlay.ShowLoading();
                var selectedId = vm.SelectedBusiness.IdBusiness;
                await vm.SaveEdsDataAsync();
            }
            finally
            {
                LoadingOverlay.HideLoading();

                Name = string.Empty;
                Nit = string.Empty;
                Address = string.Empty;
                Sicom = string.Empty;
                _edsService.SelectedBusiness = null;
            }
            
        }
        else
        {
            await DisplayAlert("Error", "Por favor, seleccione un Negocio", "OK");
        }
    }

    public string Name
    {
        get => _edsService.Name;
        set
        {
            _edsService.Name = value;
            OnPropertyChanged();
        }
    }
    public string Nit
    {
        get => _edsService.Nit;
        set
        {
            _edsService.Nit = value;
            OnPropertyChanged();
        }
    }
    public string Address
    {
        get => _edsService.Address;
        set
        {
            _edsService.Address = value;
            OnPropertyChanged();
        }
    }
    public string Sicom
    {
        get => _edsService.Sicom;
        set
        {
            _edsService.Sicom = value;
            OnPropertyChanged();
        }
    }
}
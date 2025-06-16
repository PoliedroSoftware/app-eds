using System.Xml.Linq;
using APP.Eds.Services.Provider;

namespace APP.Eds.UsesCases.Provider;

public partial class ProviderPostView : ContentPage
{
    private ProviderService _providerService;
    public ProviderPostView()
    {
        InitializeComponent();
        _providerService = new ProviderService();
        BindingContext = _providerService;
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        try
        {
            LoadingOverlay.ShowLoading();
            await _providerService.SaveProviderDataAsync();
        }
        finally
        {
            LoadingOverlay.HideLoading();

            Name = string.Empty;
        }
        
    }

    public string Name
    {
        get => _providerService.Name;
        set
        {
            _providerService.Name = value;
            OnPropertyChanged();
        }
    }
}
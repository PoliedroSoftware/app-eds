using System.ComponentModel;
using APP.Eds.Services.Business;

namespace APP.Eds.UsesCases.Business;

public partial class BusinessPostView : ContentPage
{
    private BusinessService _businessService;

    public BusinessPostView()
    {
        InitializeComponent();
        _businessService = new BusinessService();
        BindingContext = _businessService;
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        try
        {
            LoadingOverlay.ShowLoading();
            await _businessService.SaveBusinessDataAsync();
        }
        finally
        {
            LoadingOverlay.HideLoading();

            Name = string.Empty;
        }
    }

    public string Name
    {
        get => _businessService.Name;
        set
        {
            _businessService.Name = value;
            OnPropertyChanged();
        }
    }
}


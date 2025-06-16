using APP.Eds.Services.Expenditures;

namespace APP.Eds.UsesCases.Expenditures;

public partial class ExpendituresPostView : ContentPage
{
    private ExpendituresService _expendituresService;
    public ExpendituresPostView()
	{
		InitializeComponent();
        _expendituresService = new ExpendituresService();
        BindingContext = _expendituresService;
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        try
        {
            LoadingOverlay.ShowLoading();
            await _expendituresService.SaveExpendituresDataAsync();
        }
        finally
        {
            LoadingOverlay.HideLoading();

            Description = string.Empty;
        }
        
    }

    public string Description
    {
        get => _expendituresService.Description;
        set
        {
            _expendituresService.Description = value;
            OnPropertyChanged();
        }
    }
}
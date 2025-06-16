using APP.Eds.Services.TypeOfCollection;

namespace APP.Eds.UsesCases.TypeOfCollection;

public partial class TypeOfCollectionPostView : ContentPage
{
    private TypeOfCollectionService _typeOfCollectionService;
    public TypeOfCollectionPostView()
	{
		InitializeComponent();
        _typeOfCollectionService = new TypeOfCollectionService();
        BindingContext = _typeOfCollectionService;
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        try
        {
            LoadingOverlay.ShowLoading();
            await _typeOfCollectionService.SaveTypeOfCollectionDataAsync();
        }
        finally
        {
            LoadingOverlay.HideLoading();

            Description = string.Empty;
        }
        
    }

    public string Description
    {
        get => _typeOfCollectionService.Description;
        set
        {
            _typeOfCollectionService.Description = value;
            OnPropertyChanged();
        }
    }
}
using System.Text.RegularExpressions;
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
            if (string.IsNullOrWhiteSpace(_typeOfCollectionService.Description))
            {
                await DisplayAlert("Error", $"{_typeOfCollectionService.ErrorEmpty}!", "OK");
                return;
            }
            if (!Regex.IsMatch(_typeOfCollectionService.Description, @"^\p{L}+$"))
            {
                await DisplayAlert("Error", $"{_typeOfCollectionService.ErrorCharacteres}!\n{_typeOfCollectionService.Description}", "OK");
                return;
            }
            await _typeOfCollectionService.SaveTypeOfCollectionDataAsync();
        }
        finally
        {
            LoadingOverlay.HideLoading();

            _typeOfCollectionService.Description = string.Empty;
        }  
    }
}
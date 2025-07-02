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
            if (string.IsNullOrWhiteSpace(Description))
            {
                await DisplayAlert("Error", $"El tipo de colección {Description} no puede estar vacio !", "OK");
                return;
            }
            if (!Regex.IsMatch(Description, @"^\p{L}+$"))
            {
                await DisplayAlert("Error", $"El tipo de colección {Description} no debe contener caracteres especiales !", "OK");
                return;
            }
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
using APP.Eds.Models.DispenserType;
using APP.Eds.Services.DispenserType;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace APP.Eds.UsesCases.DispenserType;

public partial class DispenserTypePostView : ContentPage
{
    private DispenserTypeService _dispenserTypeService;
    public ObservableCollection<DispenserTypeModels> DispenserTypeList { get; set; }
    public ICommand EditDispenserTypeCommand { get; private set; }
    public ICommand DeleteDispenserTypeCommand { get; private set; }
    public DispenserTypePostView()
	{
		InitializeComponent();
        _dispenserTypeService = new DispenserTypeService();
        DispenserTypeList = new ObservableCollection<DispenserTypeModels>();
        EditDispenserTypeCommand = new Command<DispenserTypeModels>(EditDispenserType);
        DeleteDispenserTypeCommand = new Command<DispenserTypeModels>(DeleteDispenserType);
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDispenserTypes();
    }

    private async Task LoadDispenserTypes()
    {
        LoadingOverlay.ShowLoading();
        var dispenserTypes = await _dispenserTypeService.GetDispenserTypesAsync();
        DispenserTypeList.Clear();
        foreach (var item in dispenserTypes)
        {
            DispenserTypeList.Add(item);
        }
        LoadingOverlay.HideLoading();
    }


    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        try
        {
            LoadingOverlay.ShowLoading();
            _dispenserTypeService.Description = this.Description;
            await _dispenserTypeService.SaveDispenserTypeDataAsync();
            Description = string.Empty;
            await LoadDispenserTypes();
        }
        finally
        {
            LoadingOverlay.HideLoading();
        }
        
    }

    private void EditDispenserType(DispenserTypeModels dispenserType)
    {
        Description = dispenserType.Description;
        _dispenserTypeService.Id = dispenserType.Id;
    }

    private async void DeleteDispenserType(DispenserTypeModels dispenserType)
    {
        bool answer = await DisplayAlert("Eliminar", $"¿Está seguro de que desea eliminar el tipo de dispensador '{dispenserType.Description}'?", "Sí", "No");
        if (answer)
        {
            LoadingOverlay.ShowLoading();
            await _dispenserTypeService.DeleteDispenserTypeAsync(dispenserType.Id);
            await LoadDispenserTypes();
            LoadingOverlay.HideLoading();
        }
    }


    private string _description;
    public string Description
    {
        get => _description;
        set
        {
            _description = value;
            OnPropertyChanged();
        }
    }
}
using APP.Eds.Models.DispenserType;
using APP.Eds.Services.DispenserType;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using APP.Eds.Components.PopUp; // Agregado para CustomAlert
using APP.Eds.UsesCases.LoadingView; // Agregado para LoadingOverlay
 
 namespace APP.Eds.UsesCases.DispenserType;

public partial class DispenserTypePostView : ContentPage, INotifyPropertyChanged
{
    private DispenserTypeService _dispenserTypeService;

    public DispenserTypePostView()
    {
        InitializeComponent();
        _dispenserTypeService = new DispenserTypeService();
        DispenserTypeList = new ObservableCollection<DispenserTypeModel>();
        EditDispenserTypeCommand = new Command<DispenserTypeModel>(EditDispenserType);
        DeleteDispenserTypeCommand = new Command<DispenserTypeModel>(DeleteDispenserType);
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
        var button = sender as Button;
        try
        {
            // Disable button to prevent multiple submissions
            if (button != null)
            {
                button.IsEnabled = false;
                button.Text = "Guardando...";
            }

            // Enhanced validation with professional alerts
            if (string.IsNullOrWhiteSpace(_dispenserTypeService.Description))
            {
                await CustomAlert.ShowErrorAsync("La descripción del tipo de dispensador es obligatoria para el registro", "Descripción Requerida");
                return;
            }

            // Additional validation for minimum length
            if (_dispenserTypeService.Description.Length < 3)
            {
                await CustomAlert.ShowErrorAsync("La descripción debe tener al menos 3 caracteres para ser válida", "Descripción Muy Corta");
                return;
            }

            if (_dispenserTypeService.Description.Length > 100)
            {
                await CustomAlert.ShowErrorAsync("La descripción no puede exceder 100 caracteres", "Descripción Muy Larga");
                return;
            }

            // Check for special characters or inappropriate content
            if (_dispenserTypeService.Description.Trim() != _dispenserTypeService.Description)
            {
                await CustomAlert.ShowWarningAsync("La descripción contiene espacios al inicio o final que serán removidos automáticamente", "Espacios Detectados");
                _dispenserTypeService.Description = _dispenserTypeService.Description.Trim();
            }

            LoadingOverlay.ShowLoading();
            _dispenserTypeService.Description = this.Description;
            await _dispenserTypeService.SaveDispenserTypeDataAsync();

        }
        finally
        {
            LoadingOverlay.HideLoading();

        }
    }

    private void EditDispenserType(DispenserTypeModel dispenserType)
    {
        Description = dispenserType.Description;
        _dispenserTypeService.Id = dispenserType.Id;
    }

    private async void DeleteDispenserType(DispenserTypeModel dispenserType)
    {
        bool answer = await DisplayAlert("Eliminar", $"Â¿EstÃ¡ seguro de que desea eliminar el tipo de dispensador '{dispenserType.Description}'?", "SÃ­", "No");
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
            if (_description != value)
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }
    }

    public ObservableCollection<DispenserTypeModel> DispenserTypeList { get; set; }
    public ICommand EditDispenserTypeCommand { get; set; }
    public ICommand DeleteDispenserTypeCommand { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
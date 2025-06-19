using APP.Eds.Components.PopUp;
using APP.Eds.Models.Court;
using APP.Eds.Models.Dispensers;
using APP.Eds.Models.Eds;
using APP.Eds.Services.Court;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace APP.Eds.UsesCases.Court;

public partial class CourtPostView : ContentPage
{
	private CourtService _service;
    public string UserRole { get; set; } = string.Empty;
    public CourtPostView()
	{

        InitializeComponent();
        _service = CourtService.Instance;
        _service.DateStarttime = DateTime.Today;
        BindingContext = _service;
        datePicker.MinimumDate = new DateTime(1900, 1, 1);

        Task.Run(async () => await _service.LoadTranslationsAsync());

        UserRole = Preferences.Get("userRole", string.Empty);

        string configJson = Preferences.Get("userConfig", "{}");
        var config = JsonSerializer.Deserialize<Dictionary<string, bool>>(configJson);


        if (config != null)
        {
            ApplyConfig(config);
        }

    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            LoadingOverlay.ShowLoading();
            MainContent.IsVisible = false;
            await _service.GetAllEdsData();
            await _service.LoadTranslationsAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al cargar datos: {ex.Message}", "OK");
        }
        finally
        {
            LoadingOverlay.HideLoading();
            MainContent.IsVisible = true;
        }
    }

    private void ApplyConfig(Dictionary<string, bool> config)
    {
        foreach (var kvp in config)
        {
            var element = this.FindByName<VisualElement>(kvp.Key);
            if (element != null)
            {
                element.IsVisible = kvp.Value;
            }
        }
    }
    private void OpenDispenserPopUp(object sender, EventArgs e)
    {
        this.ShowPopup(new AddDispenser(_service));
    }
    private void OpenAditionalInfoPopUp(object sender, EventArgs e)
    {
        this.ShowPopup(new AddInfo(_service));
    }
    private void OpenDocumentPopUp(object sender, EventArgs e)
    {
        this.ShowPopup(new AddDocuemt(_service));
    }
    private void OpenExpenditurePopUp(object sender, EventArgs e)
    {
        this.ShowPopup(new AddCourtExpenditure(_service));
    }
    private void OpenTypeOfCollectionPopUp(object sender, EventArgs e)
    {
        this.ShowPopup(new AddCourtTypeOfCollection(_service));
    }

    private async void OnShowCourtListClicked(object sender, EventArgs e)
    { 
            await Navigation.PushAsync(new CourtListView());       
    }
    private async void Button_Clicked(object sender, EventArgs e)
    {
        if (BindingContext is CourtService vm)
        {
            if (vm.SelectedBusiness is null)
            {
                await DisplayAlert("Error", "Por favor, seleccione un Negocio", "OK");
                return;
            }
            if (vm.SelectedEds is null)
            {
                await DisplayAlert("Error", "Por favor, seleccione un EDS", "OK");
                return;
            }
            if (vm.SelectedIslander is null)
            {
                await DisplayAlert("Error", "Por favor, seleccione un Isle�o", "OK");
                return;
            }

                var selectedId = vm.SelectedEds.IdEds;
                await vm.SendCourtDataAsync();
                CourtService.ResetInstanceFields();
                _service = CourtService.Instance;
                BindingContext = _service;

        }  
     }
        
    
    private void OnBusinessSelected(object sender, EventArgs e)
    {
        var picker = sender as Picker;
        if (picker == null) return;
        Debug.WriteLine($"Tipo de SelectedItem: {picker.SelectedItem?.GetType()}");
        Dispatcher.Dispatch(() =>
        {
            if (picker.SelectedItem is BusinessModel selectedBusiness)
            {
                int businessId = selectedBusiness.IdBusiness;
                Debug.WriteLine($"Negocio seleccionado: {selectedBusiness.Name}, ID: {businessId}");
                _service.LoadEdsByBusiness(businessId);
                _service.IslanderSelectList.Clear();
                _service.IsBusinessSelected = true;
            }
            else
            {
                Debug.WriteLine("El SelectedItem no es del tipo esperado o es null");
            }
        });
    }

    private void OnEdsSelected(object sender, EventArgs e)
    {
        var picker = sender as Picker;
        if (picker == null) return;
        Debug.WriteLine($"Tipo de SelectedItem: {picker.SelectedItem?.GetType()}");
        Dispatcher.Dispatch(() =>
        {
            if (picker.SelectedItem is EdsCourtModel selectedEds)
            {
                int edsId = selectedEds.IdEds;
                Debug.WriteLine($"Eds seleccionado: {selectedEds.Name}, ID: {edsId}");
                _service.LoadIslandersByEds(edsId);
                _service.IsEdsSelected = true;
            }
            else
            {
                Debug.WriteLine("El SelectedItem no es del tipo esperado o es null");
            }
        });
    }

    private void OnIslanderSelected(object sender, EventArgs e)
    {
        var picker = sender as Picker;
        if (picker == null) return;
        Debug.WriteLine($"Tipo de SelectedItem: {picker.SelectedItem?.GetType()}");
        Dispatcher.Dispatch(() =>
        {
            if (picker.SelectedItem is IslanderResponse selectedIslander)
            {
                int islanderId = selectedIslander.IdIslander;
                Debug.WriteLine($"Eds seleccionado: {selectedIslander.Name}, ID: {islanderId}");
                _service.LoadEdsByBusiness(islanderId);
            }
            else
            {
                Debug.WriteLine("El SelectedItem no es del tipo esperado o es null");
            }
        });
    }

}
using APP.Eds.Models.Business;
using APP.Eds.Services.Business;

namespace APP.Eds.UsesCases.Business;

public partial class BusinessListView : ContentPage
{
    private BusinessListViewModel _viewModel;

    public BusinessListView()
    {
        InitializeComponent();
        _viewModel = new BusinessListViewModel();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            LoadingOverlay.ShowLoading();
            await _viewModel.LoadBusinessesAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al cargar negocios: {ex.Message}", "OK");
        }
        finally
        {
            LoadingOverlay.HideLoading();
        }
    }

    private async void OnBusinessSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is BusinessModel selectedBusiness)
        {
            // Execute the view detail command
            if (_viewModel.ViewBusinessDetailCommand.CanExecute(selectedBusiness))
            {
                _viewModel.ViewBusinessDetailCommand.Execute(selectedBusiness);
            }

            // Clear selection
            ((CollectionView)sender).SelectedItem = null;
        }
    }
}
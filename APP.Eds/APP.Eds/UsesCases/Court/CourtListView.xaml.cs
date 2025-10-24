using APP.Eds.Models.Court;
using APP.Eds.Services.Court;

namespace APP.Eds.UsesCases.Court;

public partial class CourtListView : ContentPage
{
    private readonly CourtListService _courtListService;

    public CourtListView()
    {
        InitializeComponent();
        _courtListService = new CourtListService();
        BindingContext = _courtListService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            LoadingOverlay.ShowLoading();
            await _courtListService.LoadAllCourtListAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al cargar datos: {ex.Message}", "OK");
        }
        finally
        {
            LoadingOverlay.HideLoading();
        }
    }

    private void OnCourtSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is CourtListItemModel selectedCourt)
        {
            // Use the CourtListService command to open court detail
            _courtListService.OpenCourtDetailCommand.Execute(selectedCourt);

            ((CollectionView)sender).SelectedItem = null;
        }
    }
}

using APP.Eds.Models.Court;
using APP.Eds.Services.Court;

namespace APP.Eds.UsesCases.Court;

public partial class CourtListView : ContentPage
{
    public CourtListView()
    {

        InitializeComponent();
        BindingContext = new CourtService();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is CourtService courtService)
        {
            try
            {
                LoadingOverlay.ShowLoading();
                await courtService.LoadAllCourtListAsync();
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
    }

    private void OnCourtSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is CourtListItemModel selectedCourt)
        {
            //CourtService.Instance.OpenCourtDetailCommand.Execute(selectedCourt);

            ((CollectionView)sender).SelectedItem = null;
        }
    }
}

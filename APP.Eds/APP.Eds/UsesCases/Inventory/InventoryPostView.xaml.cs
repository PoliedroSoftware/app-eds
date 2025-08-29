using APP.Eds.Models.Inventory;
using APP.Eds.UsesCases.Inventory;
using APP.Eds.Components.PopUp;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace APP.Eds.UsesCases.Inventory
{
    public partial class InventoryPostView : ContentPage
    {
        public InventoryViewModel ViewModel { get; set; }

        public InventoryPostView()
        {
            InitializeComponent();
            ViewModel = new InventoryViewModel();
            BindingContext = ViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                LoadingOverlay.ShowLoading();
                if (ViewModel != null)
                    await ViewModel.LoadDataAsync();
            }
            catch (Exception ex)
            {
                await CustomAlert.ShowErrorAsync($"Error al cargar los datos del inventario:\n\n{ex.Message}", "Error de Carga");
            }
            finally
            {
                LoadingOverlay.HideLoading();
            }
        }
    }
}

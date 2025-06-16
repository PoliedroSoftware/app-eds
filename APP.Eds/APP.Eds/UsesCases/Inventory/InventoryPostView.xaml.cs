using APP.Eds.Models.Inventory;
using APP.Eds.UsesCases.Inventory;
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
    }

}

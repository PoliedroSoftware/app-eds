using APP.Eds.Services.Navigation;
using CommunityToolkit.Maui.Views;

namespace APP.Eds.Views.Popups
{
    public partial class CategoryPopup : Popup
    {
        const double ItemHeight = 60;
        const double HeaderHeight = 60;
        const double MaxFrameHeight = 600;
        public CategoryPopup(List<MainService.MenuItemModel> items, string categoryTitle)
        {
            InitializeComponent();
            CategoryTitle.Text = categoryTitle;
            MenuItemsList.ItemsSource = items;
            UpdateHeight(items.Count);
        }

        public void UpdateHeight(int itemCount)
        {
            double totalHeight = HeaderHeight + (ItemHeight * itemCount);

            double frameHeight = Math.Min(totalHeight, MaxFrameHeight);

            double collectionHeight = frameHeight - HeaderHeight - 10;

            MenuItemsList.HeightRequest = collectionHeight;
            Frame.HeightRequest = frameHeight;
        }

        private void OnCloseTapped(object sender, EventArgs e)
        {
            
            Close();
        }

        private async void OnMenuItemClicked(object sender, EventArgs e)
        {
            // Cerrar popup
            Close();
        }

    }
}

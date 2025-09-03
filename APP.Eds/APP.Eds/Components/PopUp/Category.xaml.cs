using APP.Eds.Services.Navigation;
using CommunityToolkit.Maui.Views;

namespace APP.Eds.Views.Popups
{
    public partial class CategoryPopup : Popup
    {
        const double ItemHeight = 58;
        const double HeaderHeight = 80;
        const double MaxFrameHeight = 500;
        const double Padding = 40;
        
        public CategoryPopup(List<MainService.MenuItemModel> items, string categoryTitle)
        {
            InitializeComponent();
            CategoryTitle.Text = categoryTitle;
            MenuItemsList.ItemsSource = items;
            UpdateHeight(items.Count);
            
            // Inicializar con animación de entrada
            _ = AnimateEntry();
        }

        public void UpdateHeight(int itemCount)
        {
            // Calcular altura dinámica basada en el contenido
            double contentHeight = (ItemHeight * itemCount) + Padding;
            double totalHeight = HeaderHeight + contentHeight;

            // Limitar la altura máxima
            double frameHeight = Math.Min(totalHeight, MaxFrameHeight);

            // Si el contenido excede el máximo, habilitar scroll
            if (totalHeight > MaxFrameHeight)
            {
                MenuItemsList.HeightRequest = MaxFrameHeight - HeaderHeight - Padding;
            }
            else
            {
                MenuItemsList.HeightRequest = contentHeight;
            }

            Frame.HeightRequest = frameHeight;
        }

        private async Task AnimateEntry()
        {
            // Iniciar con escala pequeña y transparente
            Frame.Scale = 0.8;
            Frame.Opacity = 0;
            Frame.TranslationY = 50;
            
            // Animar entrada con efecto más dinámico
            await Task.WhenAll(
                Frame.ScaleTo(1, 400, Easing.SpringOut),
                Frame.FadeTo(1, 250),
                Frame.TranslateTo(0, 0, 300, Easing.CubicOut)
            );
        }

        private async void OnCloseTapped(object sender, EventArgs e)
        {
            try
            {
                await AnimateExit();                
                try
                {
                    Close();
                }
                catch (ObjectDisposedException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"CategoryPopup was already disposed during close: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CategoryPopup OnCloseTapped: {ex.Message}");
            }
        }

        private async void OnMenuItemClicked(object sender, EventArgs e)
        {
            try
            {
                try
                {
#if ANDROID || IOS
                    HapticFeedback.Perform(HapticFeedbackType.Click);
#endif
                }
                catch { }
                
                if (sender is Border border)
                {
                    _ = Task.Run(async () =>
                    {
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            // Crear un efecto de "pulso"
                            await border.ScaleTo(0.92, 80, Easing.CubicOut);
                            await border.ScaleTo(1.02, 80, Easing.CubicOut);
                            await border.ScaleTo(1, 80, Easing.CubicOut);
                        });
                    });
                }

                await Task.Delay(180);
                await AnimateExit();
                try
                {
                    Close();
                }
                catch (ObjectDisposedException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"CategoryPopup was already disposed during menu item close: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CategoryPopup OnMenuItemClicked: {ex.Message}");
            }
        }

        private async Task AnimateExit()
        {
            await Task.WhenAll(
                Frame.ScaleTo(0.85, 200, Easing.CubicIn),
                Frame.FadeTo(0, 150),
                Frame.TranslateTo(0, -30, 200, Easing.CubicIn)
            );
        }
    }
}

using APP.Eds.Services.Navigation;
using CommunityToolkit.Maui.Views;

namespace APP.Eds.Views.Popups
{
    public partial class CategoryPopup : Popup
    {
        const double ItemHeight = 64;
        const double HeaderHeight = 100;
        const double MaxFrameHeight = 520;
        const double Padding = 52;
        
        public CategoryPopup(List<MainService.MenuItemModel> items, string categoryTitle)
        {
            InitializeComponent();
            CategoryTitle.Text = categoryTitle;
            MenuItemsList.ItemsSource = items;
            UpdateHeight(items.Count);
            
            // Inicializar con animación de entrada mejorada
            _ = AnimateEntry();
        }

        public void UpdateHeight(int itemCount)
        {
            // Para altura automática, no establecemos HeightRequest en el Frame
            // El popup se ajustará automáticamente al contenido
            System.Diagnostics.Debug.WriteLine($"Popup auto-sizing for {itemCount} items");
            
            // Si hay muchos items, podríamos mostrar información sobre scroll potencial
            if (itemCount > 8) // Aproximadamente cuando empezaría a necesitar scroll con MaximumHeightRequest="600"
            {
                System.Diagnostics.Debug.WriteLine($"Popup may need scrolling for {itemCount} items");
            }
            
            // Información de debug sobre la altura esperada
            const double ItemHeight = 58;
            const double HeaderHeight = 85;
            const double Padding = 40;
            
            double expectedContentHeight = (ItemHeight * itemCount) + Padding;
            double expectedTotalHeight = HeaderHeight + expectedContentHeight;
            
            System.Diagnostics.Debug.WriteLine($"Expected total height: {expectedTotalHeight}px for {itemCount} items (max: 600px)");
        }

        private async Task AnimateEntry()
        {
            // Asegurar centrado perfecto antes de la animación
            await EnsureCentering();
            
            // Iniciar con escala pequeña y transparente, sin desplazamiento que pueda afectar el centrado
            Frame.Scale = 0.8;
            Frame.Opacity = 0;
            Frame.TranslationY = 0; // Mantener centrado, sin desplazamiento
            Frame.TranslationX = 0; // Asegurar que esté centrado horizontalmente
            Frame.Rotation = 0; // Sin rotación para mantener centrado
            
            // Animar entrada centrada con efectos más suaves
            await Task.WhenAll(
                Frame.ScaleTo(1, 400, Easing.SpringOut),
                Frame.FadeTo(1, 300, Easing.CubicOut)
            );
        }

        /// <summary>
        /// Método auxiliar para asegurar centrado perfecto en Android y otras plataformas
        /// </summary>
        private async Task EnsureCentering()
        {
            try
            {
                await Task.Delay(50); // Permitir que el layout se establezca

                // Verificar y ajustar el centrado si es necesario
                var mainPage = Application.Current?.MainPage;
                if (mainPage != null)
                {
                    var screenWidth = mainPage.Width;
                    var screenHeight = mainPage.Height;
                    
                    if (screenWidth > 0 && screenHeight > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"Screen dimensions: {screenWidth}x{screenHeight}");
                        System.Diagnostics.Debug.WriteLine($"Popup should be centered with auto-height");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in EnsureCentering: {ex.Message}");
            }
        }

        private async void OnCloseTapped(object sender, EventArgs e)
        {
            try
            {
                // Efecto visual inmediato para feedback del usuario (ahora es un Button)
                if (sender is Button closeButton)
                {
                    _ = Task.Run(async () =>
                    {
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            await closeButton.ScaleTo(0.8, 100, Easing.CubicOut);
                            await closeButton.ScaleTo(1, 100, Easing.CubicOut);
                        });
                    });
                }

                await Task.Delay(50);
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
                // Feedback háptico mejorado
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
                            // Efecto de selección más elegante
                            await Task.WhenAll(
                                border.ScaleTo(0.95, 80, Easing.CubicOut),
                                border.FadeTo(0.7, 80)
                            );
                            await Task.WhenAll(
                                border.ScaleTo(1.02, 100, Easing.SpringOut),
                                border.FadeTo(1, 100)
                            );
                            await border.ScaleTo(1, 80, Easing.CubicOut);
                        });
                    });
                }

                await Task.Delay(200);
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
            // Animación de salida manteniendo el centrado
            await Task.WhenAll(
                Frame.ScaleTo(0.85, 200, Easing.CubicIn),
                Frame.FadeTo(0, 150, Easing.CubicIn)
            );
        }
    }
}

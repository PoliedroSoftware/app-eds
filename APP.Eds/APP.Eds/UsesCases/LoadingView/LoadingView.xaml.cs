using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APP.Eds.UsesCases.LoadingView
{
    public partial class LoadingView : ContentView
    {
        private bool _isAnimating = false;
        private CancellationTokenSource _animationCancellation;

        public LoadingView()
        {
            InitializeComponent();
        }

        public async void ShowLoading(string loadingText = "Cargando...", string description = "Procesando informacion")
        {
            try
            {
                // Update texts
                LoadingLabel.Text = loadingText;
                LoadingDescription.Text = description;

                // Show the overlay
                LoadingOverlay.IsVisible = true;
                LoadingIndicator.IsRunning = true;
                LoadingLabel.IsVisible = true;
                
                // Start entrance animation
                await AnimateEntrance();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ShowLoading: {ex.Message}");
                // Fallback to basic visibility
                LoadingOverlay.IsVisible = true;
                LoadingIndicator.IsRunning = true;
                LoadingLabel.IsVisible = true;
            }
        }

        public async void HideLoading()
        {
            try
            {
                // Stop animations
                LoadingIndicator.IsRunning = false;

                // Animate exit
                await AnimateExit();

                // Hide overlay
                LoadingOverlay.IsVisible = false;
                LoadingLabel.IsVisible = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in HideLoading: {ex.Message}");
                // Fallback to basic visibility
                LoadingOverlay.IsVisible = false;
                LoadingIndicator.IsRunning = false;
                LoadingLabel.IsVisible = false;
            }
        }

        private async Task AnimateEntrance()
        {
            try
            {
                // Reset container state
                LoadingContainer.Scale = 0.8;
                LoadingContainer.Opacity = 0;

                // Animate container entrance with spring effect
                var scaleAnimation = LoadingContainer.ScaleTo(1.05, 300, Easing.SpringOut);
                var fadeAnimation = LoadingContainer.FadeTo(1, 250, Easing.CubicOut);

                await Task.WhenAll(scaleAnimation, fadeAnimation);

                // Subtle bounce back
                await LoadingContainer.ScaleTo(1, 100, Easing.CubicOut);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AnimateEntrance: {ex.Message}");
            }
        }

        private async Task AnimateExit()
        {
            try
            {
                // Animate container exit
                var scaleAnimation = LoadingContainer.ScaleTo(0.9, 200, Easing.CubicIn);
                var fadeAnimation = LoadingContainer.FadeTo(0, 200, Easing.CubicIn);

                await Task.WhenAll(scaleAnimation, fadeAnimation);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AnimateExit: {ex.Message}");
            }
        }

        // Enhanced overloads for different loading scenarios
        public void ShowLoadingWithCustomMessage(string message, string description = null)
        {
            ShowLoading(message, description ?? "Por favor espere...");
        }

        public void ShowSavingLoader()
        {
            ShowLoading("Guardando datos...", "Procesando informacion");
        }

        public void ShowLoadingData()
        {
            ShowLoading("Cargando datos...", "Obteniendo informacion del servidor");
        }

        public void ShowProcessingLoader()
        {
            ShowLoading("Procesando...", "Ejecutando operacion");
        }

        public void ShowSyncingLoader()
        {
            ShowLoading("Sincronizando...", "Actualizando informacion");
        }

        public void ShowValidatingLoader()
        {
            ShowLoading("Validando...", "Verificando informacion");
        }

        public void ShowDeletingLoader()
        {
            ShowLoading("Eliminando...", "Procesando solicitud");
        }

        public void ShowUpdatingLoader()
        {
            ShowLoading("Actualizando...", "Guardando cambios");
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();
            
            // Clean up when view is removed
            if (Parent == null)
            {
                _isAnimating = false;
                _animationCancellation?.Cancel();
            }
        }
    }
}

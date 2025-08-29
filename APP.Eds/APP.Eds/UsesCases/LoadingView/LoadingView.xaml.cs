using System;
using System.Threading;
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
                
                // Start continuous animations
                StartContinuousAnimations();
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
                _animationCancellation?.Cancel();

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
                LoadingContainer.TranslationY = 30;

                // Elegant entrance animation with staggered effects
                await Task.WhenAll(
                    LoadingContainer.ScaleTo(1.0, 500, Easing.SpringOut),
                    LoadingContainer.FadeTo(1, 350, Easing.CubicOut),
                    LoadingContainer.TranslateTo(0, 0, 400, Easing.CubicOut)
                );

                // Subtle bounce for premium feel
                await LoadingContainer.ScaleTo(1.05, 150, Easing.CubicOut);
                await LoadingContainer.ScaleTo(1.0, 150, Easing.CubicIn);
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
                // Smooth exit animation
                await Task.WhenAll(
                    LoadingContainer.ScaleTo(0.9, 250, Easing.CubicIn),
                    LoadingContainer.FadeTo(0, 200, Easing.CubicIn),
                    LoadingContainer.TranslateTo(0, -20, 250, Easing.CubicIn)
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AnimateExit: {ex.Message}");
            }
        }

        private void StartContinuousAnimations()
        {
            try
            {
                _animationCancellation = new CancellationTokenSource();
                var token = _animationCancellation.Token;

                // Start spinner rotation
                Task.Run(async () =>
                {
                    try
                    {
                        while (!token.IsCancellationRequested && LoadingOverlay?.IsVisible == true)
                        {
                            await MainThread.InvokeOnMainThreadAsync(async () =>
                            {
                                try
                                {
                                    var progressRing = this.FindByName<Microsoft.Maui.Controls.Shapes.Ellipse>("ProgressRing");
                                    if (progressRing != null)
                                    {
                                        await progressRing.RotateTo(progressRing.Rotation + 360, 2000, Easing.Linear);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error in spinner animation: {ex.Message}");
                                }
                            });
                            
                            if (token.IsCancellationRequested) break;
                            await Task.Delay(50, token);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in spinner task: {ex.Message}");
                    }
                }, token);

                // Start dot animation
                Task.Run(async () =>
                {
                    try
                    {
                        while (!token.IsCancellationRequested && LoadingOverlay?.IsVisible == true)
                        {
                            await MainThread.InvokeOnMainThreadAsync(async () =>
                            {
                                try
                                {
                                    await AnimateDots();
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error in dot animation: {ex.Message}");
                                }
                            });
                            
                            if (token.IsCancellationRequested) break;
                            await Task.Delay(1500, token);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in dot animation task: {ex.Message}");
                    }
                }, token);

                // Start progress bar animation
                Task.Run(async () =>
                {
                    try
                    {
                        while (!token.IsCancellationRequested && LoadingOverlay?.IsVisible == true)
                        {
                            await MainThread.InvokeOnMainThreadAsync(async () =>
                            {
                                try
                                {
                                    var progressBar = this.FindByName<Microsoft.Maui.Controls.Shapes.Rectangle>("ProgressBar");
                                    if (progressBar != null)
                                    {
                                        await progressBar.TranslateTo(-80, 0, 0);
                                        await progressBar.TranslateTo(80, 0, 2000, Easing.SinInOut);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error in progress bar animation: {ex.Message}");
                                }
                            });
                            
                            if (token.IsCancellationRequested) break;
                            await Task.Delay(500, token);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in progress bar task: {ex.Message}");
                    }
                }, token);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting continuous animations: {ex.Message}");
            }
        }

        private async Task AnimateDots()
        {
            try
            {
                var dot1 = this.FindByName<Microsoft.Maui.Controls.Shapes.Ellipse>("Dot1");
                var dot2 = this.FindByName<Microsoft.Maui.Controls.Shapes.Ellipse>("Dot2");
                var dot3 = this.FindByName<Microsoft.Maui.Controls.Shapes.Ellipse>("Dot3");

                if (dot1 != null)
                {
                    dot1.Fill = Color.FromArgb("#6366F1");
                    await Task.WhenAll(dot1.ScaleTo(1.4, 150), dot1.FadeTo(1, 150));
                    await Task.WhenAll(dot1.ScaleTo(1.0, 150), dot1.FadeTo(0.3, 150));
                    dot1.Fill = Color.FromArgb("#CBD5E1");
                }

                await Task.Delay(200);

                if (dot2 != null)
                {
                    dot2.Fill = Color.FromArgb("#8B5CF6");
                    await Task.WhenAll(dot2.ScaleTo(1.4, 150), dot2.FadeTo(1, 150));
                    await Task.WhenAll(dot2.ScaleTo(1.0, 150), dot2.FadeTo(0.3, 150));
                    dot2.Fill = Color.FromArgb("#CBD5E1");
                }

                await Task.Delay(200);

                if (dot3 != null)
                {
                    dot3.Fill = Color.FromArgb("#EC4899");
                    await Task.WhenAll(dot3.ScaleTo(1.4, 150), dot3.FadeTo(1, 150));
                    await Task.WhenAll(dot3.ScaleTo(1.0, 150), dot3.FadeTo(0.3, 150));
                    dot3.Fill = Color.FromArgb("#CBD5E1");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error animating dots: {ex.Message}");
            }
        }

        // Enhanced overloads for different loading scenarios
        public void ShowLoadingWithCustomMessage(string message, string description = null)
        {
            ShowLoading(message, description ?? "Por favor espere...");
        }

        public void ShowSavingLoader()
        {
            ShowLoading("Guardando datos...", "Almacenando información de forma segura");
        }

        public void ShowLoadingData()
        {
            ShowLoading("Cargando datos...", "Obteniendo información del servidor");
        }

        public void ShowProcessingLoader()
        {
            ShowLoading("Procesando...", "Ejecutando operación solicitada");
        }

        public void ShowSyncingLoader()
        {
            ShowLoading("Sincronizando...", "Actualizando información en tiempo real");
        }

        public void ShowValidatingLoader()
        {
            ShowLoading("Validando...", "Verificando integridad de datos");
        }

        public void ShowDeletingLoader()
        {
            ShowLoading("Eliminando...", "Procesando solicitud de eliminación");
        }

        public void ShowUpdatingLoader()
        {
            ShowLoading("Actualizando...", "Guardando cambios realizados");
        }

        public void ShowConnectingLoader()
        {
            ShowLoading("Conectando...", "Estableciendo conexión segura");
        }

        public void ShowUploadingLoader()
        {
            ShowLoading("Subiendo archivos...", "Transfiriendo datos al servidor");
        }

        public void ShowDownloadingLoader()
        {
            ShowLoading("Descargando...", "Obteniendo archivos del servidor");
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();
            
            // Clean up when view is removed
            if (Parent == null)
            {
                _isAnimating = false;
                _animationCancellation?.Cancel();
                _animationCancellation?.Dispose();
            }
        }
    }
}

using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Threading.Tasks;

namespace APP.Eds.UsesCases.PowerBI
{
    public class PowerBIView : ContentPage
    {
        private const string POWERBI_URL = "https://app.powerbi.com/view?r=eyJrIjoiYzkzOTY0ZTctMDY3NS00MjEzLTk0MDAtMDdkNjk3NGEwNWNhIiwidCI6ImY5MGY3OWRkLTczMDgtNDc4ZS05YTY4LTNjZDAwODljOGM4ZiIsImMiOjR9&embedImagePlaceholder=true";
        private bool _isLoading = true;
        private Grid _loadingOverlay;
        private ScrollView _mainContent;
        private WebView _powerBIWebView;

        public PowerBIView()
        {
            Title = "Power BI Dashboard";
            BuildUI();
            LoadPowerBIDashboard();
        }

        private void BuildUI()
        {
            // Create the main grid
            var mainGrid = new Grid();

            // Create loading overlay
            _loadingOverlay = new Grid
            {
                BackgroundColor = Color.FromArgb("#80000000"),
                IsVisible = true,
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Fill
            };

            var loadingBorder = new Border
            {
                BackgroundColor = Colors.White,
                StrokeShape = new RoundRectangle { CornerRadius = 20 },
                Padding = new Thickness(30, 25),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            var loadingStack = new StackLayout { Spacing = 15 };
            loadingStack.Children.Add(new ActivityIndicator
            {
                IsRunning = true,
                Color = Colors.Purple,
                WidthRequest = 40,
                HeightRequest = 40,
                HorizontalOptions = LayoutOptions.Center
            });
            loadingStack.Children.Add(new Label
            {
                Text = "Cargando Dashboard...",
                FontSize = 16,
                TextColor = Colors.Gray,
                HorizontalOptions = LayoutOptions.Center
            });

            loadingBorder.Content = loadingStack;
            _loadingOverlay.Children.Add(loadingBorder);

            // Create main content
            _mainContent = new ScrollView { IsVisible = false };
            var contentStack = new StackLayout { Padding = new Thickness(20), Spacing = 20 };

            // Header section
            var headerBorder = new Border
            {
                BackgroundColor = Colors.White,
                StrokeShape = new RoundRectangle { CornerRadius = 20 },
                StrokeThickness = 0,
                Padding = new Thickness(25, 20)
            };

            var headerStack = new StackLayout { Spacing = 5 };
            headerStack.Children.Add(new Label
            {
                Text = "Power BI Dashboard",
                FontSize = 24,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Purple
            });
            headerStack.Children.Add(new Label
            {
                Text = "Reportes y analisis de datos en tiempo real",
                FontSize = 14,
                TextColor = Colors.Gray
            });
            headerBorder.Content = headerStack;
            contentStack.Children.Add(headerBorder);

            // Dashboard container
            var dashboardBorder = new Border
            {
                BackgroundColor = Colors.White,
                StrokeShape = new RoundRectangle { CornerRadius = 15 },
                StrokeThickness = 1,
                Stroke = Colors.LightGray,
                Padding = new Thickness(0)
            };

            var webViewBorder = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 15 },
                StrokeThickness = 0,
                HeightRequest = 600
            };

            _powerBIWebView = new WebView
            {
                BackgroundColor = Colors.White
            };
            _powerBIWebView.Navigating += OnWebViewNavigating;
            _powerBIWebView.Navigated += OnWebViewNavigated;

            webViewBorder.Content = _powerBIWebView;
            dashboardBorder.Content = webViewBorder;
            contentStack.Children.Add(dashboardBorder);

            // Information section
            var infoBorder = new Border
            {
                BackgroundColor = Colors.White,
                StrokeShape = new RoundRectangle { CornerRadius = 15 },
                StrokeThickness = 1,
                Stroke = Colors.LightGray,
                Padding = new Thickness(20)
            };

            var infoStack = new StackLayout { Spacing = 10 };
            infoStack.Children.Add(new Label
            {
                Text = "Informacion del Dashboard",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Purple
            });
            infoStack.Children.Add(new Label
            {
                Text = "• Este dashboard muestra datos de ejemplo de Power BI",
                FontSize = 14,
                TextColor = Colors.Gray
            });
            infoStack.Children.Add(new Label
            {
                Text = "• Los reportes se actualizan en tiempo real",
                FontSize = 14,
                TextColor = Colors.Gray
            });
            infoStack.Children.Add(new Label
            {
                Text = "• Use los controles interactivos para filtrar la informacion",
                FontSize = 14,
                TextColor = Colors.Gray
            });
            infoStack.Children.Add(new Label
            {
                Text = "• Para reportes personalizados, contacte al administrador",
                FontSize = 14,
                TextColor = Colors.Gray
            });
            infoBorder.Content = infoStack;
            contentStack.Children.Add(infoBorder);

            // Refresh button
            var refreshBorder = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 15 },
                StrokeThickness = 0,
                BackgroundColor = Colors.Purple,
                Margin = new Thickness(0, 10, 0, 20)
            };

            var refreshTap = new TapGestureRecognizer();
            refreshTap.Tapped += OnRefreshTapped;
            refreshBorder.GestureRecognizers.Add(refreshTap);

            var refreshGrid = new Grid { Padding = new Thickness(20, 15) };
            refreshGrid.Children.Add(new Label
            {
                Text = "Actualizar Dashboard",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            });
            refreshBorder.Content = refreshGrid;
            contentStack.Children.Add(refreshBorder);

            _mainContent.Content = contentStack;

            // Add to main grid
            mainGrid.Children.Add(_loadingOverlay);
            mainGrid.Children.Add(_mainContent);

            Content = mainGrid;
        }

        private async void LoadPowerBIDashboard()
        {
            try
            {
                // Show loading overlay
                _loadingOverlay.IsVisible = true;
                _mainContent.IsVisible = false;

                // Create simple HTML content with the iframe
                var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Power BI Dashboard</title>
    <style>
        body {{
            margin: 0;
            padding: 0;
            background-color: #f8f9fa;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
        }}
        .container {{
            width: 100%;
            height: 100vh;
            display: flex;
            flex-direction: column;
            justify-content: center;
            align-items: center;
        }}
        .powerbi-frame {{
            width: 100%;
            height: 100%;
            border: none;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
        }}
        .loading {{
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            height: 100vh;
            color: #666;
        }}
        .spinner {{
            width: 40px;
            height: 40px;
            border: 4px solid #f3f3f3;
            border-top: 4px solid #6A1B9A;
            border-radius: 50%;
            animation: spin 1s linear infinite;
            margin-bottom: 20px;
        }}
        @keyframes spin {{
            0% {{ transform: rotate(0deg); }}
            100% {{ transform: rotate(360deg); }}
        }}
        .error {{
            padding: 20px;
            text-align: center;
            color: #dc3545;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div id='loading' class='loading'>
            <div class='spinner'></div>
            <p>Cargando Dashboard de Power BI...</p>
        </div>
        <iframe 
            id='powerbi-iframe'
            class='powerbi-frame'
            title='Power BI Dashboard'
            src='{POWERBI_URL}'
            frameborder='0'
            allowfullscreen='true'
            style='display:none;'
            onload='hideLoading()'>
        </iframe>
        <div id='error' class='error' style='display:none;'>
            <h3>Error al cargar el dashboard</h3>
            <p>No se pudo cargar el contenido de Power BI.</p>
            <p>Por favor, verifique su conexion a internet e intente nuevamente.</p>
            <button onclick='reloadFrame()' style='padding: 10px 20px; background: #6A1B9A; color: white; border: none; border-radius: 5px; cursor: pointer;'>
                Reintentar
            </button>
        </div>
    </div>
    
    <script>
        let loadTimeout;
        
        function hideLoading() {{
            document.getElementById('loading').style.display = 'none';
            document.getElementById('powerbi-iframe').style.display = 'block';
            clearTimeout(loadTimeout);
        }}
        
        function showError() {{
            document.getElementById('loading').style.display = 'none';
            document.getElementById('powerbi-iframe').style.display = 'none';
            document.getElementById('error').style.display = 'block';
        }}
        
        function reloadFrame() {{
            document.getElementById('error').style.display = 'none';
            document.getElementById('loading').style.display = 'flex';
            document.getElementById('powerbi-iframe').src = document.getElementById('powerbi-iframe').src;
            loadTimeout = setTimeout(showError, 15000);
        }}
        
        loadTimeout = setTimeout(showError, 15000);
        
        document.getElementById('powerbi-iframe').onerror = function() {{
            showError();
        }};
    </script>
</body>
</html>";

                // Set the HTML source
                var htmlSource = new HtmlWebViewSource
                {
                    Html = htmlContent
                };

                _powerBIWebView.Source = htmlSource;

                // Hide loading after a delay (fallback)
                await Task.Delay(3000);
                if (_isLoading)
                {
                    HideLoadingOverlay();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading Power BI dashboard: {ex.Message}");
                await DisplayAlert("Error", 
                    "No se pudo cargar el dashboard de Power BI. Por favor, intente nuevamente.", 
                    "OK");
                HideLoadingOverlay();
            }
        }

        private void OnWebViewNavigating(object sender, WebNavigatingEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"WebView navigating to: {e.Url}");
        }

        private void OnWebViewNavigated(object sender, WebNavigatedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"WebView navigated to: {e.Url}, Result: {e.Result}");
            
            if (e.Result == WebNavigationResult.Success)
            {
                HideLoadingOverlay();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"WebView navigation failed: {e.Result}");
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Error de Conexion", 
                        "No se pudo cargar el dashboard. Verifique su conexion a internet.", 
                        "OK");
                    HideLoadingOverlay();
                });
            }
        }

        private void HideLoadingOverlay()
        {
            _isLoading = false;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _loadingOverlay.IsVisible = false;
                _mainContent.IsVisible = true;
            });
        }

        private async void OnRefreshTapped(object sender, EventArgs e)
        {
            try
            {
                // Show loading
                _loadingOverlay.IsVisible = true;
                _mainContent.IsVisible = false;
                _isLoading = true;

                // Add visual feedback
                if (sender is Border border)
                {
                    await border.ScaleTo(0.95, 100);
                    await border.ScaleTo(1.0, 100);
                }

                // Reload the dashboard
                LoadPowerBIDashboard();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing dashboard: {ex.Message}");
                await DisplayAlert("Error", "No se pudo actualizar el dashboard.", "OK");
                HideLoadingOverlay();
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            System.Diagnostics.Debug.WriteLine("PowerBIView appeared");
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            System.Diagnostics.Debug.WriteLine("PowerBIView disappeared");
        }
    }
}
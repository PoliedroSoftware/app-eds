using APP.Eds.Models.TransferValidation;
using APP.Eds.Services.TransferValidation;
using Microsoft.Maui.Controls.Shapes;

namespace APP.Eds.UsesCases.TransferValidation;

public partial class QRPaymentsListView : ContentPage
{
    private readonly TransferValidationService _service;
    private readonly ActivityIndicator _loadingIndicator;
    private readonly Grid _loadingOverlay;

    public QRPaymentsListView()
    {
        try
        {
            Title = "Listado de Pagos QR";
            BackgroundColor = Color.FromArgb("#F8F9FA");

            _service = new TransferValidationService();
            _service.RefreshCommand = new Command(async () => await RefreshDataAsync());
            BindingContext = _service;

            // Create loading overlay
            _loadingIndicator = new ActivityIndicator
            {
                IsRunning = true,
                Color = Color.FromArgb("#6A1B9A"),
                WidthRequest = 50,
                HeightRequest = 50,
                HorizontalOptions = LayoutOptions.Center
            };

            var loadingLabel = new Label
            {
                Text = "Cargando pagos QR...",
                FontSize = 16,
                TextColor = Color.FromArgb("#636E72"),
                HorizontalOptions = LayoutOptions.Center
            };

            var loadingStack = new StackLayout
            {
                Spacing = 15,
                Children = { _loadingIndicator, loadingLabel }
            };

            var loadingBorder = new Border
            {
                BackgroundColor = Colors.White,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(20) },
                Padding = new Thickness(40, 30),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Content = loadingStack
            };

            _loadingOverlay = new Grid
            {
                BackgroundColor = Color.FromArgb("#80000000"),
                IsVisible = false,
                Children = { loadingBorder }
            };
            _loadingOverlay.SetBinding(Grid.IsVisibleProperty, nameof(_service.IsLoading));

            // Create main content
            var mainContent = CreateMainContent();

            // Set up the page layout
            Content = new Grid
            {
                Children = { mainContent, _loadingOverlay }
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing QRPaymentsListView: {ex.Message}");
            DisplayAlert("Error", $"Error al inicializar la vista: {ex.Message}", "OK");
        }
    }

    private View CreateMainContent()
    {
        var scrollView = new ScrollView();
        var mainStack = new StackLayout
        {
            Padding = new Thickness(20),
            Spacing = 20
        };

        // Header Card
        mainStack.Children.Add(CreateHeaderCard());

        // Statistics Grid
        mainStack.Children.Add(CreateStatisticsGrid());

        // Total Amount Card
        mainStack.Children.Add(CreateTotalAmountCard());

        // Search and Filter Section
        mainStack.Children.Add(CreateSearchFilterSection());

        // Payments List
        mainStack.Children.Add(CreatePaymentsList());

        // Refresh Button
        mainStack.Children.Add(CreateRefreshButton());

        scrollView.Content = mainStack;
        return scrollView;
    }

    private Border CreateHeaderCard()
    {
        var titleLabel = new Label
        {
            Text = "Pagos con Código QR",
            FontSize = 24,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#6A1B9A"),
            HorizontalOptions = LayoutOptions.Center
        };

        var subtitleLabel = new Label
        {
            Text = "Validaciones de transferencias realizadas",
            FontSize = 14,
            TextColor = Color.FromArgb("#636E72"),
            HorizontalOptions = LayoutOptions.Center
        };

        var headerStack = new StackLayout
        {
            Spacing = 10,
            Children = { titleLabel, subtitleLabel }
        };

        return new Border
        {
            BackgroundColor = Colors.White,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(20) },
            StrokeThickness = 0,
            Padding = new Thickness(20),
            Content = headerStack
        };
    }

    private Grid CreateStatisticsGrid()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
         {
        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
         new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
     },
            ColumnSpacing = 10
        };

        Grid.SetColumn(CreateStatCard("Total", nameof(_service.TotalTransactions), "#6A1B9A"), 0);
        grid.Children.Add(CreateStatCard("Total", nameof(_service.TotalTransactions), "#6A1B9A"));

        var confirmedCard = CreateStatCard("Confirmadas", nameof(_service.ConfirmedTransactions), "#4CAF50");
        Grid.SetColumn(confirmedCard, 1);
        grid.Children.Add(confirmedCard);

        var pendingCard = CreateStatCard("Pendientes", nameof(_service.PendingTransactions), "#FF9800");
        Grid.SetColumn(pendingCard, 2);
        grid.Children.Add(pendingCard);

        return grid;
    }

    private Border CreateStatCard(string title, string bindingPath, string color)
    {
        var titleLabel = new Label
        {
            Text = title,
            FontSize = 11,
            TextColor = Color.FromArgb("#636E72"),
            HorizontalOptions = LayoutOptions.Center
        };

        var valueLabel = new Label
        {
            FontSize = 22,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(color),
            HorizontalOptions = LayoutOptions.Center
        };
        valueLabel.SetBinding(Label.TextProperty, bindingPath);

        var stack = new StackLayout
        {
            Spacing = 5,
            Children = { titleLabel, valueLabel }
        };

        return new Border
        {
            BackgroundColor = Colors.White,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(15) },
            Padding = new Thickness(15, 12),
            Content = stack
        };
    }

    private Border CreateTotalAmountCard()
    {
        var titleLabel = new Label
        {
            Text = "Monto Total Procesado",
            FontSize = 13,
            TextColor = Color.FromArgb("#636E72")
        };

        var amountLabel = new Label
        {
            FontSize = 24,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#6A1B9A")
        };
        amountLabel.SetBinding(Label.TextProperty, nameof(_service.FormattedTotalAmount));

        var stack = new StackLayout
        {
            Spacing = 3,
            VerticalOptions = LayoutOptions.Center,
            Children = { titleLabel, amountLabel }
        };

        return new Border
        {
            BackgroundColor = Colors.White,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(15) },
            Padding = new Thickness(20, 15),
            Content = stack
        };
    }

    private Border CreateSearchFilterSection()
    {
        var searchEntry = new Entry
        {
            Placeholder = "Buscar por cliente, ID o confirmador...",
            BackgroundColor = Color.FromArgb("#F8F9FA"),
            TextColor = Color.FromArgb("#2D3436"),
            FontSize = 14
        };
        searchEntry.SetBinding(Entry.TextProperty, nameof(_service.SearchText));

        var filterLabel = new Label
        {
            Text = "Filtrar:",
            FontSize = 14,
            TextColor = Color.FromArgb("#636E72"),
            VerticalOptions = LayoutOptions.Center
        };

        var filterPicker = new Picker
        {
            BackgroundColor = Color.FromArgb("#F8F9FA"),
            TextColor = Color.FromArgb("#2D3436"),
            FontSize = 14
        };
        filterPicker.SetBinding(Picker.ItemsSourceProperty, nameof(_service.StatusFilterOptions));
        filterPicker.SetBinding(Picker.SelectedItemProperty, nameof(_service.SelectedStatusFilter));

        var filterGrid = new Grid
        {
            ColumnDefinitions =
        {
       new ColumnDefinition { Width = GridLength.Auto },
    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
      },
            ColumnSpacing = 10
        };
        Grid.SetColumn(filterLabel, 0);
        filterGrid.Children.Add(filterLabel);
        Grid.SetColumn(filterPicker, 1);
        filterGrid.Children.Add(filterPicker);

        var stack = new StackLayout
        {
            Spacing = 10,
            Children = { searchEntry, filterGrid }
        };

        return new Border
        {
            BackgroundColor = Colors.White,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(15) },
            Padding = new Thickness(15),
            StrokeThickness = 1,
            Stroke = Color.FromArgb("#E5E7EB"),
            Content = stack
        };
    }

    private Border CreatePaymentsList()
    {
        var titleLabel = new Label
        {
            Text = "Transacciones",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#2D3436"),
            Padding = new Thickness(15, 15, 15, 10)
        };

        var collectionView = new CollectionView
        {
            SelectionMode = SelectionMode.None,
            EmptyView = "No hay transacciones para mostrar",
            ItemTemplate = new DataTemplate(() => CreatePaymentItemTemplate())
        };
        collectionView.SetBinding(ItemsView.ItemsSourceProperty, nameof(_service.FilteredTransferValidations));

        var stack = new StackLayout
        {
            Spacing = 0,
            Children = { titleLabel, collectionView }
        };

        return new Border
        {
            BackgroundColor = Colors.White,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(15) },
            Padding = 0,
            StrokeThickness = 1,
            Stroke = Color.FromArgb("#E5E7EB"),
            Content = stack
        };
    }

    private View CreatePaymentItemTemplate()
    {
        var customerLabel = new Label
        {
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#2D3436"),
            LineBreakMode = LineBreakMode.TailTruncation
        };
        customerLabel.SetBinding(Label.TextProperty, nameof(TransferValidationModel.CustomerName));

        var statusLabel = new Label
        {
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        };
        statusLabel.SetBinding(Label.TextProperty, nameof(TransferValidationModel.Status));

        var statusBorder = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
            Padding = new Thickness(8, 4),
            StrokeThickness = 0,
            Content = statusLabel
        };
        statusBorder.SetBinding(Border.BackgroundColorProperty, nameof(TransferValidationModel.StatusColor));

        var headerGrid = new Grid
        {
            ColumnDefinitions =
       {
      new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
     new ColumnDefinition { Width = GridLength.Auto }
       },
            ColumnSpacing = 10
        };
        Grid.SetColumn(customerLabel, 0);
        headerGrid.Children.Add(customerLabel);
        Grid.SetColumn(statusBorder, 1);
        headerGrid.Children.Add(statusBorder);

        var idValueLabel = new Label { FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#6A1B9A") };
        idValueLabel.SetBinding(Label.TextProperty, nameof(TransferValidationModel.UniqueId));

        var amountValueLabel = new Label { FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#2D3436") };
        amountValueLabel.SetBinding(Label.TextProperty, nameof(TransferValidationModel.FormattedAmount));

        var detailsGrid = new Grid
        {
            ColumnDefinitions =
    {
   new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
       new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
  },
            ColumnSpacing = 10
        };

        var idStack = new StackLayout
        {
            Spacing = 3,
            Children =
   {
       new Label { Text = "ID Transacción:", FontSize = 11, TextColor = Color.FromArgb("#636E72") },
  idValueLabel
  }
        };
        var amountStack = new StackLayout
        {
            Spacing = 3,
            Children =
   {
   new Label { Text = "Monto:", FontSize = 11, TextColor = Color.FromArgb("#636E72") },
       amountValueLabel
    }
        };
        Grid.SetColumn(idStack, 0);
        detailsGrid.Children.Add(idStack);
        Grid.SetColumn(amountStack, 1);
        detailsGrid.Children.Add(amountStack);

        var dateTimeLabel = new Label { FontSize = 12, TextColor = Color.FromArgb("#636E72") };
        dateTimeLabel.SetBinding(Label.TextProperty, nameof(TransferValidationModel.FormattedDateTime));

        var confirmedLabel = new Label { FontSize = 12, TextColor = Color.FromArgb("#636E72") };
        confirmedLabel.SetBinding(Label.TextProperty, new Binding
        {
            Path = nameof(TransferValidationModel.ConfirmedBy),
            StringFormat = "Confirmado por: {0}"
        });

        var confirmedStack = new StackLayout { Children = { confirmedLabel } };
        confirmedStack.SetBinding(IsVisibleProperty, new Binding
        {
            Path = nameof(TransferValidationModel.ConfirmedBy),
            Converter = Application.Current.Resources["StringNotEmptyConverter"] as IValueConverter
        });

        var mainGrid = new Grid
        {
            RowDefinitions =
      {
    new RowDefinition { Height = GridLength.Auto },
      new RowDefinition { Height = GridLength.Auto },
      new RowDefinition { Height = GridLength.Auto },
     new RowDefinition { Height = GridLength.Auto }
   },
            RowSpacing = 8
        };
        Grid.SetRow(headerGrid, 0);
        mainGrid.Children.Add(headerGrid);
        Grid.SetRow(detailsGrid, 1);
        mainGrid.Children.Add(detailsGrid);
        Grid.SetRow(dateTimeLabel, 2);
        mainGrid.Children.Add(dateTimeLabel);
        Grid.SetRow(confirmedStack, 3);
        mainGrid.Children.Add(confirmedStack);

        return new Border
        {
            Margin = new Thickness(10, 5),
            BackgroundColor = Color.FromArgb("#FEFEFE"),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Padding = new Thickness(15),
            StrokeThickness = 1,
            Stroke = Color.FromArgb("#E5E7EB"),
            Content = mainGrid
        };
    }

    private Button CreateRefreshButton()
    {
        var button = new Button
        {
            Text = "Actualizar Datos",
            BackgroundColor = Color.FromArgb("#6A1B9A"),
            TextColor = Colors.White,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 15,
            HeightRequest = 50,
            Margin = new Thickness(0, 10, 0, 20)
        };
        button.SetBinding(Button.CommandProperty, nameof(_service.RefreshCommand));
        return button;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await _service.GetTransferValidationsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading data on appearing: {ex.Message}");
            await DisplayAlert("Error", $"Error al cargar los datos: {ex.Message}", "OK");
        }
    }

    private async Task RefreshDataAsync()
    {
        try
        {
            await _service.RefreshDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error refreshing data: {ex.Message}");
            await DisplayAlert("Error", $"Error al actualizar los datos: {ex.Message}", "OK");
        }
    }
}

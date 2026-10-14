using APP.Eds.Components.PopUp;
using APP.Eds.Models.Client;
using APP.Eds.Models.PointOfSale;
using APP.Eds.Services.PointOfSale;
using APP.Eds.Services.WhatsApp;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;
using System.Globalization;

namespace APP.Eds.UsesCases.PointOfSale;

public partial class PointOfSaleView : ContentPage
{
    private PointOfSaleViewModel _viewModel;

    public PointOfSaleView()
    {
        global::APP.Eds.Helpers.PoliedroTheme.Watch(this);
        var pointOfSaleService = Application.Current?.Handler?.MauiContext?.Services?.GetService<IPointOfSaleService>()
    ?? new PointOfSaleService();
        var whatsAppMessageService = Application.Current?.Handler?.MauiContext?.Services?.GetService<IWhatsAppMessageService>()
    ?? new WhatsAppMessageService();
        _viewModel = new PointOfSaleViewModel(pointOfSaleService, whatsAppMessageService);
        BindingContext = _viewModel;

        Title = "Punto de Venta";
        BackgroundColor = global::APP.Eds.Helpers.PoliedroTheme.PageBackground;

        CreateContent();
    }

    private void CreateContent()
    {
        var scrollView = new ScrollView();

        var mainStack = new StackLayout
        {
            Padding = new Thickness(16),
            Spacing = 20
        };

        // Client selector section (NEW)
        var clientSection = CreateClientSelectorSection();
        mainStack.Add(clientSection);

        // Products section (top)
        var productsSection = CreateProductsSection();
        mainStack.Add(productsSection);

        // Cart section (bottom)
        var cartSection = CreateCartSection();
        mainStack.Add(cartSection);

        scrollView.Content = mainStack;
        Content = scrollView;
    }

    private Frame CreateClientSelectorSection()
    {
        var frame = new Frame
        {
            BackgroundColor = global::APP.Eds.Helpers.PoliedroTheme.CardSurface,
            CornerRadius = 16,
            HasShadow = false,
            Padding = new Thickness(20),
            BorderColor = global::APP.Eds.Helpers.PoliedroTheme.SubtleBorder
        };

        var stackLayout = new StackLayout
        {
            Spacing = 12
        };

        // Header
        var headerLabel = new Label
        {
            Text = "Cliente",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = global::APP.Eds.Helpers.PoliedroTheme.TextSecondary,
            HorizontalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, 0, 0, 8)
        };
        stackLayout.Add(headerLabel);

        // Selected client display with search integrated
        var clientDisplayFrame = new Frame
        {
            BackgroundColor = global::APP.Eds.Helpers.PoliedroTheme.PageBackground,
            CornerRadius = 12,
            HasShadow = false,
            Padding = new Thickness(16, 12),
            BorderColor = global::APP.Eds.Helpers.PoliedroTheme.BrandPrimary
        };

        var clientStack = new StackLayout
        {
            Spacing = 8
        };

        // Cliente seleccionado (visible cuando hay cliente)
        var selectedClientLabel = new Label
        {
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = global::APP.Eds.Helpers.PoliedroTheme.TextSecondary,
            LineBreakMode = LineBreakMode.WordWrap
        };
        selectedClientLabel.SetBinding(Label.TextProperty, "ClientButtonText");
        selectedClientLabel.SetBinding(VisualElement.IsVisibleProperty, new Binding("SelectedClient",
          converter: new FuncConverter<ClientLegalModel, bool>(client => client != null && client.Id > 0)));
        clientStack.Add(selectedClientLabel);

        // Separator line (visible cuando hay cliente)
        var separator = new BoxView
        {
            Color = global::APP.Eds.Helpers.PoliedroTheme.SubtleBorder,
            HeightRequest = 1,
            Margin = new Thickness(0, 4, 0, 4)
        };
        separator.SetBinding(VisualElement.IsVisibleProperty, new Binding("SelectedClient",
     converter: new FuncConverter<ClientLegalModel, bool>(client => client != null && client.Id > 0)));
        clientStack.Add(separator);

        // Search section
        var searchLabel = new Label
        {
            Text = "Buscar por Número de Documento:",
            FontSize = 14,
            TextColor = global::APP.Eds.Helpers.PoliedroTheme.TextSecondary,
            FontAttributes = FontAttributes.Bold
        };
        clientStack.Add(searchLabel);

        var searchGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
   {
      new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
         new ColumnDefinition { Width = GridLength.Auto }
  },
            ColumnSpacing = 8
        };

        var searchEntry = new Entry
        {
            Placeholder = "Ej: 123456789",
            FontSize = 16,
            BackgroundColor = global::APP.Eds.Helpers.PoliedroTheme.CardSurface,
            TextColor = global::APP.Eds.Helpers.PoliedroTheme.TextSecondary,
            PlaceholderColor = global::APP.Eds.Helpers.PoliedroTheme.TextSecondary,
            HeightRequest = 45
        };
        searchEntry.SetBinding(Entry.TextProperty, "ClientSearchText");
        searchGrid.SetColumn(searchEntry, 0);
        searchGrid.Add(searchEntry);

        var searchButton = new Button
        {
            Text = "Buscar",
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            BackgroundColor = global::APP.Eds.Helpers.PoliedroTheme.BrandPrimary,
            TextColor = Colors.White,
            CornerRadius = 10,
            WidthRequest = 90,
            HeightRequest = 45
        };
        searchButton.SetBinding(Button.CommandProperty, "SearchClientCommand");
        searchButton.SetBinding(VisualElement.IsEnabledProperty, new Binding("IsSearching",
                  converter: new FuncConverter<bool, bool>(searching => !searching)));
        searchGrid.SetColumn(searchButton, 1);
        searchGrid.Add(searchButton);

        clientStack.Add(searchGrid);

        // Loading indicator
        var activityIndicator = new ActivityIndicator
        {
            Color = global::APP.Eds.Helpers.PoliedroTheme.BrandPrimary,
            IsRunning = false,
            HeightRequest = 25
        };
        activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, "IsSearching");
        activityIndicator.SetBinding(VisualElement.IsVisibleProperty, "IsSearching");
        clientStack.Add(activityIndicator);

        // ⭐ NUEVO: Botón para crear tercero desde RUT
        var createClientButton = new Button
        {
            Text = "Crear Tercero desde RUT",
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            BackgroundColor = global::APP.Eds.Helpers.PoliedroTheme.BrandPrimary,
            TextColor = Colors.White,
            CornerRadius = 10,
            HeightRequest = 45,
            Margin = new Thickness(0, 8, 0, 0)
        };
        createClientButton.Clicked += OnCreateClientFromRutClicked;
        clientStack.Add(createClientButton);

        // Hint label
        var hintLabel = new Label
        {
            Text = "Busca en persona jurídica (empresas) y naturales (personas)",
            FontSize = 12,
            TextColor = global::APP.Eds.Helpers.PoliedroTheme.TextSecondary,
            FontAttributes = FontAttributes.Italic,
            HorizontalOptions = LayoutOptions.Start
        };
        clientStack.Add(hintLabel);

        clientDisplayFrame.Content = clientStack;
        stackLayout.Add(clientDisplayFrame);

        frame.Content = stackLayout;
        return frame;
    }

    // ⭐ ACTUALIZADO: Método para manejar la creación de cliente desde RUT
    private async void OnCreateClientFromRutClicked(object sender, EventArgs e)
    {
        try
        {
            // Show the RUT popup
            var popup = new CreateClientFromRutPopup();
            var result = await this.ShowPopupAsync(popup);

            if (result is ClientLegalModel createdClient && createdClient.Id > 0)
            {
                // Update the selected client in the ViewModel
                if (_viewModel != null)
                {
                    _viewModel.SelectedClient = createdClient;
                    _viewModel.ClientSearchText = createdClient.DocumentNumber;
                }

                await DisplayAlert(
                    "Cliente Seleccionado",
                    $"El cliente '{createdClient.Name}' ha sido creado y seleccionado correctamente.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Error",
                $"Error al procesar la creación del tercero:\n\n{ex.Message}",
                "OK");
        }
    }

    private Frame CreateProductsSection()
    {
        var frame = new Frame
        {
            BackgroundColor = global::APP.Eds.Helpers.PoliedroTheme.CardSurface,
            CornerRadius = 16,
            HasShadow = false,
            Padding = new Thickness(20),
            BorderColor = global::APP.Eds.Helpers.PoliedroTheme.SubtleBorder
        };

        var stackLayout = new StackLayout
        {
            Spacing = 16
        };

        var header = new Label
        {
            Text = "Productos",
            FontSize = 24,
            FontAttributes = FontAttributes.Bold,
            TextColor = global::APP.Eds.Helpers.PoliedroTheme.TextSecondary,
            HorizontalOptions = LayoutOptions.Center
        };
        stackLayout.Add(header);

        var collectionView = new CollectionView
        {
            SelectionMode = SelectionMode.None,
            HeightRequest = 320 // Reducido de 400 a 320
,
            ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical)
            {
                HorizontalItemSpacing = 12,
                VerticalItemSpacing = 12
            }
        };

        collectionView.SetBinding(ItemsView.ItemsSourceProperty, "Products");

        var productTemplate = new DataTemplate(() =>
        {
            var productFrame = new Frame
            {
                BackgroundColor = global::APP.Eds.Helpers.PoliedroTheme.CardSurface,
                CornerRadius = 16,
                HasShadow = false,
                Padding = new Thickness(14),
                MinimumHeightRequest = 104,
                BorderColor = global::APP.Eds.Helpers.PoliedroTheme.SubtleBorder
            };

            var tapGesture = new TapGestureRecognizer();
            tapGesture.SetBinding(TapGestureRecognizer.CommandProperty, new Binding("BindingContext.AddToCartCommand", source: this));
            tapGesture.SetBinding(TapGestureRecognizer.CommandParameterProperty, ".");
            productFrame.GestureRecognizers.Add(tapGesture);

            // Main container grid
            var containerGrid = new Grid
            {
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = GridLength.Auto }, // For cart indicator
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) } // For content
                }
            };

            // Cart indicator badge - positioned at the top
            var cartIndicatorFrame = new Frame
            {
                BackgroundColor = global::APP.Eds.Helpers.PoliedroTheme.BrandText, // Gold background
                CornerRadius = 8, // Smaller circle
                HasShadow = false,
                Padding = new Thickness(0),
                WidthRequest = 16,
                HeightRequest = 16,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, -8, -8, 0) // Negative margin to position outside
            };
            cartIndicatorFrame.SetBinding(VisualElement.IsVisibleProperty, "IsInCart");

            // Content stack
            var contentStackLayout = new StackLayout
            {
                Spacing = 2,
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            var nameLabel = new Label
            {
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = global::APP.Eds.Helpers.PoliedroTheme.TextPrimary,
                HorizontalOptions = LayoutOptions.Center,
                MaxLines = 2,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalTextAlignment = TextAlignment.Center
            };
            nameLabel.SetBinding(Label.TextProperty, "Name");

            var priceLabel = new Label
            {
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = global::APP.Eds.Helpers.PoliedroTheme.TextPrimary,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            };
            priceLabel.SetBinding(Label.TextProperty, new Binding("SellPrice", stringFormat: "${0:F2}"));

            var stockLabel = new Label
            {
                FontSize = 12,
                TextColor = global::APP.Eds.Helpers.PoliedroTheme.TextPrimary,
                HorizontalOptions = LayoutOptions.Center,
                Opacity = 0.9,
                HorizontalTextAlignment = TextAlignment.Center
            };
            stockLabel.SetBinding(Label.TextProperty, new Binding("Stock", stringFormat: "Stock: {0}"));

            contentStackLayout.Add(nameLabel);
            contentStackLayout.Add(priceLabel);
            contentStackLayout.Add(stockLabel);

            // Add content to row 1
            containerGrid.SetRow(contentStackLayout, 1);
            containerGrid.Add(contentStackLayout);

            // Add cart indicator to row 0
            containerGrid.SetRow(cartIndicatorFrame, 0);
            containerGrid.Add(cartIndicatorFrame);

            productFrame.Content = containerGrid;

            // Create outer container for border effect
            var outerContainer = new Grid();

            // Add border effect when in cart
            var borderFrame = new Frame
            {
                BorderColor = global::APP.Eds.Helpers.PoliedroTheme.BrandText, // Gold border
                BackgroundColor = Colors.Transparent,
                HasShadow = false,
                CornerRadius = 16,
                Padding = new Thickness(0),
                Margin = new Thickness(-2) // Slightly larger than inner frame
            };
            borderFrame.SetBinding(VisualElement.IsVisibleProperty, "IsInCart");

            outerContainer.Add(borderFrame);
            outerContainer.Add(productFrame);

            return outerContainer;
        });

        collectionView.SizeChanged += (_, _) =>
        {
            if (collectionView.ItemsLayout is GridItemsLayout layout && collectionView.Width > 0)
                layout.Span = Math.Clamp((int)(collectionView.Width / 160), 1, 4);
        };
        collectionView.ItemTemplate = productTemplate;
        stackLayout.Add(collectionView);

        frame.Content = stackLayout;
        return frame;
    }

    private Frame CreateCartSection()
    {
        var frame = new Frame
        {
            BackgroundColor = global::APP.Eds.Helpers.PoliedroTheme.CardSurface,
            CornerRadius = 16,
            HasShadow = false,
            Padding = new Thickness(20),
            BorderColor = global::APP.Eds.Helpers.PoliedroTheme.SubtleBorder
        };

        var stackLayout = new StackLayout
        {
            Spacing = 16
        };

        // Header
        var header = new Label
        {
            Text = "Carrito de Compras",
            FontSize = 22,
            FontAttributes = FontAttributes.Bold,
            TextColor = global::APP.Eds.Helpers.PoliedroTheme.TextSecondary,
            HorizontalOptions = LayoutOptions.Center
        };
        stackLayout.Add(header);

        // Cart items
        var cartCollectionView = new CollectionView
        {
            SelectionMode = SelectionMode.None,
            HeightRequest = 320 // Increased to accommodate the improved layout
        };
        cartCollectionView.SetBinding(ItemsView.ItemsSourceProperty, "CartItems");

        var cartTemplate = new DataTemplate(() =>
        {
            var cartFrame = new Frame
            {
                BackgroundColor = global::APP.Eds.Helpers.PoliedroTheme.PageBackground,
                CornerRadius = 12,
                HasShadow = false,
                Padding = new Thickness(16, 14),
                Margin = new Thickness(0, 0, 0, 12),
                BorderColor = global::APP.Eds.Helpers.PoliedroTheme.SubtleBorder
            };

            // Main container with vertical layout
            var mainStack = new StackLayout
            {
                Spacing = 8
            };

            // Top row: Input and controls
            var topGrid = new global::APP.Eds.Controls.ResponsiveFormGrid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(90, GridUnitType.Absolute) },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                ColumnSpacing = 12
            };

            // Total amount entry (now takes full width in first column)
            var totalAmountEntry = new Entry
            {
                Keyboard = Keyboard.Numeric,
                FontSize = 20,
                BackgroundColor = global::APP.Eds.Helpers.PoliedroTheme.CardSurface,
                TextColor = global::APP.Eds.Helpers.PoliedroTheme.TextPrimary,
                Placeholder = "Monto",
                PlaceholderColor = global::APP.Eds.Helpers.PoliedroTheme.TextSecondary,
                HeightRequest = 50,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                ReturnType = ReturnType.Done,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Start,
                VerticalTextAlignment = TextAlignment.Center,
                ClearButtonVisibility = ClearButtonVisibility.WhileEditing
            };

            // Use TotalAmountText property to avoid formatting issues
            totalAmountEntry.SetBinding(Entry.TextProperty, "TotalAmountText");

            totalAmountEntry.TextChanged += (sender, e) =>
            {
                try
                {
                    if (sender is Entry entry && entry.BindingContext is SaleItemModel item)
                    {
                        // Update the TotalAmountText which will trigger calculation
                        item.TotalAmountText = e.NewTextValue ?? "";

                        // Trigger the update command
                        if (this.BindingContext is PointOfSaleViewModel viewModel)
                        {
                            viewModel.UpdateTotalAmountCommand.Execute(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log error but don't crash
                    System.Diagnostics.Debug.WriteLine($"Error in totalAmountEntry.TextChanged: {ex.Message}");
                }
            };

            // Wrap the entry in a border for better visibility
            var entryBorder = new Border
            {
                BackgroundColor = global::APP.Eds.Helpers.PoliedroTheme.CardSurface,
                StrokeThickness = 2,
                Stroke = global::APP.Eds.Helpers.PoliedroTheme.InputBorder,
                StrokeShape = new RoundRectangle { CornerRadius = 10 },
                Padding = new Thickness(12, 0),
                Content = totalAmountEntry,
                Shadow = new Shadow
                {
                    Brush = Colors.LightGray,
                    Offset = new Point(0, 1),
                    Radius = 2,
                    Opacity = 0.3f
                }
            };

            var amountField = new VerticalStackLayout
            {
                Spacing = 8,
                Children = { new Label { Text = "Monto (COP)", FontSize = 14 }, entryBorder }
            };
            topGrid.SetColumn(amountField, 0);
            topGrid.Add(amountField);

            // Gallons display (calculated)
            var gallonsLabel = new Label
            {
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = global::APP.Eds.Helpers.PoliedroTheme.TextPrimary,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                BackgroundColor = global::APP.Eds.Helpers.PoliedroTheme.BrandSoft,
                Padding = new Thickness(12, 8),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };
            gallonsLabel.SetBinding(Label.TextProperty, "GallonsDisplay");

            // Wrap gallons label in a border for better appearance
            var gallonsBorder = new Border
            {
                BackgroundColor = global::APP.Eds.Helpers.PoliedroTheme.BrandSoft,
                StrokeThickness = 1,
                Stroke = global::APP.Eds.Helpers.PoliedroTheme.Success,
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                Padding = new Thickness(0),
                Content = gallonsLabel
            };

            topGrid.SetColumn(gallonsBorder, 1);
            topGrid.Add(gallonsBorder);

            var removeButton = new Button
            {
                Text = "×",
                FontSize = 24,
                FontAttributes = FontAttributes.Bold,
                WidthRequest = 45,
                HeightRequest = 45,
                BackgroundColor = global::APP.Eds.Helpers.PoliedroTheme.ErrorFill,
                TextColor = Colors.White,
                CornerRadius = 22,
                Padding = new Thickness(0),
                BorderWidth = 0,
                FontFamily = "Arial"
            };
            removeButton.SetBinding(Button.CommandProperty, new Binding("BindingContext.RemoveFromCartCommand", source: this));
            removeButton.SetBinding(Button.CommandParameterProperty, ".");
            topGrid.SetColumn(removeButton, 2);
            topGrid.Add(removeButton);

            // Add top grid to main stack
            mainStack.Add(topGrid);

            // Separator line
            var separator = new BoxView
            {
                Color = global::APP.Eds.Helpers.PoliedroTheme.SubtleBorder,
                HeightRequest = 1,
                HorizontalOptions = LayoutOptions.Fill,
                Margin = new Thickness(0, 4, 0, 4)
            };
            mainStack.Add(separator);

            // Bottom row: Product name with price info
            var productInfoStack = new StackLayout
            {
                Spacing = 4
            };

            var nameLabel = new Label
            {
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = global::APP.Eds.Helpers.PoliedroTheme.TextSecondary,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                LineBreakMode = LineBreakMode.WordWrap,
                MaxLines = 2
            };
            nameLabel.SetBinding(Label.TextProperty, "ProductName");
            productInfoStack.Add(nameLabel);

            // Price per unit info
            var priceLabel = new Label
            {
                FontSize = 12,
                TextColor = global::APP.Eds.Helpers.PoliedroTheme.TextSecondary,
                HorizontalOptions = LayoutOptions.Start
            };
            priceLabel.SetBinding(Label.TextProperty, new Binding("UnitPrice", stringFormat: "Precio: ${0:N0} por galón"));
            productInfoStack.Add(priceLabel);

            // Add product info to main stack
            mainStack.Add(productInfoStack);

            // Money section (Currency and Words)
            var moneyFrame = new Frame
            {
                BackgroundColor = global::APP.Eds.Helpers.PoliedroTheme.WarningSurface,
                CornerRadius = 8,
                HasShadow = false,
                Padding = new Thickness(12, 8),
                Margin = new Thickness(0, 4, 0, 0),
                BorderColor = global::APP.Eds.Helpers.PoliedroTheme.Warning
            };

            var moneyStack = new StackLayout
            {
                Spacing = 4
            };

            // Currency format
            var currencyLabel = new Label
            {
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = global::APP.Eds.Helpers.PoliedroTheme.Warning,
                HorizontalOptions = LayoutOptions.Start
            };
            currencyLabel.SetBinding(Label.TextProperty, "TotalAmountCurrency");
            moneyStack.Add(currencyLabel);

            // Amount in words
            var wordsLabel = new Label
            {
                FontSize = 12,
                TextColor = global::APP.Eds.Helpers.PoliedroTheme.Warning,
                HorizontalOptions = LayoutOptions.Start,
                LineBreakMode = LineBreakMode.WordWrap,
                FontAttributes = FontAttributes.Italic
            };
            wordsLabel.SetBinding(Label.TextProperty, "TotalAmountInWords");
            moneyStack.Add(wordsLabel);

            moneyFrame.Content = moneyStack;
            mainStack.Add(moneyFrame);

            cartFrame.Content = mainStack;
            return cartFrame;
        });

        cartCollectionView.ItemTemplate = cartTemplate;
        stackLayout.Add(cartCollectionView);

        // Totals section
        var totalsFrame = CreateTotalsSection();
        stackLayout.Add(totalsFrame);

        // Action buttons section (includes WhatsApp field)
        var actionButtonsSection = CreateActionButtonsSection();
        stackLayout.Add(actionButtonsSection);

        frame.Content = stackLayout;
        return frame;
    }

    private Frame CreateTotalsSection()
    {
        var frame = new Frame
        {
            BackgroundColor = global::APP.Eds.Helpers.PoliedroTheme.BrandSoft,
            CornerRadius = 16,
            HasShadow = false,
            Padding = new Thickness(20),
            BorderColor = global::APP.Eds.Helpers.PoliedroTheme.SubtleBorder
        };

        var stackLayout = new StackLayout { Spacing = 8 };

        // Subtotal
        var subtotalGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
       {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
          }
        };

        var subtotalLabel = new Label
        {
            Text = "Subtotal:",
            FontSize = 16,
            TextColor = global::APP.Eds.Helpers.PoliedroTheme.TextPrimary
        };
        subtotalGrid.SetColumn(subtotalLabel, 0);
        subtotalGrid.Add(subtotalLabel);

        var subtotalValue = new Label
        {
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = global::APP.Eds.Helpers.PoliedroTheme.TextPrimary
        };
        subtotalValue.SetBinding(Label.TextProperty, new Binding("SubTotal", stringFormat: "${0:N0}"));
        subtotalGrid.SetColumn(subtotalValue, 1);
        subtotalGrid.Add(subtotalValue);

        stackLayout.Add(subtotalGrid);

        // Separator
        var separator = new BoxView
        {
            Color = global::APP.Eds.Helpers.PoliedroTheme.CardSurface,
            HeightRequest = 1,
            Margin = new Thickness(0, 8),
            Opacity = 0.5
        };
        stackLayout.Add(separator);

        // Total
        var totalGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
           new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
        new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        var totalLabel = new Label
        {
            Text = "Total",
            FontSize = 22,
            FontAttributes = FontAttributes.Bold,
            TextColor = global::APP.Eds.Helpers.PoliedroTheme.TextPrimary
        };
        totalGrid.SetColumn(totalLabel, 0);
        totalGrid.Add(totalLabel);

        var totalValue = new Label
        {
            FontSize = 34,
            FontAttributes = FontAttributes.Bold,
            TextColor = global::APP.Eds.Helpers.PoliedroTheme.TextPrimary
        };
        totalValue.SetBinding(Label.TextProperty, new Binding("Total", stringFormat: "${0:N0}"));
        totalGrid.SetColumn(totalValue, 1);
        totalGrid.Add(totalValue);

        stackLayout.Add(totalGrid);

        frame.Content = stackLayout;
        return frame;
    }

    private Frame CreateWhatsAppSection()
    {
        var frame = new Frame
        {
            BackgroundColor = Colors.Transparent, // ✅ Changed from green to transparent
            CornerRadius = 12,
            HasShadow = false,
            Padding = new Thickness(16),
            BorderColor = Colors.Transparent,
            Margin = new Thickness(0, 12, 0, 0)
        };

        var stackLayout = new StackLayout
        {
            Spacing = 8
        };

        var whatsappLabel = new Label
        {
            Text = "WhatsApp para enviar factura:",
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = global::APP.Eds.Helpers.PoliedroTheme.TextSecondary, // ✅ Changed to dark gray (was white)
            HorizontalOptions = LayoutOptions.Start
        };
        stackLayout.Add(whatsappLabel);

        var whatsappEntry = new Entry
        {
            Placeholder = "Ej: 3154286798",
            FontSize = 16,
            BackgroundColor = global::APP.Eds.Helpers.PoliedroTheme.CardSurface,
            TextColor = global::APP.Eds.Helpers.PoliedroTheme.TextSecondary,
            PlaceholderColor = global::APP.Eds.Helpers.PoliedroTheme.TextSecondary,
            HeightRequest = 50,
            Keyboard = Keyboard.Telephone,
            ClearButtonVisibility = ClearButtonVisibility.WhileEditing
        };
        whatsappEntry.SetBinding(Entry.TextProperty, "ClientWhatsAppNumber");
        stackLayout.Add(whatsappEntry);

        frame.Content = stackLayout;
        return frame;
    }

    private StackLayout CreateActionButtonsSection()
    {
        var section = new StackLayout
        {
            Spacing = 0
        };

        // WhatsApp section
        var whatsappSection = CreateWhatsAppSection();
        section.Add(whatsappSection);

        // Buttons grid
        var buttonsGrid = CreateActionButtons();
        section.Add(buttonsGrid);

        return section;
    }

    private Grid CreateActionButtons()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
 {
         new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
},
            Margin = new Thickness(0, 12, 0, 0)
        };

        // ✅ REMOVED: Clear button - eliminado completamente

        var processButton = new Button
        {
            Text = "Facturar", // ✅ Changed from "Procesar Pago" to "Facturar"
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            BackgroundColor = global::APP.Eds.Helpers.PoliedroTheme.BrandPrimary,
            TextColor = Colors.White,
            CornerRadius = 12,
            MinimumHeightRequest = 52
        };
        processButton.SetBinding(Button.CommandProperty, "ProcessPaymentCommand");
        processButton.SetBinding(Button.IsEnabledProperty, "CanCompleteTransaction");
        grid.SetColumn(processButton, 0);
        grid.Add(processButton);

        return grid;
    }

    private Frame CreatePaymentSection()
    {
        // ✅ MÉTODO ELIMINADO - Ya no se usa la sección de métodos de pago
        // Este método puede ser eliminado completamente o dejarlo comentado por si se necesita en el futuro
        return null;
    }
}

// Helper converter for toggle button
public class FuncConverter<TSource, TTarget> : IValueConverter
{
    private readonly Func<TSource, TTarget> _convertFunc;
    private readonly Func<TTarget, TSource> _convertBackFunc;

    public FuncConverter(Func<TSource, TTarget> convertFunc, Func<TTarget, TSource> convertBackFunc = null)
    {
        _convertFunc = convertFunc ?? throw new ArgumentNullException(nameof(convertFunc));
        _convertBackFunc = convertBackFunc;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TSource sourceValue)
        {
            return _convertFunc(sourceValue);
        }
        return default(TTarget);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (_convertBackFunc != null && value is TTarget targetValue)
        {
            return _convertBackFunc(targetValue);
        }
        throw new NotImplementedException();
    }
}

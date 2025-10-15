using APP.Eds.Services.PointOfSale;

namespace APP.Eds.UsesCases.PointOfSale;

public partial class PointOfSaleView : ContentPage
{
    public PointOfSaleView()
    {
        var pointOfSaleService = Application.Current?.Handler?.MauiContext?.Services?.GetService<IPointOfSaleService>() 
            ?? new PointOfSaleService();
        BindingContext = new PointOfSaleViewModel(pointOfSaleService);
        
        Title = "Punto de Venta";
        BackgroundColor = Color.FromArgb("#F8FAFC");
        
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
        
        // Products section (top)
        var productsSection = CreateProductsSection();
        mainStack.Add(productsSection);
        
        // Cart section (bottom)
        var cartSection = CreateCartSection();
        mainStack.Add(cartSection);
        
        // Payment section
        var paymentSection = CreatePaymentSection();
        mainStack.Add(paymentSection);
        
        scrollView.Content = mainStack;
        Content = scrollView;
    }
    
    private Frame CreateProductsSection()
    {
        var frame = new Frame
        {
            BackgroundColor = Colors.White,
            CornerRadius = 16,
            HasShadow = true,
            Padding = new Thickness(20),
            BorderColor = Color.FromArgb("#E2E8F0")
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
            TextColor = Color.FromArgb("#374151"),
            HorizontalOptions = LayoutOptions.Center
        };
        stackLayout.Add(header);
        
        var collectionView = new CollectionView
        {
            SelectionMode = SelectionMode.None,
            HeightRequest = 320, // Reducido de 400 a 320
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
                BackgroundColor = Color.FromArgb("#3B82F6"),
                CornerRadius = 16,
                HasShadow = true,
                Padding = new Thickness(14),
                HeightRequest = 90,
                BorderColor = Color.FromArgb("#2563EB")
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
                BackgroundColor = Color.FromArgb("#FFD700"), // Gold background
                CornerRadius = 8, // Smaller circle
                HasShadow = true,
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
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center,
                MaxLines = 1,
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalTextAlignment = TextAlignment.Center
            };
            nameLabel.SetBinding(Label.TextProperty, "Name");
            
            var priceLabel = new Label
            {
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            };
            priceLabel.SetBinding(Label.TextProperty, new Binding("SellPrice", stringFormat: "${0:F2}"));
            
            var stockLabel = new Label
            {
                FontSize = 10,
                TextColor = Colors.White,
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
                BorderColor = Color.FromArgb("#FFD700"), // Gold border
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
        
        collectionView.ItemTemplate = productTemplate;
        stackLayout.Add(collectionView);
        
        frame.Content = stackLayout;
        return frame;
    }
    
    private Frame CreateCartSection()
    {
        var frame = new Frame
        {
            BackgroundColor = Colors.White,
            CornerRadius = 16,
            HasShadow = true,
            Padding = new Thickness(20),
            BorderColor = Color.FromArgb("#E2E8F0")
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
            TextColor = Color.FromArgb("#374151"),
            HorizontalOptions = LayoutOptions.Center
        };
        stackLayout.Add(header);
        
        // Cart items
        var cartCollectionView = new CollectionView
        {
            SelectionMode = SelectionMode.None,
            HeightRequest = 200
        };
        cartCollectionView.SetBinding(ItemsView.ItemsSourceProperty, "CartItems");
        
        var cartTemplate = new DataTemplate(() =>
        {
            var cartFrame = new Frame
            {
                BackgroundColor = Color.FromArgb("#F1F5F9"),
                CornerRadius = 12,
                HasShadow = false,
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 8),
                BorderColor = Color.FromArgb("#E2E8F0")
            };
            
            var mainGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                ColumnSpacing = 8
            };
            
            var nameLabel = new Label
            {
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#374151"),
                VerticalOptions = LayoutOptions.Center,
                MaxLines = 1,
                LineBreakMode = LineBreakMode.TailTruncation
            };
            nameLabel.SetBinding(Label.TextProperty, "ProductName");
            mainGrid.SetColumn(nameLabel, 0);
            mainGrid.Add(nameLabel);
            
            var decreaseButton = new Button
            {
                Text = "-",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                WidthRequest = 35,
                HeightRequest = 35,
                BackgroundColor = Color.FromArgb("#9CA3AF"),
                TextColor = Colors.White,
                CornerRadius = 8,
                Padding = new Thickness(0)
            };
            decreaseButton.SetBinding(Button.CommandProperty, new Binding("BindingContext.DecreaseQuantityCommand", source: this));
            decreaseButton.SetBinding(Button.CommandParameterProperty, ".");
            mainGrid.SetColumn(decreaseButton, 1);
            mainGrid.Add(decreaseButton);
            
            var quantityLabel = new Label
            {
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#374151"),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                WidthRequest = 30
            };
            quantityLabel.SetBinding(Label.TextProperty, "Quantity");
            mainGrid.SetColumn(quantityLabel, 2);
            mainGrid.Add(quantityLabel);
            
            var increaseButton = new Button
            {
                Text = "+",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                WidthRequest = 35,
                HeightRequest = 35,
                BackgroundColor = Color.FromArgb("#9CA3AF"),
                TextColor = Colors.White,
                CornerRadius = 8,
                Padding = new Thickness(0)
            };
            increaseButton.SetBinding(Button.CommandProperty, new Binding("BindingContext.IncreaseQuantityCommand", source: this));
            increaseButton.SetBinding(Button.CommandParameterProperty, ".");
            mainGrid.SetColumn(increaseButton, 3);
            mainGrid.Add(increaseButton);
            
            var removeButton = new Button
            {
                Text = "X",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                WidthRequest = 30,
                HeightRequest = 30,
                BackgroundColor = Color.FromArgb("#EF4444"),
                TextColor = Colors.White,
                CornerRadius = 15,
                Padding = new Thickness(0)
            };
            removeButton.SetBinding(Button.CommandProperty, new Binding("BindingContext.RemoveFromCartCommand", source: this));
            removeButton.SetBinding(Button.CommandParameterProperty, ".");
            mainGrid.SetColumn(removeButton, 4);
            mainGrid.Add(removeButton);
            
            cartFrame.Content = mainGrid;
            return cartFrame;
        });
        
        cartCollectionView.ItemTemplate = cartTemplate;
        stackLayout.Add(cartCollectionView);
        
        // Totals section
        var totalsFrame = CreateTotalsSection();
        stackLayout.Add(totalsFrame);
        
        // Action buttons
        var buttonsGrid = CreateActionButtons();
        stackLayout.Add(buttonsGrid);
        
        frame.Content = stackLayout;
        return frame;
    }
    
    private Frame CreateTotalsSection()
    {
        var frame = new Frame
        {
            BackgroundColor = Color.FromArgb("#10B981"),
            CornerRadius = 16,
            HasShadow = true,
            Padding = new Thickness(20),
            BorderColor = Color.FromArgb("#059669")
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
            TextColor = Colors.White
        };
        subtotalGrid.SetColumn(subtotalLabel, 0);
        subtotalGrid.Add(subtotalLabel);
        
        var subtotalValue = new Label
        {
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        };
        subtotalValue.SetBinding(Label.TextProperty, new Binding("SubTotal", stringFormat: "${0:N0}"));
        subtotalGrid.SetColumn(subtotalValue, 1);
        subtotalGrid.Add(subtotalValue);
        
        stackLayout.Add(subtotalGrid);
        
        // Separator
        var separator = new BoxView
        {
            Color = Colors.White,
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
            Text = "TOTAL:",
            FontSize = 22,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        };
        totalGrid.SetColumn(totalLabel, 0);
        totalGrid.Add(totalLabel);
        
        var totalValue = new Label
        {
            FontSize = 22,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        };
        totalValue.SetBinding(Label.TextProperty, new Binding("Total", stringFormat: "${0:N0}"));
        totalGrid.SetColumn(totalValue, 1);
        totalGrid.Add(totalValue);
        
        stackLayout.Add(totalGrid);
        
        frame.Content = stackLayout;
        return frame;
    }
    
    private Grid CreateActionButtons()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            },
            ColumnSpacing = 12
        };
        
        var clearButton = new Button
        {
            Text = "Limpiar",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            BackgroundColor = Color.FromArgb("#D97706"),
            TextColor = Colors.White,
            CornerRadius = 12,
            HeightRequest = 50
        };
        clearButton.SetBinding(Button.CommandProperty, "ClearCartCommand");
        grid.SetColumn(clearButton, 0);
        grid.Add(clearButton);
        
        var processButton = new Button
        {
            Text = "Procesar Pago",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            BackgroundColor = Color.FromArgb("#059669"),
            TextColor = Colors.White,
            CornerRadius = 12,
            HeightRequest = 50
        };
        processButton.SetBinding(Button.CommandProperty, "ProcessPaymentCommand");
        processButton.SetBinding(Button.IsEnabledProperty, "CanCompleteTransaction");
        grid.SetColumn(processButton, 1);
        grid.Add(processButton);
        
        return grid;
    }
    
    private Frame CreatePaymentSection()
    {
        var frame = new Frame
        {
            BackgroundColor = Colors.White,
            CornerRadius = 16,
            HasShadow = true,
            Padding = new Thickness(20),
            BorderColor = Color.FromArgb("#E2E8F0")
        };
        
        var stackLayout = new StackLayout { Spacing = 16 };
        
        // Payment method header
        var paymentHeader = new Label
        {
            Text = "Método de Pago",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#374151"),
            HorizontalOptions = LayoutOptions.Center
        };
        stackLayout.Add(paymentHeader);
        
        // Payment buttons
        var paymentGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            },
            ColumnSpacing = 12
        };
        
        var cashButton = new Button
        {
            Text = "Efectivo",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            BackgroundColor = Color.FromArgb("#059669"),
            TextColor = Colors.White,
            CornerRadius = 12,
            HeightRequest = 50
        };
        cashButton.SetBinding(Button.CommandProperty, "SelectPaymentMethodCommand");
        cashButton.CommandParameter = "Cash";
        paymentGrid.SetColumn(cashButton, 0);
        paymentGrid.Add(cashButton);
        
        var cardButton = new Button
        {
            Text = "Tarjeta",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            BackgroundColor = Color.FromArgb("#6B7280"),
            TextColor = Colors.White,
            CornerRadius = 12,
            HeightRequest = 50
        };
        cardButton.SetBinding(Button.CommandProperty, "SelectPaymentMethodCommand");
        cardButton.CommandParameter = "Card";
        paymentGrid.SetColumn(cardButton, 1);
        paymentGrid.Add(cardButton);
        
        stackLayout.Add(paymentGrid);
        
        // Cash section
        var cashGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            },
            ColumnSpacing = 12
        };
        
        var receivedLabel = new Label
        {
            Text = "Recibido:",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#374151"),
            VerticalOptions = LayoutOptions.Center
        };
        cashGrid.SetColumn(receivedLabel, 0);
        cashGrid.Add(receivedLabel);
        
        var cashEntry = new Entry
        {
            Keyboard = Keyboard.Numeric,
            FontSize = 16,
            BackgroundColor = Color.FromArgb("#F1F5F9"),
            TextColor = Color.FromArgb("#374151"),
            Placeholder = "$0",
            PlaceholderColor = Color.FromArgb("#9CA3AF"),
            HeightRequest = 45
        };
        cashEntry.SetBinding(Entry.TextProperty, "CashReceived");
        cashGrid.SetColumn(cashEntry, 1);
        cashGrid.Add(cashEntry);
        
        var changeLabel = new Label
        {
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#059669"),
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.End
        };
        changeLabel.SetBinding(Label.TextProperty, new Binding("Change", stringFormat: "Cambio: ${0:N0}"));
        cashGrid.SetColumn(changeLabel, 2);
        cashGrid.Add(changeLabel);
        
        stackLayout.Add(cashGrid);
        
        frame.Content = stackLayout;
        return frame;
    }
}
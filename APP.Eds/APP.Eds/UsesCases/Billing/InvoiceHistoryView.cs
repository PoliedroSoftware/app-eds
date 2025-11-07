using APP.Eds.Models.Billing;

namespace APP.Eds.UsesCases.Billing;

public partial class InvoiceHistoryView : ContentPage
{
    private InvoiceHistoryViewModel _viewModel;

    public InvoiceHistoryView()
    {
        _viewModel = new InvoiceHistoryViewModel();
        BindingContext = _viewModel;

        CreateContent();
    }

    private void CreateContent()
    {
        Title = "Historial de Facturas";
        var mainLayout = new Grid
        {
            RowDefinitions =
             {
                new RowDefinition { Height = GridLength.Star }
             },
            Padding = 0
        };

        var invoicesCollection = new CollectionView
        {
            SelectionMode = SelectionMode.None,
            ItemTemplate = new DataTemplate(CreateInvoiceTemplate),
            EmptyView = CreateEmptyView()
        };
        invoicesCollection.SetBinding(CollectionView.ItemsSourceProperty, nameof(InvoiceHistoryViewModel.Invoices));

        var scrollView = new ScrollView
        {
            Content = invoicesCollection
        };

        mainLayout.Add(scrollView, 0, 0);

        var loadingOverlay = new Frame
        {
            BackgroundColor = Color.FromArgb("#80000000"),
            IsVisible = false,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            Padding = 0,
            Content = new ActivityIndicator
            {
                IsRunning = true,
                Color = Colors.White,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        };
        loadingOverlay.SetBinding(VisualElement.IsVisibleProperty, nameof(InvoiceHistoryViewModel.IsLoading)); // ✨ CORREGIDO

        mainLayout.Add(loadingOverlay, 0, 0);

        Content = mainLayout;
    }

    private View CreateInvoiceTemplate()
    {
        return new Frame
        {
            Margin = new Thickness(16, 8),
            Padding = 16,
            BackgroundColor = Colors.White,
            CornerRadius = 12,
            HasShadow = true,
            Content = new Grid
            {
                RowDefinitions =
       {
          new RowDefinition { Height = GridLength.Auto }, // Header
          new RowDefinition { Height = GridLength.Auto }, // Client
          new RowDefinition { Height = GridLength.Auto }, // Islander & EDS (NEW)
          new RowDefinition { Height = GridLength.Auto }, // Totals
          new RowDefinition { Height = GridLength.Auto }  // Actions
  },
                ColumnDefinitions =
      {
     new ColumnDefinition { Width = GridLength.Star },
        new ColumnDefinition { Width = GridLength.Auto }
       },
                RowSpacing = 12,
                Children =
          {
     // Fila 1: Número de factura y estado
     CreateHeaderRow(),
  
     // Fila 2: Cliente
     CreateClientRow(),
        
     // Fila 3: Islandero y EDS (NEW)
     CreateIslanderEdsRow(),

     // Fila 4: Totales
     CreateTotalsRow(),
      
      // Fila 5: Botones de acción
     CreateActionsRow()
      }
            }
        };
    }

    private Grid CreateHeaderRow()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
       {
 new ColumnDefinition { Width = GridLength.Star },
       new ColumnDefinition { Width = GridLength.Auto }
       }
        };

        var invoiceLabel = new Label
        {
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#6200E8"),
            VerticalOptions = LayoutOptions.Center
        };
        invoiceLabel.SetBinding(Label.TextProperty, nameof(ElectronicInvoiceModel.FullInvoiceNumber));

        var statusStack = new HorizontalStackLayout
        {
            Spacing = 4,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center
        };

        var statusIcon = new Label
        {
            FontSize = 16,
            VerticalOptions = LayoutOptions.Center
        };
        statusIcon.SetBinding(Label.TextProperty, nameof(ElectronicInvoiceModel.StatusIcon));

        var statusLabel = new Label
        {
            FontSize = 12,
            TextColor = Color.FromArgb("#4CAF50"),
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center
        };
        statusLabel.SetBinding(Label.TextProperty, nameof(ElectronicInvoiceModel.Status));

        statusStack.Add(statusIcon);
        statusStack.Add(statusLabel);

        grid.Add(invoiceLabel, 0, 0);
        grid.Add(statusStack, 1, 0);
        Grid.SetRow(grid, 0);
        Grid.SetColumnSpan(grid, 2);

        return grid;
    }

    private Grid CreateClientRow()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
     {
         new ColumnDefinition { Width = GridLength.Auto },
        new ColumnDefinition { Width = GridLength.Star }
       }
        };

        var icon = new Label
        {
            Text = "👤",
            FontSize = 16,
            Margin = new Thickness(0, 0, 8, 0),
            VerticalOptions = LayoutOptions.Center
        };

        var clientStack = new VerticalStackLayout
        {
            Spacing = 2
        };

        // Cliente label
        var clientLabel = new Label
        {
            Text = "Tercero:",
            FontSize = 10,
            TextColor = Color.FromArgb("#999999"),
            FontAttributes = FontAttributes.Bold
        };
        clientStack.Add(clientLabel);

        var clientName = new Label
        {
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#333333")
        };
        clientName.SetBinding(Label.TextProperty, nameof(ElectronicInvoiceModel.ClientName));

        var clientDoc = new Label
        {
            FontSize = 12,
            TextColor = Color.FromArgb("#666666")
        };
        clientDoc.SetBinding(Label.TextProperty, nameof(ElectronicInvoiceModel.ClientDocumentNumber));

        var dateLabel = new Label
        {
            FontSize = 11,
            TextColor = Color.FromArgb("#999999")
        };
        dateLabel.SetBinding(Label.TextProperty, nameof(ElectronicInvoiceModel.DateFormatted));

        clientStack.Add(clientName);
        clientStack.Add(clientDoc);
        clientStack.Add(dateLabel);

        grid.Add(icon, 0, 0);
        grid.Add(clientStack, 1, 0);
        Grid.SetRow(grid, 1);
        Grid.SetColumnSpan(grid, 2);

        return grid;
    }

    // ✨ NEW: Islander and EDS row
    private Grid CreateIslanderEdsRow()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            },
            ColumnSpacing = 8
        };

        // Islander info (left side)
        var islanderStack = new VerticalStackLayout
        {
            Spacing = 2
        };

        var islanderTitleStack = new HorizontalStackLayout
        {
            Spacing = 4
        };

        var islanderIcon = new Label
        {
            Text = "👤",
            FontSize = 12,
            VerticalOptions = LayoutOptions.Center
        };

        var islanderTitle = new Label
        {
            Text = "Islandero:",
            FontSize = 9,
            TextColor = Color.FromArgb("#999999"),
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center
        };

        islanderTitleStack.Add(islanderIcon);
        islanderTitleStack.Add(islanderTitle);

        var islanderLabel = new Label
        {
            FontSize = 11,
            TextColor = Color.FromArgb("#6B7280"),
            LineBreakMode = LineBreakMode.TailTruncation,
            MaxLines = 1
        };
        islanderLabel.SetBinding(Label.TextProperty, nameof(ElectronicInvoiceModel.IslanderName));

        islanderStack.Add(islanderTitleStack);
        islanderStack.Add(islanderLabel);

        // EDS info (right side)
        var edsStack = new VerticalStackLayout
        {
            Spacing = 2,
            HorizontalOptions = LayoutOptions.End
        };

        var edsTitleStack = new HorizontalStackLayout
        {
            Spacing = 4,
            HorizontalOptions = LayoutOptions.End
        };

        var edsIcon = new Label
        {
            Text = "🏪",
            FontSize = 12,
            VerticalOptions = LayoutOptions.Center
        };

        var edsTitle = new Label
        {
            Text = "EDS:",
            FontSize = 9,
            TextColor = Color.FromArgb("#999999"),
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center
        };

        edsTitleStack.Add(edsIcon);
        edsTitleStack.Add(edsTitle);

        var edsLabel = new Label
        {
            FontSize = 11,
            TextColor = Color.FromArgb("#6B7280"),
            LineBreakMode = LineBreakMode.TailTruncation,
            MaxLines = 1,
            HorizontalTextAlignment = TextAlignment.End
        };
        edsLabel.SetBinding(Label.TextProperty, nameof(ElectronicInvoiceModel.EdsName));

        edsStack.Add(edsTitleStack);
        edsStack.Add(edsLabel);

        grid.Add(islanderStack, 0, 0);
        grid.Add(edsStack, 1, 0);

        Grid.SetRow(grid, 2); // Row 2 (after client row)
        Grid.SetColumnSpan(grid, 2);

        return grid;
    }

    private Grid CreateTotalsRow()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
         {
   new ColumnDefinition { Width = GridLength.Star },
     new ColumnDefinition { Width = GridLength.Auto }
 }
        };

        var paymentContainer = new VerticalStackLayout
        {
            Spacing = 2
        };

        var paymentTitleLabel = new Label
        {
            Text = "Método de Pago:",
            FontSize = 9,
            TextColor = Color.FromArgb("#999999"),
            FontAttributes = FontAttributes.Bold
        };

        var paymentStack = new HorizontalStackLayout
        {
            Spacing = 4
        };

        var paymentIcon = new Label
        {
            FontSize = 14
        };
        paymentIcon.SetBinding(Label.TextProperty, nameof(ElectronicInvoiceModel.PaymentMethodIcon));

        var paymentLabel = new Label
        {
            FontSize = 12,
            TextColor = Color.FromArgb("#666666")
        };
        paymentLabel.SetBinding(Label.TextProperty, nameof(ElectronicInvoiceModel.PaymentMethod));

        paymentStack.Add(paymentIcon);
        paymentStack.Add(paymentLabel);

        paymentContainer.Add(paymentTitleLabel);
        paymentContainer.Add(paymentStack);

        var totalContainer = new VerticalStackLayout
        {
            Spacing = 2,
            HorizontalOptions = LayoutOptions.End
        };

        var totalTitleLabel = new Label
        {
            Text = "Total:",
            FontSize = 9,
            TextColor = Color.FromArgb("#999999"),
            FontAttributes = FontAttributes.Bold,
            HorizontalTextAlignment = TextAlignment.End
        };

        var totalLabel = new Label
        {
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#6200E8"),
            HorizontalOptions = LayoutOptions.End
        };
        totalLabel.SetBinding(Label.TextProperty, nameof(ElectronicInvoiceModel.TotalAmountFormatted));

        totalContainer.Add(totalTitleLabel);
        totalContainer.Add(totalLabel);

        grid.Add(paymentContainer, 0, 0);
        grid.Add(totalContainer, 1, 0);
        Grid.SetRow(grid, 3);
        Grid.SetColumnSpan(grid, 2);

        return grid;
    }

    private Grid CreateActionsRow()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
 {
         new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
         new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
         new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
},
            ColumnSpacing = 6
        };

        // ✅ View PDF Button
        var viewButton = CreateActionButton(
            icon: "📄",
            text: "Ver PDF",
            backgroundColor: "#6200E8",
            commandBinding: nameof(InvoiceHistoryViewModel.ViewPdfCommand)
        );
        grid.SetColumn(viewButton, 0);
        grid.Add(viewButton);

        // ✅ Share Button
        var shareButton = CreateActionButton(
            icon: "📤",
            text: "Compartir",
            backgroundColor: "#4CAF50",
            commandBinding: nameof(InvoiceHistoryViewModel.SharePdfCommand)
        );
        grid.SetColumn(shareButton, 1);
        grid.Add(shareButton);

        // ✨ Credit Note Button
        var creditNoteButton = CreateActionButton(
            icon: "📝",
            text: "Nota Crédito",
            backgroundColor: "#FF9800",
            commandBinding: nameof(InvoiceHistoryViewModel.CreditNoteCommand),
            fontSize: 9
        );

        // Trigger para deshabilitar si está anulada
        var trigger = new DataTrigger(typeof(Frame))
        {
            Binding = new Binding("Status"),
            Value = "Anulada"
        };
        trigger.Setters.Add(new Setter
        {
            Property = VisualElement.OpacityProperty,
            Value = 0.5
        });
        trigger.Setters.Add(new Setter
        {
            Property = VisualElement.IsEnabledProperty,
            Value = false
        });
        creditNoteButton.Triggers.Add(trigger);

        grid.SetColumn(creditNoteButton, 2);
        grid.Add(creditNoteButton);

        Grid.SetRow(grid, 4);
        Grid.SetColumnSpan(grid, 2);

        return grid;
    }

    private Frame CreateActionButton(
        string icon,
        string text,
        string backgroundColor,
        string commandBinding,
        int fontSize = 11)
    {
        var container = new Frame
        {
            BackgroundColor = Color.FromArgb(backgroundColor),
            Padding = new Thickness(4, 8),
            HeightRequest = 62,
            CornerRadius = 8,
            HasShadow = false,
            BorderColor = Colors.Transparent
        };

        var stackLayout = new VerticalStackLayout
        {
            Spacing = 4,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        // Icono
        var iconLabel = new Label
        {
            Text = icon,
            FontSize = 20,
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center
        };
        stackLayout.Add(iconLabel);

        // Texto
        var textLabel = new Label
        {
            Text = text,
            FontSize = fontSize,
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            LineBreakMode = LineBreakMode.WordWrap,
            MaxLines = 2,
            Margin = new Thickness(2, 0)
        };
        stackLayout.Add(textLabel);

        container.Content = stackLayout;

        // Agregar TapGestureRecognizer
        var tapGesture = new TapGestureRecognizer();
        tapGesture.SetBinding(TapGestureRecognizer.CommandProperty,
            new Binding(commandBinding, source: _viewModel));
        tapGesture.SetBinding(TapGestureRecognizer.CommandParameterProperty, ".");
        container.GestureRecognizers.Add(tapGesture);

        return container;
    }

    private View CreateEmptyView()
    {
        return new VerticalStackLayout
        {
            Spacing = 16,
            Padding = 32,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
      new Label
 {
    Text = "📄",
       FontSize = 64,
     HorizontalOptions = LayoutOptions.Center
 },
      new Label
 {
       Text = "No hay facturas electrónicas",
    FontSize = 18,
     FontAttributes = FontAttributes.Bold,
 TextColor = Color.FromArgb("#666666"),
   HorizontalOptions = LayoutOptions.Center
 },
   new Label
 {
   Text = "Las facturas que generes aparecerán aquí",
  FontSize = 14,
       TextColor = Color.FromArgb("#999999"),
      HorizontalOptions = LayoutOptions.Center,
      HorizontalTextAlignment = TextAlignment.Center
       }
      }
        };
    }
}

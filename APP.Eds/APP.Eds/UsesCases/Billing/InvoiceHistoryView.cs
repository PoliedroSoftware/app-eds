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
          new RowDefinition { Height = GridLength.Auto },
          new RowDefinition { Height = GridLength.Auto },
          new RowDefinition { Height = GridLength.Auto },
          new RowDefinition { Height = GridLength.Auto }
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
        
     // Fila 3: Totales
 CreateTotalsRow(),
      
          // Fila 4: Botones de acción
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

    private Grid CreateTotalsRow()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
         {
   new ColumnDefinition { Width = GridLength.Star },
     new ColumnDefinition { Width = GridLength.Star }
 }
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

        var totalLabel = new Label
        {
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#6200E8"),
            HorizontalOptions = LayoutOptions.End
        };
        totalLabel.SetBinding(Label.TextProperty, nameof(ElectronicInvoiceModel.TotalAmountFormatted));

        grid.Add(paymentStack, 0, 0);
        grid.Add(totalLabel, 1, 0);
        Grid.SetRow(grid, 2);
        Grid.SetColumnSpan(grid, 2);

        return grid;
    }

    private Grid CreateActionsRow()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
 {
        new ColumnDefinition { Width = GridLength.Star },
 new ColumnDefinition { Width = GridLength.Star }
  },
            ColumnSpacing = 8
        };

        var viewButton = new Button
        {
            Text = "📄 Ver PDF",
            BackgroundColor = Color.FromArgb("#6200E8"),
            TextColor = Colors.White,
            CornerRadius = 8,
            FontSize = 12,
            Padding = new Thickness(12, 8)
        };
        viewButton.SetBinding(Button.CommandProperty, new Binding(nameof(InvoiceHistoryViewModel.ViewPdfCommand), source: _viewModel));
        viewButton.SetBinding(Button.CommandParameterProperty, ".");

        var shareButton = new Button
        {
            Text = "📤 Compartir",
            BackgroundColor = Color.FromArgb("#4CAF50"),
            TextColor = Colors.White,
            CornerRadius = 8,
            FontSize = 12,
            Padding = new Thickness(12, 8)
        };
        shareButton.SetBinding(Button.CommandProperty, new Binding(nameof(InvoiceHistoryViewModel.SharePdfCommand), source: _viewModel));
        shareButton.SetBinding(Button.CommandParameterProperty, ".");

        grid.Add(viewButton, 0, 0);
        grid.Add(shareButton, 1, 0);
        Grid.SetRow(grid, 3);
        Grid.SetColumnSpan(grid, 2);

        return grid;
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

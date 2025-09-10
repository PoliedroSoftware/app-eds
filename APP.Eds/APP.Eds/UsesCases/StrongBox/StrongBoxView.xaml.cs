using APP.Eds.Services.StrongBox;
using Microsoft.Maui.Controls;

namespace APP.Eds.UsesCases.StrongBox;

public partial class StrongBoxView : ContentPage
{
    public StrongBoxView()
    {
        InitializeComponent();
        BindingContext = new StrongBoxService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as StrongBoxService)?.LoadDataCommand.Execute(null);
    }
}
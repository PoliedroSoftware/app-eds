using APP.Eds.Services.Court;

namespace APP.Eds.UsesCases;

public partial class CourtView : ContentPage
{
	public CourtView()
	{
		InitializeComponent();
		BindingContext = new CourtService();
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
        var courtId = CourtIdEntry.Text;

        if (BindingContext is CourtService viewModel)
        {
            await viewModel.LoadCourtDataAsync(int.Parse(courtId));

        }
    }
}
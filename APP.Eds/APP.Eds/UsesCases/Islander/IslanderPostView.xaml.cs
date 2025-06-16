using APP.Eds.Services.Islander;

namespace APP.Eds.UsesCases.Islander;

public partial class IslanderPostView : ContentPage
{
    private IslanderService _islanderService;
    public IslanderPostView()
	{
        InitializeComponent();
        _islanderService = new IslanderService();
        BindingContext = _islanderService;
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        if (BindingContext is IslanderService vm && vm.SelectedEds is not null)
        {
            try
            {
                LoadingOverlay.ShowLoading();
                var selectedId = vm.SelectedEds.IdEds;
                await vm.SaveIslanderDataAsync();
            }
            finally
            {
                LoadingOverlay.HideLoading();

                Name = string.Empty;
                Email = string.Empty;
                FirstName = string.Empty;
                LastName = string.Empty;
                _islanderService.SelectedEds = null;
                Password = string.Empty;

            }
        }
        else
        {
            await DisplayAlert("Error", "Por favor, seleccione un EDS", "OK");
        }
    }

    public string Name
    {
        get => _islanderService.Name;
        set
        {
            _islanderService.Name = value;
            OnPropertyChanged();
        }
    }
    public string Email
    {
        get => _islanderService.Email;
        set
        {
            _islanderService.Email = value;
            OnPropertyChanged();
        }
    }
    public string FirstName
    {
        get => _islanderService.FirstName;
        set
        {
            _islanderService.FirstName = value;
            OnPropertyChanged();
        }
    }
    public string LastName
    {
        get => _islanderService.LastName;
        set
        {
            _islanderService.LastName = value;
            OnPropertyChanged();
        }
    }
    public string Password
    {
        get => _islanderService.Password;
        set
        {
            _islanderService.Password = value;
            OnPropertyChanged();
        }
    }
}
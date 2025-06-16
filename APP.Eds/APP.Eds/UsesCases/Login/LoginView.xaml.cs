using System.ComponentModel;

namespace APP.Eds.UsesCases.Login;

public partial class LoginView : ContentPage
{
    private LoginViewModel _viewModel;
    public LoginView()
    {
        InitializeComponent();
        _viewModel = new LoginViewModel();
        BindingContext = _viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }
    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_viewModel.GotoLoginPinView))
        {
            if (_viewModel.GotoLoginPinView == true)
            {
                ShowLoginPinView();
            }
            else
            {
                HideLoginPinView();
            }
        }
    }

    private void ShowLoginPinView()
    {
        var loginPinContentView = new LoginPinView(this, _viewModel);
        LoginPinContentView.Content = loginPinContentView.Content;
        LoginPinContentView.IsVisible = true;
        ContentLogin.IsVisible = false;
        HeaderLogin.IsVisible = false;

    }

    public void HideLoginPinView()
    {
        ContentLogin.IsVisible = true;
        HeaderLogin.IsVisible = true;
        LoginPinContentView.IsVisible = false;
        LoginPinContentView.Content = null;
    }
}
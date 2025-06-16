using APP.Eds.Resources.Custom;

namespace APP.Eds.UsesCases.Login;

public partial class LoginPinView : ContentView
{
    private LoginView loginView;
    private LoginViewModel _viewModel;
    public LoginPinView(LoginView loginView, LoginViewModel viewModel)
    {
        InitializeComponent();
        this.loginView = loginView;
        _viewModel = viewModel;

        // Configuración para avanzar al siguiente PIN
        PinEntry1.PinCompleted += (s, e) => PinEntry2.SetFocus();
        PinEntry2.PinCompleted += (s, e) => PinEntry3.SetFocus();
        PinEntry3.PinCompleted += (s, e) => PinEntry4.SetFocus();
        PinEntry4.PinCompleted += (s, e) => _viewModel.OnPinCompleted();

        // Configuración para retroceder al anterior PIN
        PinEntry2.PinTextChanged += OnPinTextChanged;
        PinEntry3.PinTextChanged += OnPinTextChanged;
        PinEntry4.PinTextChanged += OnPinTextChanged;

    }

    // Manejador del evento PinTextChanged
    private void OnPinTextChanged(object sender, TextChangedEventArgs e)
    {
        var currentEntry = sender as CustomEntryPin;

        // Si el nuevo texto está vacío y se presionó "Delete", mueve el foco hacia atrás
        if (string.IsNullOrEmpty(e.NewTextValue) && currentEntry?.Text.Length < e.OldTextValue.Length)
        {
            // Se mueve el foco al campo anterior
            MoveFocusBack(currentEntry);
        }
    }

    // Función para mover el foco al campo anterior
    private void MoveFocusBack(CustomEntryPin currentEntry)
    {
        if (currentEntry == PinEntry2)
        {
            PinEntry1.SetFocus();
        }
        else if (currentEntry == PinEntry3)
        {
            PinEntry2.SetFocus();
        }
        else if (currentEntry == PinEntry4)
        {
            PinEntry3.SetFocus();
        }
    }

    private void BackToLoginClicked(object sender, EventArgs e)
    {
        loginView?.HideLoginPinView();
    }

}

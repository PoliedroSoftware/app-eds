using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using APP.Eds.Services.Alert;
using APP.Eds.Services.Authentication;

namespace APP.Eds.UsesCases.Login
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly IAlertService _alertService;
        private readonly AuthenticationService _authenticationService;
        private string _codeCountry;
        private string _phone;
        private string _password;
        private string _responseMessage;
        private string AuthToken;
        private bool _gotoLoginPinView = false;
        private string _pinEntry1;
        private string _pinEntry2;
        private string _pinEntry3;
        private string _pinEntry4;


        public ICommand ValidationCommand { get; }
        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand VerifyPhoneCommand { get; }

        #region Properties

        public string CodeCountry
        {
            get => _codeCountry;
            set
            {
                _codeCountry = value;
                OnPropertyChanged();
            }
        }

        public string Phone
        {
            get => _phone;
            set
            {
                _phone = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public string ResponseMessage
        {
            get => _responseMessage;
            set
            {
                _responseMessage = value;
                OnPropertyChanged();
            }
        }
        public bool GotoLoginPinView
        {
            get => _gotoLoginPinView;
            set
            {
                _gotoLoginPinView = value;
                OnPropertyChanged();
            }
        }

        public string PinEntry1
        {
            get => _pinEntry1;
            set
            {
                _pinEntry1 = value;
                OnPropertyChanged();
            }
        }

        public string PinEntry2
        {
            get => _pinEntry2;
            set
            {
                _pinEntry2 = value;
                OnPropertyChanged();
            }
        }

        public string PinEntry3
        {
            get => _pinEntry3;
            set
            {
                _pinEntry3 = value;
                OnPropertyChanged();
            }
        }

        public string PinEntry4
        {
            get => _pinEntry4;
            set
            {
                _pinEntry4 = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public LoginViewModel()
        {
            _alertService = new AlertService();
            _authenticationService = new AuthenticationService();
            ValidationCommand = new Command(async () => await Validation());
            LoginCommand = new Command(async () => await Login());
            RegisterCommand = new Command(async () => await Register());
            VerifyPhoneCommand = new Command(async () => await VerifyPhone());
        }


        public void OnPinCompleted()
        {
            Password = $"{PinEntry1}{PinEntry2}{PinEntry3}{PinEntry4}";
            Login();
        }

        private async Task Validation()
        {
            if (!string.IsNullOrEmpty(Phone))
            {
                GotoLoginPinView = true;
            }

        }
        private async Task Login()
        {
            if (Password == "1234")
            {
                ResponseMessage = await _authenticationService.Login(CodeCountry, Phone, Password);
            }
            else
            {
                await _alertService.ShowAlert("Oops! Wrong Password", "The password you entered is incorrect. Please try again.", "OK");
            }

        }

        private async Task Register()
        {
            ResponseMessage = await _authenticationService.Register(CodeCountry, Phone, Password);
        }

        private async Task VerifyPhone()
        {
            ResponseMessage = await _authenticationService.VerifyPhone(CodeCountry, Phone);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

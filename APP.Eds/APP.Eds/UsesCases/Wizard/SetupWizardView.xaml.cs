using APP.Eds.Services.Setup;
using System.ComponentModel;

namespace APP.Eds.UsesCases.Wizard
{
    public partial class SetupWizardView : ContentPage, INotifyPropertyChanged
    {
        private SetupService _setupService;

        public SetupWizardView()
        {
            InitializeComponent();
            _setupService = new SetupService();
            BindingContext = _setupService;
        }

        public new event PropertyChangedEventHandler? PropertyChanged;

        protected new virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
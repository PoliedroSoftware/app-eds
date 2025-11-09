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
        private void NumericEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                string input = e.NewTextValue;
                bool esValido = double.TryParse(
                    input,
                    System.Globalization.NumberStyles.Any,
                    new System.Globalization.CultureInfo("es-CO"),
                    out _);

                if (!string.IsNullOrEmpty(input) && !esValido)
                {
                    entry.Text = e.OldTextValue;
                }
            }
        }
        protected new virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
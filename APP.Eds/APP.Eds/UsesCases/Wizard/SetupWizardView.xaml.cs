using APP.Eds.Services.Wizard;
using APP.Eds.Services.Copilot;
using APP.Eds.Components.PopUp;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Maui.Views;

namespace APP.Eds.UsesCases.Wizard
{
    public partial class SetupWizardView : ContentPage, INotifyPropertyChanged
    {
        private WizardService _wizardService;
        private CopilotService _copilotService;
        private bool _isLoading;

        public WizardService WizardService => _wizardService;
        public CopilotService CopilotService => _copilotService;

        public int CompletedSteps => _wizardService.GetCompletedStepsCount();
        public int TotalSteps => _wizardService.Steps.Count;
        public double CompletionPercentage => _wizardService.GetCompletionPercentage();

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public ICommand NavigateToStepCommand { get; private set; }
        public ICommand NextStepCommand { get; private set; }
        public ICommand PreviousStepCommand { get; private set; }
        public ICommand ValidateStepCommand { get; private set; }
        public ICommand FinishWizardCommand { get; private set; }

        public SetupWizardView()
        {
            InitializeComponent();
            _wizardService = new WizardService();
            _copilotService = new CopilotService();
            
            InitializeCommands();
            BindingContext = this;
            
            // Initial validation
            _ = Task.Run(async () => await _wizardService.RefreshValidationAsync());

            // ✨ Animar entrada del botón flotante
            AnimateFloatingButtonEntry();
        }

        private void InitializeCommands()
        {
            NavigateToStepCommand = new Command(async () => await NavigateToCurrentStep());
            NextStepCommand = new Command(async () => await GoNextStep(), () => _wizardService.CanGoNext);
            PreviousStepCommand = new Command(async () => await GoPreviousStep(), () => _wizardService.CanGoPrevious);
            ValidateStepCommand = new Command(async () => await ValidateCurrentStep());
            FinishWizardCommand = new Command(async () => await FinishWizard());
        }

        // ✨ MEJORADO: Animación de entrada del botón flotante
        private async void AnimateFloatingButtonEntry()
        {
            await Task.Delay(500); // Esperar a que la página se cargue

            if (FloatingChatButton != null)
            {
                FloatingChatButton.Scale = 0;
                FloatingChatButton.Opacity = 0;

                await Task.WhenAll(
                    FloatingChatButton.ScaleTo(1, 600, Easing.SpringOut),
                    FloatingChatButton.FadeTo(1, 400, Easing.CubicOut)
                );

                // Pequeña animación de "rebote" para llamar la atención
                await FloatingChatButton.ScaleTo(1.1, 100, Easing.CubicOut);
                await FloatingChatButton.ScaleTo(1, 100, Easing.CubicIn);
            }
        }

        // ✨ MEJORADO: Manejador del botón flotante con animación
        private async void OnFloatingChatTapped(object sender, EventArgs e)
        {
            try
            {
                // Animación de "presionar" el botón
                if (sender is Frame button)
                {
                    await button.ScaleTo(0.9, 50, Easing.CubicOut);
                    await button.ScaleTo(1, 100, Easing.SpringOut);
                }

                var currentStepContext = _wizardService.CurrentStep?.Title ?? "";
                var chatPopup = new FloatingChatPopup(_copilotService, currentStepContext);
                
                await this.ShowPopupAsync(chatPopup);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo abrir el asistente:\n\n{ex.Message}", "OK");
            }
        }

        // ✨ NUEVO: Animación de hover/pulso para el botón flotante
        private async void OnFloatingChatPointerEntered(object sender, PointerEventArgs e)
        {
            if (sender is Frame button)
            {
                await button.ScaleTo(1.1, 150, Easing.CubicOut);
            }
        }

        private async void OnFloatingChatPointerExited(object sender, PointerEventArgs e)
        {
            if (sender is Frame button)
            {
                await button.ScaleTo(1, 150, Easing.CubicIn);
            }
        }

        // ✨ NUEVO: Animación de pulso continuo para llamar la atención
        private async void StartFloatingButtonPulseAnimation()
        {
            if (FloatingChatButton == null) return;

            while (FloatingChatButton.IsVisible)
            {
                await FloatingChatButton.ScaleTo(1.05, 1000, Easing.SinInOut);
                await FloatingChatButton.ScaleTo(1, 1000, Easing.SinInOut);
                await Task.Delay(3000); // Pausa entre pulsos
            }
        }

        private async Task NavigateToCurrentStep()
        {
            if (_wizardService.CurrentStep?.ViewType == null) return;

            try
            {
                IsLoading = true;
                
                var page = Activator.CreateInstance(_wizardService.CurrentStep.ViewType) as Page;
                if (page != null)
                {
                    await Navigation.PushAsync(page);
                }
            }
            catch (Exception ex)
            {
                await CustomAlert.ShowErrorAsync($"No se pudo abrir el formulario:\n\n{ex.Message}", "Error de Navegación");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GoNextStep()
        {
            if (_wizardService.CanGoNext)
            {
                _wizardService.SetCurrentStep(_wizardService.CurrentStepIndex + 1);
                await ValidateCurrentStep();
                
                // Refresh command states
                ((Command)NextStepCommand).ChangeCanExecute();
                ((Command)PreviousStepCommand).ChangeCanExecute();
            }
        }

        private async Task GoPreviousStep()
        {
            if (_wizardService.CanGoPrevious)
            {
                _wizardService.SetCurrentStep(_wizardService.CurrentStepIndex - 1);
                
                // Refresh command states
                ((Command)NextStepCommand).ChangeCanExecute();
                ((Command)PreviousStepCommand).ChangeCanExecute();
            }
        }

        private async Task ValidateCurrentStep()
        {
            try
            {
                IsLoading = true;
                await _wizardService.ValidateCurrentStepAsync();
                
                // Refresh command states
                ((Command)NextStepCommand).ChangeCanExecute();
                ((Command)PreviousStepCommand).ChangeCanExecute();
                
                // Refresh progress display
                UpdateProgressDisplay();
            }
            catch (Exception ex)
            {
                await CustomAlert.ShowErrorAsync($"Error al validar el paso de configuración:\n\n{ex.Message}", "Error de Validación");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task FinishWizard()
        {
            try
            {
                IsLoading = true;
                
                // Check if all steps are completed
                if (CompletionPercentage < 100)
                {
                    bool confirm = await CustomAlert.ShowConfirmAsync(
                        $"La configuración está {CompletionPercentage:F0}% completa.\n\n" +
                        $"Pasos completados: {CompletedSteps}/{TotalSteps}\n\n" +
                        $"¿Desea finalizar de todas formas? Podrá completar los pasos restantes más tarde.",
                        "Configuración Incompleta",
                        "Finalizar",
                        "Continuar");
                    
                    if (!confirm) return;
                }
                
                // Show completion message
                await CustomAlert.ShowSuccessAsync(
                    $"¡Configuración del sistema completada!\n\n" +
                    $"• Pasos completados: {CompletedSteps}/{TotalSteps}\n" +
                    $"• Progreso: {CompletionPercentage:F0}%\n\n" +
                    $"Ya puede comenzar a usar la aplicación.",
                    "¡Configuración Exitosa!");
                
                // Navigate back to main menu
                await Navigation.PopToRootAsync();
            }
            catch (Exception ex)
            {
                await CustomAlert.ShowErrorAsync($"Error al finalizar la configuración:\n\n{ex.Message}", "Error del Sistema");
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // Refresh validation when returning from a form
            await _wizardService.RefreshValidationAsync();
            
            // Update command states
            ((Command)NextStepCommand).ChangeCanExecute();
            ((Command)PreviousStepCommand).ChangeCanExecute();
            
            // Refresh progress display
            UpdateProgressDisplay();
        }

        private void UpdateProgressDisplay()
        {
            OnPropertyChanged(nameof(CompletedSteps));
            OnPropertyChanged(nameof(CompletionPercentage));
            
            // Update progress bar
            if (OverallProgressBar != null)
            {
                OverallProgressBar.Progress = CompletionPercentage / 100.0;
            }
        }

        public new event PropertyChangedEventHandler? PropertyChanged;

        protected new virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
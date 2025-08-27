using APP.Eds.Services.Wizard;
using APP.Eds.Services.Copilot;
using APP.Eds.Components.PopUp;
using System.ComponentModel;
using System.Windows.Input;

namespace APP.Eds.UsesCases.Wizard
{
    public partial class SetupWizardView : ContentPage, INotifyPropertyChanged
    {
        private WizardService _wizardService;
        private CopilotService _copilotService;
        private bool _isLoading;
        private bool _showHelpResponse;

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

        public bool ShowHelpResponse
        {
            get => _showHelpResponse;
            set
            {
                _showHelpResponse = value;
                OnPropertyChanged();
            }
        }

        public ICommand NavigateToStepCommand { get; private set; }
        public ICommand NextStepCommand { get; private set; }
        public ICommand PreviousStepCommand { get; private set; }
        public ICommand ValidateStepCommand { get; private set; }
        public ICommand GetHelpCommand { get; private set; }
        public ICommand GetContextualHelpCommand { get; private set; }
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
        }

        private void InitializeCommands()
        {
            NavigateToStepCommand = new Command(async () => await NavigateToCurrentStep());
            NextStepCommand = new Command(async () => await GoNextStep(), () => _wizardService.CanGoNext);
            PreviousStepCommand = new Command(async () => await GoPreviousStep(), () => _wizardService.CanGoPrevious);
            ValidateStepCommand = new Command(async () => await ValidateCurrentStep());
            GetHelpCommand = new Command<string>(async (question) => await GetHelp(question));
            GetContextualHelpCommand = new Command(async () => await GetContextualHelp());
            FinishWizardCommand = new Command(async () => await FinishWizard());
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

        private async Task GetHelp(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                await CustomAlert.ShowInfoAsync("Escriba una pregunta específica sobre la configuración actual para obtener ayuda personalizada", "Pregunta Requerida");
                return;
            }

            try
            {
                string currentStepContext = _wizardService.CurrentStep?.Title ?? "";
                await _copilotService.GetHelpAsync(question, currentStepContext);
                ShowHelpResponse = !string.IsNullOrEmpty(_copilotService.Response);
                
                if (ShowHelpResponse)
                {
                    await CustomAlert.ShowSuccessAsync("Se ha generado una respuesta de ayuda personalizada para su consulta", "Ayuda Obtenida");
                }
                
                // Clear the entry
                if (HelpEntry != null)
                    HelpEntry.Text = string.Empty;
            }
            catch (Exception ex)
            {
                await CustomAlert.ShowErrorAsync($"No se pudo obtener ayuda del asistente:\n\n{ex.Message}", "Error de Ayuda");
            }
        }

        private async Task GetContextualHelp()
        {
            try
            {
                string currentStep = _wizardService.CurrentStep?.Title ?? "configuración";
                string contextualQuestion = $"¿Cómo configuro {currentStep}? ¿Qué debo hacer en este paso?";
                
                await _copilotService.GetHelpAsync(contextualQuestion, _wizardService.CurrentStep?.Title ?? "");
                ShowHelpResponse = !string.IsNullOrEmpty(_copilotService.Response);
                
                if (ShowHelpResponse)
                {
                    await CustomAlert.ShowInfoAsync($"Se ha generado ayuda contextual para el paso: {currentStep}", "Ayuda Contextual");
                }
            }
            catch (Exception ex)
            {
                await CustomAlert.ShowErrorAsync($"Error al obtener ayuda contextual:\n\n{ex.Message}", "Error de Ayuda");
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
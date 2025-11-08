using APP.Eds.Components.PopUp;
using APP.Eds.Models.Compartiment;
using APP.Eds.Models.Island;
using APP.Eds.Models.Islander;
using APP.Eds.Models.Product;
using APP.Eds.Models.Provider;
using APP.Eds.Models.Tank;
using APP.Eds.Services.Copilot;
using APP.Eds.Services.Wizard;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace APP.Eds.UsesCases.Wizard
{
    public partial class SetupWizardView : ContentPage, INotifyPropertyChanged
    {
        private WizardService _wizardService;
        private CopilotService _copilotService;
        private bool _isLoading;

       
        public ObservableCollection<EdsModel> EdsList { get; set; } = new();
        public ObservableCollection<IslandModel> Islands { get; set; } = new();
        public ObservableCollection<TankModel> Tanks { get; set; } = new();
        public ObservableCollection<CompartimentModel> Compartiments { get; set; } = new();
        public ObservableCollection<ProductModel> Products { get; set; } = new();
        public ObservableCollection<IslanderModel> Islanders { get; set; } = new();
        public ObservableCollection<ProviderModel> Providers { get; set; } = new();
        
            
            
            

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

        public ICommand AddEdsCommand { get; private set; }
        public ICommand AddIslandCommand { get; private set; }
        public ICommand AddTankCommand { get; private set; }
        public ICommand AddCompartimentCommand { get; private set; }
        public ICommand AddProductCommand { get; private set; }
        public ICommand AddIslanderCommand { get; private set; }
        public ICommand AddProviderCommand { get; private set; }


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
        }

        private void InitializeCommands()
        {
            AddEdsCommand = new Command(()=>AddEds());
            AddIslandCommand = new Command(() => AddIsland());
            AddTankCommand = new Command(() => AddTank());
            AddCompartimentCommand = new Command(() => AddCompartiment());
            AddProductCommand = new Command(() => AddProduct());
            AddIslanderCommand = new Command(() => AddIslander());
            AddProviderCommand = new Command(() => AddProvider());

            NavigateToStepCommand = new Command(async () => await NavigateToCurrentStep());
            NextStepCommand = new Command(async () => await GoNextStep(), () => _wizardService.CanGoNext);
            PreviousStepCommand = new Command(async () => await GoPreviousStep(), () => _wizardService.CanGoPrevious);
            ValidateStepCommand = new Command(async () => await ValidateCurrentStep());
            FinishWizardCommand = new Command(async () => await FinishWizard());
        }
        private void AddEds()
        {
            EdsList.Add(new EdsModel
            {
                Name = "",
                Nit = "",
                Address = "",
                Sicom = ""
            });
        }

        private void AddIsland()
        {
            Islands.Add(new IslandModel
            {
                Description = ""
            });
        }
        private void AddTank()
        {
            Tanks.Add(new TankModel
            {
            });
        }
        private void AddCompartiment()
        {
            Compartiments.Add(new CompartimentModel
            {
            });
        }
        private void AddProduct()
        {
            Products.Add(new ProductModel
            {
            });
        }
        private void AddIslander()
        {
            Islanders.Add(new IslanderModel
            {
                Email = string.Empty,
                FirstName = string.Empty,
                LastName = string.Empty,
                Name = string.Empty,
                Password = string.Empty,
            });
        }
        private void AddProvider()
        {
            Providers.Add(new ProviderModel
            {
            });
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
          /*  if (OverallProgressBar != null)
            {
                OverallProgressBar.Progress = CompletionPercentage / 100.0;
            }*/
        }

        public new event PropertyChangedEventHandler? PropertyChanged;

        protected new virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
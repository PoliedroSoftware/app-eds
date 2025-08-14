using System.Collections.ObjectModel;
using System.ComponentModel;
using APP.Eds.Models.Wizard;
using APP.Eds.UsesCases.Business;
using APP.Eds.UsesCases.Provider;
using APP.Eds.UsesCases.Product;
using APP.Eds.UsesCases.Eds;
using APP.Eds.UsesCases.Island;
using APP.Eds.UsesCases.Dispensers;
using APP.Eds.UsesCases.Hose;
using APP.Eds.UsesCases.Tank;
using APP.Eds.UsesCases.Compartiment;
using APP.Eds.UsesCases.Islander;
using APP.Eds.Services.Business;
using APP.Eds.Services.Provider;
using APP.Eds.Services.Product;
using APP.Eds.Services.Eds;
using APP.Eds.Services.Island;
using APP.Eds.Services.Dispensers;
using APP.Eds.Services.Hose;
using APP.Eds.Services.Tank;
using APP.Eds.Services.Compartiment;
using APP.Eds.Services.Islander;

namespace APP.Eds.Services.Wizard
{
    public class WizardService : INotifyPropertyChanged
    {
        private int _currentStepIndex;
        private WizardStep? _currentStep;

        public ObservableCollection<WizardStep> Steps { get; private set; }

        public WizardStep? CurrentStep
        {
            get => _currentStep;
            set
            {
                _currentStep = value;
                OnPropertyChanged();
            }
        }

        public int CurrentStepIndex
        {
            get => _currentStepIndex;
            set
            {
                _currentStepIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(CanGoPrevious));
            }
        }

        public bool CanGoNext => CurrentStepIndex < Steps.Count - 1 && (CurrentStep?.IsCompleted ?? false);
        public bool CanGoPrevious => CurrentStepIndex > 0;
        public bool IsComplete => Steps.All(s => s.IsCompleted);

        public WizardService()
        {
            Steps = new ObservableCollection<WizardStep>();
            InitializeSteps();
            SetCurrentStep(0);
        }

        private void InitializeSteps()
        {
            Steps.Add(new WizardStep
            {
                Id = "business",
                Title = "Negocio",
                Description = "Crear el negocio principal",
                ViewType = typeof(BusinessPostView),
                Order = 1,
                Icon = "🏢",
                IsEnabled = true
            });

            Steps.Add(new WizardStep
            {
                Id = "provider",
                Title = "Proveedor",
                Description = "Agregar al menos un proveedor",
                ViewType = typeof(ProviderPostView),
                Order = 2,
                Icon = "🚚",
                IsEnabled = false
            });

            Steps.Add(new WizardStep
            {
                Id = "product",
                Title = "Productos",
                Description = "Agregar productos y sus precios",
                ViewType = typeof(ProductPostView),
                Order = 3,
                Icon = "📦",
                IsEnabled = false
            });

            Steps.Add(new WizardStep
            {
                Id = "eds",
                Title = "EDS",
                Description = "Crear estaciones de servicio",
                ViewType = typeof(EdsPostView),
                Order = 4,
                Icon = "⛽",
                IsEnabled = false
            });

            Steps.Add(new WizardStep
            {
                Id = "island",
                Title = "Islas",
                Description = "Crear islas por cada EDS",
                ViewType = typeof(IslandPostView),
                Order = 5,
                Icon = "🏝️",
                IsEnabled = false
            });

            Steps.Add(new WizardStep
            {
                Id = "dispenser",
                Title = "Dispensadores",
                Description = "Crear dispensadores por isla",
                ViewType = typeof(DispensersPostView),
                Order = 6,
                Icon = "🔧",
                IsEnabled = false
            });

            Steps.Add(new WizardStep
            {
                Id = "hose",
                Title = "Mangueras",
                Description = "Crear mangueras por dispensador",
                ViewType = typeof(HosePostView),
                Order = 7,
                Icon = "🔗",
                IsEnabled = false
            });

            Steps.Add(new WizardStep
            {
                Id = "tank",
                Title = "Tanques",
                Description = "Crear tanques por EDS",
                ViewType = typeof(TankPostView),
                Order = 8,
                Icon = "🛢️",
                IsEnabled = false
            });

            Steps.Add(new WizardStep
            {
                Id = "compartment",
                Title = "Compartimientos",
                Description = "Crear compartimientos por tanque",
                ViewType = typeof(CompartimentPostView),
                Order = 9,
                Icon = "📦",
                IsEnabled = false
            });

            Steps.Add(new WizardStep
            {
                Id = "islander",
                Title = "Isleros",
                Description = "Asignar operarios por EDS",
                ViewType = typeof(IslanderPostView),
                Order = 10,
                Icon = "👷",
                IsEnabled = false
            });
        }

        public void SetCurrentStep(int index)
        {
            if (index >= 0 && index < Steps.Count)
            {
                // Deactivate all steps
                foreach (var step in Steps)
                {
                    step.IsActive = false;
                }

                CurrentStepIndex = index;
                CurrentStep = Steps[index];
                CurrentStep.IsActive = true;
            }
        }

        public async Task<bool> ValidateCurrentStepAsync()
        {
            if (CurrentStep == null) return false;

            try
            {
                bool isValid = CurrentStep.Id switch
                {
                    "business" => await ValidateBusinessStepAsync(),
                    "provider" => await ValidateProviderStepAsync(),
                    "product" => await ValidateProductStepAsync(),
                    "eds" => await ValidateEdsStepAsync(),
                    "island" => await ValidateIslandStepAsync(),
                    "dispenser" => await ValidateDispenserStepAsync(),
                    "hose" => await ValidateHoseStepAsync(),
                    "tank" => await ValidateTankStepAsync(),
                    "compartment" => await ValidateCompartmentStepAsync(),
                    "islander" => await ValidateIslanderStepAsync(),
                    _ => false
                };

                CurrentStep.IsCompleted = isValid;
                UpdateNextStepAvailability();
                
                return isValid;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error validando paso: {ex.Message}", "OK");
                return false;
            }
        }

        private void UpdateNextStepAvailability()
        {
            for (int i = 0; i < Steps.Count; i++)
            {
                if (i == 0)
                {
                    Steps[i].IsEnabled = true;
                }
                else
                {
                    Steps[i].IsEnabled = Steps[i - 1].IsCompleted;
                }
            }

            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(IsComplete));
        }

        // Validation methods for each step
        private async Task<bool> ValidateBusinessStepAsync()
        {
            var businessService = new BusinessService();
            // For now, consider step complete if BusinessList has any items
            // This can be enhanced later with actual API calls
            return businessService.BusinessList.Any();
        }

        private async Task<bool> ValidateProviderStepAsync()
        {
            var providerService = new ProviderService();
            await providerService.GetProvidersAsync();
            return providerService.ProviderList.Any();
        }

        private async Task<bool> ValidateProductStepAsync()
        {
            var productService = new ProductService();
            // Check if ProductTypeList has items as a proxy for products
            return productService.ProductTypeList.Any();
        }

        private async Task<bool> ValidateEdsStepAsync()
        {
            var edsService = new EdsService();
            await edsService.GetEdssAsync();
            return edsService.EdsList.Any();
        }

        private async Task<bool> ValidateIslandStepAsync()
        {
            var islandService = new IslandService();
            await islandService.GetIslandAsync();
            return islandService.IslandList.Any();
        }

        private async Task<bool> ValidateDispenserStepAsync()
        {
            var dispenserService = new DispensersService();
            await dispenserService.GetDispensersAsync();
            return dispenserService.DispensersList.Any();
        }

        private async Task<bool> ValidateHoseStepAsync()
        {
            var hoseService = new HoseService();
            await hoseService.GetHoseAsync();
            return hoseService.HoseList.Any();
        }

        private async Task<bool> ValidateTankStepAsync()
        {
            var tankService = new TankService();
            await tankService.GetTankAsync();
            return tankService.TankList.Any();
        }

        private async Task<bool> ValidateCompartmentStepAsync()
        {
            var compartmentService = new CompartimentService();
            await compartmentService.GetCompartimentAsync();
            return compartmentService.CompartimentList.Any();
        }

        private async Task<bool> ValidateIslanderStepAsync()
        {
            var islanderService = new IslanderService();
            await islanderService.GetIslandersAsync();
            return islanderService.IslanderList.Any();
        }

        public async Task RefreshValidationAsync()
        {
            for (int i = 0; i <= CurrentStepIndex; i++)
            {
                var previousStep = CurrentStep;
                SetCurrentStep(i);
                await ValidateCurrentStepAsync();
            }
            
            if (previousStep != null)
            {
                SetCurrentStep(previousStep.Order - 1);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
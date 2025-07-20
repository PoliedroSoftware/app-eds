using APP.Eds.UsesCases.Business;
using APP.Eds.UsesCases.Capacity;
using APP.Eds.UsesCases.Category;
using APP.Eds.UsesCases.Compartiment;
using APP.Eds.UsesCases.CompartimentCapacity;
using APP.Eds.UsesCases.Court;
using APP.Eds.UsesCases.Dispensers;
using APP.Eds.UsesCases.DispenserType;
using APP.Eds.UsesCases.Eds;
using APP.Eds.UsesCases.EdsTank;
using APP.Eds.UsesCases.Expenditures;
using APP.Eds.UsesCases.Hose;
using APP.Eds.UsesCases.HoseHistory;
using APP.Eds.UsesCases.Inventory;
using APP.Eds.UsesCases.Island;
using APP.Eds.UsesCases.Islander;
using APP.Eds.UsesCases.Product;
using APP.Eds.UsesCases.ProductCompartiment;
using APP.Eds.UsesCases.ProductType;
using APP.Eds.UsesCases.Provider;
using APP.Eds.UsesCases.Shopping;
using APP.Eds.UsesCases.ShoppingProduct;
using APP.Eds.UsesCases.Tank;
using APP.Eds.UsesCases.TypeOfCollection;
using APP.Eds.Views.Popups;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;
using APP.Eds.Services.Config; // Importar GlobalTranslations
using APP.Eds.Services.Translations;
using System.ComponentModel;

namespace APP.Eds.Services.Navigation
{
    public class MainService : BindableObject
    {
        public ObservableCollection<CategoryModel> Categories { get; set; }
        public bool IsIslander => Preferences.Get("userRole", "") == "User";
        public ICommand NavigateToCourtCommand { get; }
        public readonly TranslationsService _translations = new TranslationsService();
        private string _translatedAdministrationCategoryKey;
        public string TranslatedAdministrationCategoryKey
        {
            get => _translatedAdministrationCategoryKey;
            set
            {
                _translatedAdministrationCategoryKey = value;
                OnPropertyChanged(nameof(TranslatedAdministrationCategoryKey));
            }
        }

        private string _translatedTanksAndCompartmentsCategoryKey;
        public string TranslatedTanksAndCompartmentsCategoryKey
        {
            get => _translatedTanksAndCompartmentsCategoryKey;
            set
            {
                _translatedTanksAndCompartmentsCategoryKey = value;
                OnPropertyChanged(nameof(TranslatedTanksAndCompartmentsCategoryKey));
            }
        }

        private string _translatedDispensersAndHosesCategoryKey;
        public string TranslatedDispensersAndHosesCategoryKey
        {
            get => _translatedDispensersAndHosesCategoryKey;
            set
            {
                _translatedDispensersAndHosesCategoryKey = value;
                OnPropertyChanged(nameof(TranslatedDispensersAndHosesCategoryKey));
            }
        }

        private string _translatedProductsAndShoppingCategoryKey;
        public string TranslatedProductsAndShoppingCategoryKey
        {
            get => _translatedProductsAndShoppingCategoryKey;
            set
            {
                _translatedProductsAndShoppingCategoryKey = value;
                OnPropertyChanged(nameof(TranslatedProductsAndShoppingCategoryKey));
            }
        }

        private string _translatedEdsAndOthersCategoryKey;
        public string TranslatedEdsAndOthersCategoryKey
        {
            get => _translatedEdsAndOthersCategoryKey;
            set
            {
                _translatedEdsAndOthersCategoryKey = value;
                OnPropertyChanged(nameof(TranslatedEdsAndOthersCategoryKey));
            }
        }

        private string _translatedInventoryCategoryKey;
        public string TranslatedInventoryCategoryKey
        {
            get => _translatedInventoryCategoryKey;
            set
            {
                _translatedInventoryCategoryKey = value;
                OnPropertyChanged(nameof(TranslatedInventoryCategoryKey));
            }
        }

        private string _translatedCourtMenuItemKey;
        public string TranslatedCourtMenuItemKey
        {
            get => _translatedCourtMenuItemKey;
            set
            {
                _translatedCourtMenuItemKey = value;
                OnPropertyChanged(nameof(TranslatedCourtMenuItemKey));
            }
        }

        private string _translatedBusinessMenuItemKey;
        public string TranslatedBusinessMenuItemKey
        {
            get => _translatedBusinessMenuItemKey;
            set
            {
                _translatedBusinessMenuItemKey = value;
                OnPropertyChanged(nameof(TranslatedBusinessMenuItemKey));
            }
        }

        private string _translatedProviderMenuItemKey;
        public string TranslatedProviderMenuItemKey
        {
            get => _translatedProviderMenuItemKey;
            set
            {
                _translatedProviderMenuItemKey = value;
                OnPropertyChanged(nameof(TranslatedProviderMenuItemKey));
            }
        }

        private string _translatedCapacityMenuItemKey;
        public string TranslatedCapacityMenuItemKey
        {
            get => _translatedCapacityMenuItemKey;
            set
            {
                _translatedCapacityMenuItemKey = value;
                OnPropertyChanged(nameof(TranslatedCapacityMenuItemKey));
            }
        }

        private string _translatedCompartmentMenuItemKey;
        public string TranslatedCompartmentMenuItemKey
        {
            get => _translatedCompartmentMenuItemKey;
            set
            {
                _translatedCompartmentMenuItemKey = value;
                OnPropertyChanged(nameof(TranslatedCompartmentMenuItemKey));
            }
        }

        private string _translatedCompartmentCapacityMenuItemKey;
        public string TranslatedCompartmentCapacityMenuItemKey
        {
            get => _translatedCompartmentCapacityMenuItemKey;
            set
            {
                _translatedCompartmentCapacityMenuItemKey = value;
                OnPropertyChanged(nameof(TranslatedCompartmentCapacityMenuItemKey));
            }
        }

        private string _translatedEdsTankMenuItemKey;
        public string TranslatedEdsTankMenuItemKey
        {
            get => _translatedEdsTankMenuItemKey;
            set
            {
                _translatedEdsTankMenuItemKey = value;
                OnPropertyChanged(nameof(TranslatedEdsTankMenuItemKey));
            }
        }

        private string _translatedTankMenuItemKey;
        public string TranslatedTankMenuItemKey
        {
            get => _translatedTankMenuItemKey;
            set
            {
                _translatedTankMenuItemKey = value;
                OnPropertyChanged(nameof(TranslatedTankMenuItemKey));
            }
        }

        private string _translatedProductCompartmentMenuItemKey;
        public string TranslatedProductCompartmentMenuItemKey
        {
            get => _translatedProductCompartmentMenuItemKey;
            set
            {
                _translatedProductCompartmentMenuItemKey = value;
                OnPropertyChanged(nameof(TranslatedProductCompartmentMenuItemKey));
            }
        }

        private string _translatedDispensersMenuItemKey;
        public string TranslatedDispensersMenuItemKey
        {
            get => _translatedDispensersMenuItemKey;
            set
            {
                _translatedDispensersMenuItemKey = value;
                OnPropertyChanged(nameof(TranslatedDispensersMenuItemKey));
            }
        }

        private string _translatedDispenserTypeMenuItemKey;
        public string TranslatedDispenserTypeMenuItemKey
        {
            get => _translatedDispenserTypeMenuItemKey;
            set
            {
                _translatedDispenserTypeMenuItemKey = value;
                OnPropertyChanged(nameof(TranslatedDispenserTypeMenuItemKey));
            }
        }

        private string _translatedHoseMenuItemKey;
        public string TranslatedHoseMenuItemKey
        {
            get => _translatedHoseMenuItemKey;
            set
            {
                _translatedHoseMenuItemKey = value;
                OnPropertyChanged(nameof(TranslatedHoseMenuItemKey));
            }
        }

        private string _translatedHoseHistoryMenuItemKey;
        public string TranslatedHoseHistoryMenuItemKey
        {
            get => _translatedHoseHistoryMenuItemKey;
            set
            {
                _translatedHoseHistoryMenuItemKey = value;
                OnPropertyChanged(nameof(TranslatedHoseHistoryMenuItemKey));
            }
        }

        private string _translatedProductMenuItemKey;
        public string TranslatedProductMenuItemKey
        {
            get => _translatedProductMenuItemKey;
            set
            {
                _translatedProductMenuItemKey = value;
                OnPropertyChanged(nameof(TranslatedProductMenuItemKey));
            }
        }

        private string _translatedProductTypeMenuItemKey;
        public string TranslatedProductTypeMenuItemKey
        {
            get => _translatedProductTypeMenuItemKey;
            set
            {
                _translatedProductTypeMenuItemKey = value;
                OnPropertyChanged(nameof(TranslatedProductTypeMenuItemKey));
            }
        }

        private string _translatedShoppingMenuItemKey;
        public string TranslatedShoppingMenuItemKey
        {
            get => _translatedShoppingMenuItemKey;
            set
            {
                _translatedShoppingMenuItemKey = value;
                OnPropertyChanged(nameof(TranslatedShoppingMenuItemKey));
            }
        }

        private string _translatedEdsMenuItemKey;
        public string TranslatedEdsMenuItemKey
        {
            get => _translatedEdsMenuItemKey;
            set
            {
                _translatedEdsMenuItemKey = value;
                OnPropertyChanged(nameof(TranslatedEdsMenuItemKey));
            }
        }

        private string _translatedExpenditureMenuItemKey;
        public string TranslatedExpenditureMenuItemKey
        {
            get => _translatedExpenditureMenuItemKey;
            set
            {
                _translatedExpenditureMenuItemKey = value;
                OnPropertyChanged(nameof(TranslatedExpenditureMenuItemKey));
            }
        }

        private string _translatedIslanderMenuItemKey;
        public string TranslatedIslanderMenuItemKey
        {
            get => _translatedIslanderMenuItemKey;
            set
            {
                _translatedIslanderMenuItemKey = value;
                OnPropertyChanged(nameof(TranslatedIslanderMenuItemKey));
            }
        }

        private string _translatedIslandMenuItemKey;
        public string TranslatedIslandMenuItemKey
        {
            get => _translatedIslandMenuItemKey;
            set
            {
                _translatedIslandMenuItemKey = value;
                OnPropertyChanged(nameof(TranslatedIslandMenuItemKey));
            }
        }

        private string _translatedCategoryMenuItemKey;
        public string TranslatedCategoryMenuItemKey
        {
            get => _translatedCategoryMenuItemKey;
            set
            {
                _translatedCategoryMenuItemKey = value;
                OnPropertyChanged(nameof(TranslatedCategoryMenuItemKey));
            }
        }

        private string _translatedTypeOfCollectionMenuItemKey;
        public string TranslatedTypeOfCollectionMenuItemKey
        {
            get => _translatedTypeOfCollectionMenuItemKey;
            set
            {
                _translatedTypeOfCollectionMenuItemKey = value;
                OnPropertyChanged(nameof(TranslatedTypeOfCollectionMenuItemKey));
            }
        }

        private string _translatedInventoryMenuItemKey;
        public string TranslatedInventoryMenuItemKey
        {
            get => _translatedInventoryMenuItemKey;
            set
            {
                _translatedInventoryMenuItemKey = value;
                OnPropertyChanged(nameof(TranslatedInventoryMenuItemKey));
            }
        }
        public MainService()
        {
            LoadTranslationsAsync();

            NavigateToCourtCommand = new Command(async () =>
            {
                if (Application.Current?.MainPage is NavigationPage navPage)
                {
                    await navPage.PushAsync(new CourtPostView());
                }
            });

            if (!IsIslander)
            {
                Categories = new ObservableCollection<CategoryModel>
                {
                    new(TranslatedAdministrationCategoryKey, new List<MenuItemModel>
                    {
                        new(TranslatedCourtMenuItemKey, typeof(CourtPostView)),
                        new(TranslatedBusinessMenuItemKey, typeof(BusinessPostView)),
                        new(TranslatedProviderMenuItemKey, typeof(ProviderPostView))
                    }),
                    new(TranslatedTanksAndCompartmentsCategoryKey, new List<MenuItemModel>
                    {
                        new(TranslatedCapacityMenuItemKey, typeof(CapacityPostView)),
                        new(TranslatedCompartmentMenuItemKey, typeof(CompartimentPostView)),
                        new(TranslatedCompartmentCapacityMenuItemKey, typeof(APP.Eds.UsesCases.CompartimentCapacity.CompartimentCapacityPostView)),
                        new(TranslatedEdsTankMenuItemKey, typeof(EdsTankPostView)),
                        new(TranslatedTankMenuItemKey, typeof(TankPostView)),
                        new(TranslatedProductCompartmentMenuItemKey, typeof(APP.Eds.UsesCases.ProductCompartiment.ProductCompartimentPostView))
                    }),
                    new(TranslatedDispensersAndHosesCategoryKey, new List<MenuItemModel>
                    {
                        new(TranslatedDispensersMenuItemKey, typeof(DispensersPostView)),
                        new(TranslatedDispenserTypeMenuItemKey, typeof(DispenserTypePostView)),
                        new(TranslatedHoseMenuItemKey, typeof(HosePostView)),
                        new(TranslatedHoseHistoryMenuItemKey, typeof(HoseHistoryPostView))
                    }),
                    new(TranslatedProductsAndShoppingCategoryKey, new List<MenuItemModel>
                    {
                        new(TranslatedProductMenuItemKey, typeof(ProductPostView)),
                        new(TranslatedProductTypeMenuItemKey, typeof(ProductTypePostView)),
                        new(TranslatedShoppingMenuItemKey, typeof(ShoppingPostView)),
                    }),
                    new(TranslatedEdsAndOthersCategoryKey, new List<MenuItemModel>
                    {
                        new(TranslatedEdsMenuItemKey, typeof(EdsPostView)),
                        new(TranslatedExpenditureMenuItemKey, typeof(ExpendituresPostView)),
                        new(TranslatedIslanderMenuItemKey, typeof(IslanderPostView)),
                        new(TranslatedIslandMenuItemKey, typeof(IslandPostView)),
                        new(TranslatedCategoryMenuItemKey, typeof(CategoryPostView)),
                        new(TranslatedTypeOfCollectionMenuItemKey, typeof(TypeOfCollectionPostView))
                    }),
                    new(TranslatedInventoryCategoryKey,
                    [
                        new(TranslatedInventoryMenuItemKey, typeof(InventoryPostView)),
                    ]),
                };
            }
            else
            {
                Categories = new ObservableCollection<CategoryModel>();
            }
        }

        private string _translatedLogoutButtonText;
        public string TranslatedLogoutButtonText
        {
            get => _translatedLogoutButtonText;
            set
            {
                _translatedLogoutButtonText = value;
                OnPropertyChanged(nameof(TranslatedLogoutButtonText));
            }
        }

        private string _translatedMenuTitle;
        public string TranslatedMenuTitle
        {
            get => _translatedMenuTitle;
            set
            {
                _translatedMenuTitle = value;
                OnPropertyChanged(nameof(TranslatedMenuTitle));
            }
        }

        private async void LoadTranslationsAsync()
        {
            var currentLanguage = TranslationsService.CurrentLanguage;
            var translations = await _translations.GetTranslationsByLanguageAsync(currentLanguage);

            TranslatedMenuTitle = translations.TryGetValue("MenuPrincipal", out var menuTitle) ? menuTitle : "Menú Principal";
            TranslatedLogoutButtonText = translations.TryGetValue("LogoutButtonText", out var logoutText) ? logoutText : "Cerrar Sesión";

            TranslatedAdministrationCategoryKey = translations.TryGetValue("AdministrationCategory", out var adminCategory) ? adminCategory : "Administración";
            TranslatedTanksAndCompartmentsCategoryKey = translations.TryGetValue("TanksAndCompartmentsCategory", out var tanksCategory) ? tanksCategory : "Tanques y Compartimentos";
            TranslatedDispensersAndHosesCategoryKey = translations.TryGetValue("DispensersAndHosesCategory", out var dispensersCategory) ? dispensersCategory : "Dispensadores y Mangueras";
            TranslatedProductsAndShoppingCategoryKey = translations.TryGetValue("ProductsAndShoppingCategory", out var productsCategory) ? productsCategory : "Productos y Compras";
            TranslatedEdsAndOthersCategoryKey = translations.TryGetValue("EdsAndOthersCategory", out var edsCategory) ? edsCategory : "EDS y Otros";
            TranslatedInventoryCategoryKey = translations.TryGetValue("InventoryCategory", out var inventoryCategory) ? inventoryCategory : "Inventario";

            TranslatedCourtMenuItemKey = translations.TryGetValue("CourtMenuItem", out var courtMenuItem) ? courtMenuItem : "Corte";
            TranslatedBusinessMenuItemKey = translations.TryGetValue("BusinessMenuItem", out var businessMenuItem) ? businessMenuItem : "Negocio";
            TranslatedProviderMenuItemKey = translations.TryGetValue("ProviderMenuItem", out var providerMenuItem) ? providerMenuItem : "Proveedor";
            TranslatedCapacityMenuItemKey = translations.TryGetValue("CapacityMenuItem", out var capacityMenuItem) ? capacityMenuItem : "Capacidad";
            TranslatedCompartmentMenuItemKey = translations.TryGetValue("CompartmentMenuItem", out var compartmentMenuItem) ? compartmentMenuItem : "Compartimento";
            TranslatedCompartmentCapacityMenuItemKey = translations.TryGetValue("CompartmentCapacityMenuItem", out var compartmentCapacityMenuItem) ? compartmentCapacityMenuItem : "Capacidad de Compartimento";
            TranslatedEdsTankMenuItemKey = translations.TryGetValue("EdsTankMenuItem", out var edsTankMenuItem) ? edsTankMenuItem : "Tanque EDS";
            TranslatedTankMenuItemKey = translations.TryGetValue("TankMenuItem", out var tankMenuItem) ? tankMenuItem : "Tanque";
            TranslatedProductCompartmentMenuItemKey = translations.TryGetValue("ProductCompartmentMenuItem", out var productCompartmentMenuItem) ? productCompartmentMenuItem : "Compartimento de Producto";
            TranslatedDispensersMenuItemKey = translations.TryGetValue("DispensersMenuItem", out var dispensersMenuItem) ? dispensersMenuItem : "Dispensadores";
            TranslatedDispenserTypeMenuItemKey = translations.TryGetValue("DispenserTypeMenuItem", out var dispenserTypeMenuItem) ? dispenserTypeMenuItem : "Tipo de Dispensador";
            TranslatedHoseMenuItemKey = translations.TryGetValue("HoseMenuItem", out var hoseMenuItem) ? hoseMenuItem : "Manguera";
            TranslatedHoseHistoryMenuItemKey = translations.TryGetValue("HoseHistoryMenuItem", out var hoseHistoryMenuItem) ? hoseHistoryMenuItem : "Historial de Manguera";
            TranslatedProductMenuItemKey = translations.TryGetValue("ProductMenuItem", out var productMenuItem) ? productMenuItem : "Producto";
            TranslatedProductTypeMenuItemKey = translations.TryGetValue("ProductTypeMenuItem", out var productTypeMenuItem) ? productTypeMenuItem : "Tipo de Producto";
            TranslatedShoppingMenuItemKey = translations.TryGetValue("ShoppingMenuItem", out var shoppingMenuItem) ? shoppingMenuItem : "Compras";
            TranslatedEdsMenuItemKey = translations.TryGetValue("EdsMenuItem", out var edsMenuItem) ? edsMenuItem : "EDS";
            TranslatedExpenditureMenuItemKey = translations.TryGetValue("ExpenditureMenuItem", out var expenditureMenuItem) ? expenditureMenuItem : "Gastos";
            TranslatedIslanderMenuItemKey = translations.TryGetValue("IslanderMenuItem", out var islanderMenuItem) ? islanderMenuItem : "Isleño";
            TranslatedIslandMenuItemKey = translations.TryGetValue("IslandMenuItem", out var islandMenuItem) ? islandMenuItem : "Isla";
            TranslatedCategoryMenuItemKey = translations.TryGetValue("CategoryMenuItem", out var categoryMenuItem) ? categoryMenuItem : "Categoría";
            TranslatedTypeOfCollectionMenuItemKey = translations.TryGetValue("TypeOfCollectionMenuItem", out var typeOfCollectionMenuItem) ? typeOfCollectionMenuItem : "Tipo de Recolección";

            // Re-initialize Categories after translations are loaded
            if (!IsIslander)
            {
                Categories = new ObservableCollection<CategoryModel>
                {
                    new(TranslatedAdministrationCategoryKey, new List<MenuItemModel>
                    {
                        new(TranslatedCourtMenuItemKey, typeof(CourtPostView)),
                        new(TranslatedBusinessMenuItemKey, typeof(BusinessPostView)),
                        new(TranslatedProviderMenuItemKey, typeof(ProviderPostView))
                    }),
                    new(TranslatedTanksAndCompartmentsCategoryKey, new List<MenuItemModel>
                    {
                        new(TranslatedCapacityMenuItemKey, typeof(CapacityPostView)),
                        new(TranslatedCompartmentMenuItemKey, typeof(CompartimentPostView)),
                        new(TranslatedCompartmentCapacityMenuItemKey, typeof(APP.Eds.UsesCases.CompartimentCapacity.CompartimentCapacityPostView)),
                        new(TranslatedEdsTankMenuItemKey, typeof(EdsTankPostView)),
                        new(TranslatedTankMenuItemKey, typeof(TankPostView)),
                        new(TranslatedProductCompartmentMenuItemKey, typeof(APP.Eds.UsesCases.ProductCompartiment.ProductCompartimentPostView))
                    }),
                    new(TranslatedDispensersAndHosesCategoryKey, new List<MenuItemModel>
                    {
                        new(TranslatedDispensersMenuItemKey, typeof(DispensersPostView)),
                        new(TranslatedDispenserTypeMenuItemKey, typeof(DispenserTypePostView)),
                        new(TranslatedHoseMenuItemKey, typeof(HosePostView)),
                        new(TranslatedHoseHistoryMenuItemKey, typeof(HoseHistoryPostView))
                    }),
                    new(TranslatedProductsAndShoppingCategoryKey, new List<MenuItemModel>
                    {
                        new(TranslatedProductMenuItemKey, typeof(ProductPostView)),
                        new(TranslatedProductTypeMenuItemKey, typeof(ProductTypePostView)),
                        new(TranslatedShoppingMenuItemKey, typeof(ShoppingPostView)),
                    }),
                    new(TranslatedEdsAndOthersCategoryKey, new List<MenuItemModel>
                    {
                        new(TranslatedEdsMenuItemKey, typeof(EdsPostView)),
                        new(TranslatedExpenditureMenuItemKey, typeof(ExpendituresPostView)),
                        new(TranslatedIslanderMenuItemKey, typeof(IslanderPostView)),
                        new(TranslatedIslandMenuItemKey, typeof(IslandPostView)),
                        new(TranslatedCategoryMenuItemKey, typeof(CategoryPostView)),
                        new(TranslatedTypeOfCollectionMenuItemKey, typeof(TypeOfCollectionPostView))
                    }),
                    new(TranslatedInventoryCategoryKey,
                    [
                        new(TranslatedInventoryMenuItemKey, typeof(InventoryPostView)),
                    ]),
                };
            }
            else
            {
                Categories = new ObservableCollection<CategoryModel>();
            }
        }

        public class CategoryModel : INotifyPropertyChanged
        {
            private string _title;
            public string Title
            {
                get => _title;
                set
                {
                    if (_title != value)
                    {
                        _title = value;
                        OnPropertyChanged(nameof(Title));
                    }
                }
            }
            public ICommand ShowPopupCommand { get; }

            public List<MenuItemModel> Items { get; set; }

            public CategoryModel(string title, List<MenuItemModel> items)
            {
                _title = title; // Asignar directamente para evitar notificación en el constructor
                Items = items;
                ShowPopupCommand = new Command(() =>
                {
                    var popup = new CategoryPopup(items, title);
                    Application.Current?.MainPage?.ShowPopup(popup);
                });
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class MenuItemModel : INotifyPropertyChanged
        {
            private string _name;
            public string Name
            {
                get => _name;
                set
                {
                    if (_name != value)
                    {
                        _name = value;
                        OnPropertyChanged(nameof(Name));
                    }
                }
            }
            public Type Page { get; set; }
            public Page PageInstance { get; set; }
            public ICommand NavigateCommand { get; }

            public MenuItemModel(string name, Type page)
            {
                _name = name; // Asignar directamente para evitar notificación en el constructor
                Page = page;
                NavigateCommand = new Command(async () =>
                {
                    if (Application.Current?.MainPage is NavigationPage navPage)
                    {

                        if (PageInstance == null)
                        {
                            PageInstance = (Page)Activator.CreateInstance(Page);
                        }

                        await navPage.PushAsync(PageInstance);
                    }
                });

            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

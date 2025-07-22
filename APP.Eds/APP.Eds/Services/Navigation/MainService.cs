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
                        new(TranslatedProductCompartmentMenuItemKey, typeof(APP.Eds.UsesCases.ProductCompartiment.ProductCompartmentPostView))
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
            var result = await new TranslationsService().GetTranslationsByLanguageAsync(currentLanguage);
            GlobalTranslations.SetTranslations(result ?? new Dictionary<string, string>());

            TranslatedMenuTitle = GlobalTranslations.Get("MenuPrincipal", "Menú Principal");
            TranslatedLogoutButtonText = GlobalTranslations.Get("LogoutButtonText", "Cerrar Sesión");

            TranslatedAdministrationCategoryKey = GlobalTranslations.Get("AdministrationCategory", "Administración");
            TranslatedTanksAndCompartmentsCategoryKey = GlobalTranslations.Get("TanksAndCompartmentsCategory", "Tanques y Compartimentos");
            TranslatedDispensersAndHosesCategoryKey = GlobalTranslations.Get("DispensersAndHosesCategory", "Dispensadores y Mangueras");
            TranslatedProductsAndShoppingCategoryKey = GlobalTranslations.Get("ProductsAndShoppingCategory", "Productos y Compras");
            TranslatedEdsAndOthersCategoryKey = GlobalTranslations.Get("EdsAndOthersCategory", "EDS y Otros");
            TranslatedInventoryCategoryKey = GlobalTranslations.Get("InventoryCategory", "Inventario");

            TranslatedCourtMenuItemKey = GlobalTranslations.Get("CourtMenuItem", "Corte");
            TranslatedBusinessMenuItemKey = GlobalTranslations.Get("BusinessMenuItem", "Negocio");
            TranslatedProviderMenuItemKey = GlobalTranslations.Get("ProviderMenuItem", "Proveedor");
            TranslatedCapacityMenuItemKey = GlobalTranslations.Get("CapacityMenuItem", "Capacidad");
            TranslatedCompartmentMenuItemKey = GlobalTranslations.Get("CompartmentMenuItem", "Compartimento");
            TranslatedCompartmentCapacityMenuItemKey = GlobalTranslations.Get("CompartmentCapacityMenuItem", "Capacidad de Compartimento");
            TranslatedEdsTankMenuItemKey = GlobalTranslations.Get("EdsTankMenuItem", "Tanque EDS");
            TranslatedTankMenuItemKey = GlobalTranslations.Get("TankMenuItem", "Tanque");
            TranslatedProductCompartmentMenuItemKey = GlobalTranslations.Get("ProductCompartmentMenuItem", "Compartimento de Producto");
            TranslatedDispensersMenuItemKey = GlobalTranslations.Get("DispensersMenuItem", "Dispensadores");
            TranslatedDispenserTypeMenuItemKey = GlobalTranslations.Get("DispenserTypeMenuItem", "Tipo de Dispensador");
            TranslatedHoseMenuItemKey = GlobalTranslations.Get("HoseMenuItem", "Manguera");
            TranslatedHoseHistoryMenuItemKey = GlobalTranslations.Get("HoseHistoryMenuItem", "Historial de Manguera");
            TranslatedProductMenuItemKey = GlobalTranslations.Get("ProductMenuItem", "Producto");
            TranslatedProductTypeMenuItemKey = GlobalTranslations.Get("ProductTypeMenuItem", "Tipo de Producto");
            TranslatedShoppingMenuItemKey = GlobalTranslations.Get("ShoppingMenuItem", "Compras");
            TranslatedEdsMenuItemKey = GlobalTranslations.Get("EdsMenuItem", "EDS");
            TranslatedExpenditureMenuItemKey = GlobalTranslations.Get("ExpenditureMenuItem", "Gastos");
            TranslatedIslanderMenuItemKey = GlobalTranslations.Get("IslanderMenuItem", "Isleño");
            TranslatedIslandMenuItemKey = GlobalTranslations.Get("IslandMenuItem", "Isla");
            TranslatedCategoryMenuItemKey = GlobalTranslations.Get("CategoryMenuItem", "Categoría");
            TranslatedTypeOfCollectionMenuItemKey = GlobalTranslations.Get("TypeOfCollectionMenuItem", "Tipo de Recolección");
            TranslatedInventoryMenuItemKey = GlobalTranslations.Get("InventoryMenuItem", "Inventario");


            // Re-initialize Categories after translations are loaded
            if (!IsIslander)
            {
                Categories = new ObservableCollection<CategoryModel>
                {
                    new(GlobalTranslations.Get("AdministrationCategory", "Administración"), new List<MenuItemModel>
                    {
                        new(GlobalTranslations.Get("CourtMenuItem", "Corte"), typeof(CourtPostView)),
                        new(GlobalTranslations.Get("BusinessMenuItem", "Negocio"), typeof(BusinessPostView)),
                        new(GlobalTranslations.Get("ProviderMenuItem", "Proveedor"), typeof(ProviderPostView))
                    }),
                    new(GlobalTranslations.Get("TanksAndCompartmentsCategory", "Tanques y Compartimentos"), new List<MenuItemModel>
                    {
                        new(GlobalTranslations.Get("CapacityMenuItem", "Capacidad"), typeof(CapacityPostView)),
                        new(GlobalTranslations.Get("CompartmentMenuItem", "Compartimento"), typeof(CompartimentPostView)),
                        new(GlobalTranslations.Get("CompartmentCapacityMenuItem", "Capacidad de Compartimento"), typeof(APP.Eds.UsesCases.CompartimentCapacity.CompartimentCapacityPostView)),
                        new(GlobalTranslations.Get("EdsTankMenuItem", "Tanque EDS"), typeof(EdsTankPostView)),
                        new(GlobalTranslations.Get("TankMenuItem", "Tanque"), typeof(TankPostView)),
                        new(GlobalTranslations.Get("ProductCompartmentMenuItem", "Compartimento de Producto"), typeof(APP.Eds.UsesCases.ProductCompartiment.ProductCompartimentPostView))
                    }),
                    new(GlobalTranslations.Get("DispensersAndHosesCategory", "Dispensadores y Mangueras"), new List<MenuItemModel>
                    {
                        new(GlobalTranslations.Get("DispensersMenuItem", "Dispensadores"), typeof(DispensersPostView)),
                        new(GlobalTranslations.Get("DispenserTypeMenuItem", "Tipo de Dispensador"), typeof(DispenserTypePostView)),
                        new(GlobalTranslations.Get("HoseMenuItem", "Manguera"), typeof(HosePostView)),
                        new(GlobalTranslations.Get("HoseHistoryMenuItem", "Historial de Manguera"), typeof(HoseHistoryPostView))
                    }),
                    new(GlobalTranslations.Get("ProductsAndShoppingCategory", "Productos y Compras"), new List<MenuItemModel>
                    {
                        new(GlobalTranslations.Get("ProductMenuItem", "Producto"), typeof(ProductPostView)),
                        new(GlobalTranslations.Get("ProductTypeMenuItem", "Tipo de Producto"), typeof(ProductTypePostView)),
                        new(GlobalTranslations.Get("ShoppingMenuItem", "Compras"), typeof(ShoppingPostView)),
                    }),
                    new(GlobalTranslations.Get("EdsAndOthersCategory", "EDS y Otros"), new List<MenuItemModel>
                    {
                        new(GlobalTranslations.Get("EdsMenuItem", "EDS"), typeof(EdsPostView)),
                        new(GlobalTranslations.Get("ExpenditureMenuItem", "Gastos"), typeof(ExpendituresPostView)),
                        new(GlobalTranslations.Get("IslanderMenuItem", "Isleño"), typeof(IslanderPostView)),
                        new(GlobalTranslations.Get("IslandMenuItem", "Isla"), typeof(IslandPostView)),
                        new(GlobalTranslations.Get("CategoryMenuItem", "Categoría"), typeof(CategoryPostView)),
                        new(GlobalTranslations.Get("TypeOfCollectionMenuItem", "Tipo de Recolección"), typeof(TypeOfCollectionPostView))
                    }),
                    new(GlobalTranslations.Get("InventoryCategory", "Inventario"),
                    [
                        new(GlobalTranslations.Get("InventoryMenuItem", "Inventario"), typeof(InventoryPostView)),
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

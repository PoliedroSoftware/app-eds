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
using System.ComponentModel;
using System.Windows.Input;
using APP.Eds.Services.Config;
using APP.Eds.Services.Translations;

namespace APP.Eds.Services.Navigation
{
    public class MainService : BindableObject, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler? PropertyChanged;

        public TranslationsService _translations = TranslationsService.GetTranslationServiceInstance();
        private ObservableCollection<CategoryModel> _categories;
        public ObservableCollection<CategoryModel> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged(nameof(Categories));
            }
        }
        public bool IsIslander => Preferences.Get("userRole", "") == "User";

        //Translations
        private String _management;
        public String Management
        {
            get => _management;
            set
            {
                _management = value;
                OnPropertyChanged(nameof(Management));
            }
        }
        private string _logOutText;
        public string LogOutText
        {
            get => _logOutText;
            set
            {
                _logOutText = value;
                OnPropertyChanged(nameof(LogOutText));
            }
        }
        public ICommand NavigateToCourtCommand { get; }

        // Nuevas variables de traducción
        private string _courtText;
        public string CourtText
        {
            get => _courtText;
            set
            {
                _courtText = value;
                OnPropertyChanged(nameof(CourtText));
            }
        }

        private string _businessText;
        public string BusinessText
        {
            get => _businessText;
            set
            {
                _businessText = value;
                OnPropertyChanged(nameof(BusinessText));
            }
        }

        private string _providerText;
        public string ProviderText
        {
            get => _providerText;
            set
            {
                _providerText = value;
                OnPropertyChanged(nameof(ProviderText));
            }
        }

        private string _tanksAndCompartmentsText;
        public string TanksAndCompartmentsText
        {
            get => _tanksAndCompartmentsText;
            set
            {
                _tanksAndCompartmentsText = value;
                OnPropertyChanged(nameof(TanksAndCompartmentsText));
            }
        }

        private string _capacityText;
        public string CapacityText
        {
            get => _capacityText;
            set
            {
                _capacityText = value;
                OnPropertyChanged(nameof(CapacityText));
            }
        }

        private string _compartmentText;
        public string CompartmentText
        {
            get => _compartmentText;
            set
            {
                _compartmentText = value;
                OnPropertyChanged(nameof(CompartmentText));
            }
        }

        private string _compartmentCapacityText;
        public string CompartmentCapacityText
        {
            get => _compartmentCapacityText;
            set
            {
                _compartmentCapacityText = value;
                OnPropertyChanged(nameof(CompartmentCapacityText));
            }
        }

        private string _edsTankText;
        public string EdsTankText
        {
            get => _edsTankText;
            set
            {
                _edsTankText = value;
                OnPropertyChanged(nameof(EdsTankText));
            }
        }

        private string _tankText;
        public string TankText
        {
            get => _tankText;
            set
            {
                _tankText = value;
                OnPropertyChanged(nameof(TankText));
            }
        }

        private string _productCompartmentText;
        public string ProductCompartmentText
        {
            get => _productCompartmentText;
            set
            {
                _productCompartmentText = value;
                OnPropertyChanged(nameof(ProductCompartmentText));
            }
        }

        private string _dispensersAndHosesText;
        public string DispensersAndHosesText
        {
            get => _dispensersAndHosesText;
            set
            {
                _dispensersAndHosesText = value;
                OnPropertyChanged(nameof(DispensersAndHosesText));
            }
        }

        private string _dispensersText;
        public string DispensersText
        {
            get => _dispensersText;
            set
            {
                _dispensersText = value;
                OnPropertyChanged(nameof(DispensersText));
            }
        }

        private string _dispenserTypeText;
        public string DispenserTypeText
        {
            get => _dispenserTypeText;
            set
            {
                _dispenserTypeText = value;
                OnPropertyChanged(nameof(DispenserTypeText));
            }
        }

        private string _hoseText;
        public string HoseText
        {
            get => _hoseText;
            set
            {
                _hoseText = value;
                OnPropertyChanged(nameof(HoseText));
            }
        }

        private string _hoseHistoryText;
        public string HoseHistoryText
        {
            get => _hoseHistoryText;
            set
            {
                _hoseHistoryText = value;
                OnPropertyChanged(nameof(HoseHistoryText));
            }
        }

        private string _productsAndPurchasesText;
        public string ProductsAndPurchasesText
        {
            get => _productsAndPurchasesText;
            set
            {
                _productsAndPurchasesText = value;
                OnPropertyChanged(nameof(ProductsAndPurchasesText));
            }
        }

        private string _productText;
        public string ProductText
        {
            get => _productText;
            set
            {
                _productText = value;
                OnPropertyChanged(nameof(ProductText));
            }
        }

        private string _productTypeText;
        public string ProductTypeText
        {
            get => _productTypeText;
            set
            {
                _productTypeText = value;
                OnPropertyChanged(nameof(ProductTypeText));
            }
        }

        private string _purchasesText;
        public string PurchasesText
        {
            get => _purchasesText;
            set
            {
                _purchasesText = value;
                OnPropertyChanged(nameof(PurchasesText));
            }
        }

        private string _edsAndOthersText;
        public string EdsAndOthersText
        {
            get => _edsAndOthersText;
            set
            {
                _edsAndOthersText = value;
                OnPropertyChanged(nameof(EdsAndOthersText));
            }
        }

        private string _registerEdsText;
        public string RegisterEdsText
        {
            get => _registerEdsText;
            set
            {
                _registerEdsText = value;
                OnPropertyChanged(nameof(RegisterEdsText));
            }
        }

        private string _expenseTypeText;
        public string ExpenseTypeText
        {
            get => _expenseTypeText;
            set
            {
                _expenseTypeText = value;
                OnPropertyChanged(nameof(ExpenseTypeText));
            }
        }

        private string _registerIslanderText;
        public string RegisterIslanderText
        {
            get => _registerIslanderText;
            set
            {
                _registerIslanderText = value;
                OnPropertyChanged(nameof(RegisterIslanderText));
            }
        }

        private string _islandText;
        public string IslandText
        {
            get => _islandText;
            set
            {
                _islandText = value;
                OnPropertyChanged(nameof(IslandText));
            }
        }

        private string _categoryText;
        public string CategoryText
        {
            get => _categoryText;
            set
            {
                _categoryText = value;
                OnPropertyChanged(nameof(CategoryText));
            }
        }

        private string _collectionTypeText;
        public string CollectionTypeText
        {
            get => _collectionTypeText;
            set
            {
                _collectionTypeText = value;
                OnPropertyChanged(nameof(CollectionTypeText));
            }
        }

        private string _inventoryText;
        public string InventoryText
        {
            get => _inventoryText;
            set
            {
                _inventoryText = value;
                OnPropertyChanged(nameof(InventoryText));
            }
        }

        public MainService()
        {
            NavigateToCourtCommand = new Command(async () =>
            {
                if (Application.Current?.MainPage is NavigationPage navPage)
                {
                    await navPage.PushAsync(new CourtPostView());
                }
            });

            Categories = new ObservableCollection<CategoryModel>();
            LoadTranslationsAsync();
        }
        public async Task InitializeAsync() {
            
            await LoadTranslationsAsync();

            var userRole = Preferences.Get("userRole", "");

            if (userRole == "Admin")
            {

                Categories = new ObservableCollection<CategoryModel>
                {
                new(Management, new List<MenuItemModel>
                {
                    new(CourtText, typeof(CourtPostView)),
                    new(BusinessText, typeof(BusinessPostView)),
                    new(ProviderText, typeof(ProviderPostView))
                }),
                new(TanksAndCompartmentsText, new List<MenuItemModel>
                {
                    new(CapacityText, typeof(CapacityPostView)),
                    new(CompartmentText, typeof(CompartimentPostView)),
                    new(CompartmentCapacityText, typeof(CompartimentCapacityPostView)),
                    new(EdsTankText, typeof(EdsTankPostView)),
                    new(TankText, typeof(TankPostView)),
                    new(ProductCompartmentText, typeof(ProductCompartimentPostView))
                }),
                new(DispensersAndHosesText, new List<MenuItemModel>
                {
                    new(DispensersText, typeof(DispensersPostView)),
                    new(DispenserTypeText, typeof(DispenserTypePostView)),
                    new(HoseText, typeof(HosePostView)),
                    new(HoseHistoryText, typeof(HoseHistoryPostView))
                }),
                new(ProductsAndPurchasesText, new List<MenuItemModel>
                {
                    new(ProductText, typeof(ProductPostView)),
                    new(ProductTypeText, typeof(ProductTypePostView)),
                    new(PurchasesText, typeof(ShoppingPostView)),
                }),
                new(EdsAndOthersText, new List<MenuItemModel>
                {
                    new(RegisterEdsText, typeof(EdsPostView)),
                    new(ExpenseTypeText, typeof(ExpendituresPostView)),
                    new(RegisterIslanderText, typeof(IslanderPostView)),
                     new(IslandText, typeof(IslandPostView)),
                    new(CategoryText, typeof(CategoryPostView)),
                    new(CollectionTypeText, typeof(TypeOfCollectionPostView))
                }),
                 new(InventoryText,
                [
                    new(InventoryText, typeof(InventoryPostView)),

                ]),
                };
            }
            else
            {
                Categories = new ObservableCollection<CategoryModel>();
            }
        }
        public async Task LoadTranslationsAsync()
        {
            var result = await _translations.GetTranslationsByLanguageAsync("es-CO");
            GlobalTranslations.SetTranslations(result ?? []);
            Management = GlobalTranslations.Get("Management");
            LogOutText = GlobalTranslations.Get("Logout");
            CourtText = GlobalTranslations.Get("Court");
            BusinessText = GlobalTranslations.Get("Business");
            ProviderText = GlobalTranslations.Get("Provider");
            TanksAndCompartmentsText = "Tanques y Compartimentos";
            CapacityText = GlobalTranslations.Get("Capacity");
            CompartmentText = GlobalTranslations.Get("Compartment");
            CompartmentCapacityText = GlobalTranslations.Get("CompartmentCapacity");
            EdsTankText = GlobalTranslations.Get("EdsTank");
            TankText = GlobalTranslations.Get("Tank");
            ProductCompartmentText = GlobalTranslations.Get("ProductCompartment");
            DispensersAndHosesText = "Dispensadores y Mangueras";
            DispensersText = GlobalTranslations.Get("Dispensers");
            DispenserTypeText = GlobalTranslations.Get("DispenserType");
            HoseText = GlobalTranslations.Get("Hose");
            HoseHistoryText = GlobalTranslations.Get("HoseHistory");
            ProductsAndPurchasesText = "Productos y Compras";
            ProductText = GlobalTranslations.Get("Product");
            ProductTypeText = GlobalTranslations.Get("ProductType");
            PurchasesText = GlobalTranslations.Get("Purchases");
            EdsAndOthersText = "EDS y Otros";
            RegisterEdsText = GlobalTranslations.Get("RegisterEds");
            ExpenseTypeText = GlobalTranslations.Get("ExpenseType");
            RegisterIslanderText = GlobalTranslations.Get("RegisterIslander");
            IslandText = GlobalTranslations.Get("Island");
            CategoryText = GlobalTranslations.Get("Category");
            CollectionTypeText = GlobalTranslations.Get("CollectionType");
            InventoryText = "Inventario";
            //Todas las demas variables de traducción que necesites
        }
        public class CategoryModel
        {
            public string Title { get; set; }
            public ICommand ShowPopupCommand { get; }

            public List<MenuItemModel> Items { get; set; }

            public CategoryModel(string title, List<MenuItemModel> items)
            {
                Title = title;
                Items = items;
                ShowPopupCommand = new Command(() =>
                {
                    var popup = new CategoryPopup(items, title);
                    Application.Current?.MainPage?.ShowPopup(popup);
                });
            }
        }

        public class MenuItemModel
        {
            public string Name { get; set; }
            public Type Page { get; set; }
            public Page PageInstance { get; set; }
            public ICommand NavigateCommand { get; }

            public MenuItemModel(string name, Type page)
            {
                Name = name;
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
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

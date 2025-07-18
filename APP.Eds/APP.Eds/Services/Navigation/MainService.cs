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
        public event PropertyChangedEventHandler? PropertyChanged;
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
           var userRole = Preferences.Get("userRole", "");

            NavigateToCourtCommand = new Command(async () =>
            {
                if (Application.Current?.MainPage is NavigationPage navPage)
                {
                    await navPage.PushAsync(new CourtPostView());
                }
            });

            

            if (userRole == "Admin")
            {

                Categories = new ObservableCollection<CategoryModel>
            {
                new(GlobalTranslations.Get("AdministrationCategoryKey"), new List<MenuItemModel>
                {
                    new(GlobalTranslations.Get("CourtMenuItemKey"), typeof(CourtPostView)),
                    new(GlobalTranslations.Get("BusinessMenuItemKey"), typeof(BusinessPostView)),
                    new(GlobalTranslations.Get("ProviderMenuItemKey"), typeof(ProviderPostView))
                }),
                new(GlobalTranslations.Get("TanksAndCompartmentsCategoryKey"), new List<MenuItemModel>
                {

                    new(GlobalTranslations.Get("CapacityMenuItemKey"), typeof(CapacityPostView)),
                    new(GlobalTranslations.Get("CompartmentMenuItemKey"), typeof(CompartimentPostView)),
                    new(GlobalTranslations.Get("CompartmentCapacityMenuItemKey"), typeof(CompartimentCapacityPostView)),
                    new(GlobalTranslations.Get("EdsTankMenuItemKey"), typeof(EdsTankPostView)),
                    new(GlobalTranslations.Get("TankMenuItemKey"), typeof(TankPostView)),
                    new(GlobalTranslations.Get("ProductCompartmentMenuItemKey"), typeof(ProductCompartimentPostView))
                
                }),
                new(GlobalTranslations.Get("DispensersAndHosesCategoryKey"), new List<MenuItemModel>
                {

                    new(GlobalTranslations.Get("DispensersMenuItemKey"), typeof(DispensersPostView)),
                    new(GlobalTranslations.Get("DispenserTypeMenuItemKey"), typeof(DispenserTypePostView)),
                    new(GlobalTranslations.Get("HoseMenuItemKey"), typeof(HosePostView)),
                    new(GlobalTranslations.Get("HoseHistoryMenuItemKey"), typeof(HoseHistoryPostView))
                
                }),
                new(GlobalTranslations.Get("ProductsAndShoppingCategoryKey"), new List<MenuItemModel>
                {

                    new(GlobalTranslations.Get("ProductMenuItemKey"), typeof(ProductPostView)),
                    new(GlobalTranslations.Get("ProductTypeMenuItemKey"), typeof(ProductTypePostView)),
                    new(GlobalTranslations.Get("ShoppingMenuItemKey"), typeof(ShoppingPostView)),


                }),
                new(GlobalTranslations.Get("EdsAndOthersCategoryKey"), new List<MenuItemModel>
                {

                    new(GlobalTranslations.Get("EdsMenuItemKey"), typeof(EdsPostView)),
                    new(GlobalTranslations.Get("ExpenditureMenuItemKey"), typeof(ExpendituresPostView)),
                    new(GlobalTranslations.Get("IslanderMenuItemKey"), typeof(IslanderPostView)),
                     new(GlobalTranslations.Get("IslandMenuItemKey"), typeof(IslandPostView)),
                    new(GlobalTranslations.Get("CategoryMenuItemKey"), typeof(CategoryPostView)),
                    new(GlobalTranslations.Get("TypeOfCollectionMenuItemKey"), typeof(TypeOfCollectionPostView))

                }),
                 new(GlobalTranslations.Get("InventoryCategoryKey"),
                [
                    new(GlobalTranslations.Get("InventoryMenuItemKey"), typeof(InventoryPostView)),
                 
                ]),
            };
            }
            else
            {
                Categories = new ObservableCollection<CategoryModel>();
            }
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
    }
}

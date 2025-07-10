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

namespace APP.Eds.Services.Navigation
{
    public class MainService : BindableObject
    {
        public ObservableCollection<CategoryModel> Categories { get; set; }

        public MainService()
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

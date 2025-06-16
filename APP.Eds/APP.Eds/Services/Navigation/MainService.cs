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

namespace APP.Eds.Services.Navigation
{
    public class MainService : BindableObject
    {
        public ObservableCollection<CategoryModel> Categories { get; set; }

        public MainService()
        {
            Categories = new ObservableCollection<CategoryModel>
            {
                new("Administración", new List<MenuItemModel>
                {
                    new("Court", typeof(CourtPostView)),
                    new("Business", typeof(BusinessPostView)),
                    new("Provider", typeof(ProviderPostView))
                }),
                new("Tanques y Compartimentos", new List<MenuItemModel>
                {
                    new("Capacity", typeof(CapacityPostView)),
                    new("Compartiment", typeof(CompartimentPostView)),
                    new("CompartimentCapacity", typeof(CompartimentCapacityPostView)),
                    new("EdsTank", typeof(EdsTankPostView)),
                    new("Tank", typeof(TankPostView)),
                    new("ProductCompartiment", typeof(ProductCompartimentPostView))
                }),
                new("Dispensadores y Mangueras", new List<MenuItemModel>
                {
                    new("Dispensers", typeof(DispensersPostView)),
                    new("DispenserType", typeof(DispenserTypePostView)),
                    new("Hose", typeof(HosePostView)),
                    new("HoseHistory", typeof(HoseHistoryPostView))
                }),
                new("Productos y Compras", new List<MenuItemModel>
                {
                    new("Product", typeof(ProductPostView)),
                    new("Product Type", typeof(ProductTypePostView)),
                    new("Shopping", typeof(ShoppingPostView)),
                    new("ShoppingProduct", typeof(ShoppingProductPostView))
                }),
                new("EDS y Otros", new List<MenuItemModel>
                {
                    new("Eds", typeof(EdsPostView)),
                    new("Expenditure", typeof(ExpendituresPostView)),
                    new("Islander", typeof(IslanderPostView)),
                     new("Island", typeof(IslandPostView)),
                    new("Category", typeof(CategoryPostView)),
                    new("Type of Collection", typeof(TypeOfCollectionPostView))
                }),
                 new("Inventario",
                [
                    new("Inventario", typeof(InventoryPostView)),
                 
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
            public ICommand NavigateCommand { get; }

            public MenuItemModel(string name, Type page)
            {
                Name = name;
                Page = page;
                NavigateCommand = new Command(async () =>
                {
                    if (Application.Current?.MainPage is NavigationPage navPage)
                    {
                        var pageInstance = (Page)Activator.CreateInstance(Page);
                        await navPage.PushAsync(pageInstance);
                    }
                });

            }
        }
    }
}

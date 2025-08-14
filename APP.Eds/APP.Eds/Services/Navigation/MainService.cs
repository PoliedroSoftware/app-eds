using APP.Eds.UsesCases.Business;
using APP.Eds.UsesCases.Capacity;
using APP.Eds.UsesCases.Category;
using APP.Eds.UsesCases.CompartimentCapacity;
using APP.Eds.UsesCases.Court;
using APP.Eds.UsesCases.Dispensers;
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
using APP.Eds.UsesCases.Provider;
using APP.Eds.UsesCases.Shopping;
using APP.Eds.UsesCases.Tank;
using APP.Eds.UsesCases.TypeOfCollection;
using APP.Eds.UsesCases.Wizard;
using APP.Eds.Views.Popups;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace APP.Eds.Services.Navigation
{
    public class MainService : BindableObject
    {
        public ObservableCollection<CategoryModel> Categories { get; set; }
        public bool IsIslander => Preferences.Get("userRole", "") == "User";

        public ICommand NavigateToCourtCommand { get; }
        public ICommand NavigateToWizardCommand { get; }
        
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

            NavigateToWizardCommand = new Command(async () =>
            {
                if (Application.Current?.MainPage is NavigationPage navPage)
                {
                    await navPage.PushAsync(new SetupWizardView());
                }
            });

            // Comando directo para Inventario
            var NavigateToInventoryCommand = new Command(async () =>
            {
                if (Application.Current?.MainPage is NavigationPage navPage)
                {
                    await navPage.PushAsync(new InventoryPostView());
                }
            });

            if (userRole == "Admin")
            {
                Categories = new ObservableCollection<CategoryModel>
            {
                // Categoría especial para Configuración Inicial que navega directamente
                new("🧙‍♂️ Configuración Inicial", NavigateToWizardCommand, isDirectNavigation: true),
                new("Administración", new List<MenuItemModel>
                {
                    new("Corte", typeof(CourtPostView)),
                    new("Negocio", typeof(BusinessPostView)),
                    new("Proveedor", typeof(ProviderPostView))
                }),
                new("Tanques y Compartimentos", new List<MenuItemModel>
                {
                    new("Capacidad", typeof(CapacityPostView)),
                    new("Compartimento", typeof(CompartimentPostView)),
                    new("Capacidad Del Compartimento", typeof(CompartimentCapacityPostView)),
                    new("Tanque EDS", typeof(EdsTankPostView)),
                    new("Tanque", typeof(TankPostView)),
                    new("Compartimento Del Producto", typeof(ProductCompartimentPostView))
                }),
                new("Dispensadores y Mangueras", new List<MenuItemModel>
                {
                    new("Dispensadores", typeof(DispensersPostView)),
                    new("Tipo De Dispensador", typeof(DispenserTypePostView)),
                    new("Manguera", typeof(HosePostView)),
                    new("Historial De La Manguera", typeof(HoseHistoryPostView))
                }),
                new("Productos y Compras", new List<MenuItemModel>
                {
                    new("Producto", typeof(ProductPostView)),
                    new("Tipo De P1roducto", typeof(ProductTypePostView)),
                    new("Compras", typeof(ShoppingPostView)),
                }),
                new("EDS y Otros", new List<MenuItemModel>
                {
                    new("Registre Una EDS", typeof(EdsPostView)),
                    new("Tipo De Gastos", typeof(ExpendituresPostView)),
                    new("Registre Un Islero", typeof(IslanderPostView)),
                     new("Isla", typeof(IslandPostView)),
                    new("Categoría", typeof(CategoryPostView)),
                    new("Tipo De Colección", typeof(TypeOfCollectionPostView))
                }),
                 new("Inventario",
                [
                    new("Inventario", typeof(InventoryPostView)),
                 
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
            public bool IsDirectNavigation { get; set; }

            public CategoryModel(string title, List<MenuItemModel> items)
            {
                Title = title;
                Items = items;
                IsDirectNavigation = false;
                ShowPopupCommand = new Command(() =>
                {
                    var popup = new CategoryPopup(items, title);
                    Application.Current?.MainPage?.ShowPopup(popup);
                });
            }

            public CategoryModel(string title, ICommand directCommand, bool isDirectNavigation = false)
            {
                Title = title;
                Items = new List<MenuItemModel>();
                IsDirectNavigation = isDirectNavigation;
                ShowPopupCommand = directCommand;
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

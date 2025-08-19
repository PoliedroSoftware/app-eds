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

            // Comando directo para navegar al wizard sin popup
            NavigateToWizardCommand = new Command(async () =>
            {
                if (Application.Current?.MainPage is NavigationPage navPage)
                {
                    await navPage.PushAsync(new SetupWizardView());
                }
            });

            if (userRole == "Admin")
            {
                Categories = new ObservableCollection<CategoryModel>
                {
                // Categoría especial para Configuración Inicial que navega directamente
                new("🧙‍♂️ Configuración Inicial", NavigateToWizardCommand, isDirectNavigation: true),
                    {
                        new("Corte", typeof(CourtPostView))
                    }),
                    new("Administración", new List<MenuItemModel>
                    {
                        new("Negocio", typeof(BusinessPostView)),
                        new("Registre una EDS", typeof(EdsPostView))
                    }),
                    new("Dispensadores y mangueras", new List<MenuItemModel>
                    {
                        new("Dispensadores", typeof(DispensersPostView)),
                        new("Manguera", typeof(HosePostView)),
                        new("Historial de la manguera", typeof(HoseHistoryPostView))
                    }),
                    new("Compras y productos", new List<MenuItemModel>
                    {
                        new("Agregar productos", typeof(ProductPostView)),
                        new("Compras", typeof(ShoppingPostView)),
                        new("Proveedor", typeof(ProviderPostView)),
                        new("Categoría", typeof(CategoryPostView))
                    }),
                    new("Tanques y compartimentos", new List<MenuItemModel>
                    {
                        new("Capacidad", typeof(CapacityPostView)),
                        new("Capacidad del compartimento", typeof(CompartimentCapacityPostView)),
                        new("Tanque EDS", typeof(EdsTankPostView)),
                        new("Tanque", typeof(TankPostView)),
                        new("Compartimento del producto", typeof(ProductCompartimentPostView))
                    }),
                    new("EDS y otros", new List<MenuItemModel>
                    {
                        new("Tipos de G", typeof(ExpendituresPostView)),
                        new("Registre un Islero", typeof(IslanderPostView)),
                        new("Isla", typeof(IslandPostView)),
                        new("Tipo de colección", typeof(TypeOfCollectionPostView))
                    }),
                    new("Inventario", new List<MenuItemModel>
                    {
                        new("Inventario", typeof(InventoryPostView))
                    })
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

            // Constructor para categorías normales que muestran popup
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

            // Constructor para categorías que navegan directamente (como Configuración Inicial)
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

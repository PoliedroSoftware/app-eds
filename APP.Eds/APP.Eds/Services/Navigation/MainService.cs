using APP.Eds.UsesCases.Business;
using APP.Eds.UsesCases.Capacity;
using APP.Eds.UsesCases.Category;
using APP.Eds.UsesCases.Compartiment;
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
using APP.Eds.UsesCases.Phone;
using APP.Eds.UsesCases.PowerBI;
using APP.Eds.UsesCases.Product;
using APP.Eds.UsesCases.ProductCompartiment;
using APP.Eds.UsesCases.Provider;
using APP.Eds.UsesCases.Shopping;
using APP.Eds.UsesCases.StrongBox;
using APP.Eds.UsesCases.Tank;
using APP.Eds.UsesCases.TypeOfCollection;
using APP.Eds.UsesCases.Wizard;
using APP.Eds.Views.Popups;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

            // ✅ NUEVO: Comando para mostrar popup de Punto de Venta
            var ShowPointOfSaleMenuCommand = new Command(async () =>
                   {
                       var popup = new PointOfSaleMenuPopup();
                       await MainThread.InvokeOnMainThreadAsync(async () =>
                            {
                           try
                           {
                               await Application.Current.MainPage.ShowPopupAsync(popup);
                           }
                           catch (ObjectDisposedException ex)
                           {
                               Debug.WriteLine($"PointOfSaleMenuPopup was disposed: {ex.Message}");
                           }
                           catch (Exception ex)
                           {
                               Debug.WriteLine($"Error showing PointOfSaleMenuPopup: {ex.Message}");
                           }
                       });
                   });

            // Comando directo para Power BI Dashboard
            var NavigateToPowerBICommand = new Command(async () =>
           {
               if (Application.Current?.MainPage is NavigationPage navPage)
               {
                   await navPage.PushAsync(new PowerBIView());
               }
           });

            if (userRole == "Admin")
            {
                Categories = new ObservableCollection<CategoryModel>
                {
       // ✅ PRIMERO: Administración
         new("Administración", "⚙️", new List<MenuItemModel>
           {
          new("Corte", typeof(CourtPostView), "💰"),
new("Negocio", typeof(BusinessPostView), "🏢"),
        new("EDS", typeof(EdsPostView), "🏪"),
          new("Caja Fuerte", typeof(StrongBoxView), "💼"),
        new("Telefonos", typeof(PhoneRegistrationView), "📱")
         }),
   new("Configuración Inicial", "🧙‍♂️", NavigateToWizardCommand, isDirectNavigation: true),
         new("Punto de Venta", "💳", ShowPointOfSaleMenuCommand, isDirectNavigation: true),
        new("Power BI Dashboard", "📊", NavigateToPowerBICommand, isDirectNavigation: true),
        new("Dispensadores y mangueras", "⛽", new List<MenuItemModel>
        {
       new("Dispensadores", typeof(DispensersPostView), "⛽"),
      new("Manguera", typeof(HosePostView), "🔧"),
 new("Historial de la manguera", typeof(HoseHistoryPostView), "📋")
     }),
new("Compras y productos", "🛒", new List<MenuItemModel>
  {
         new("Productos", typeof(ProductPostView), "➕"),
       new("Compras", typeof(ShoppingPostView), "🛒"),
       new("Proveedor", typeof(ProviderPostView), "🏭"),
        new("Categoría", typeof(CategoryPostView), "📂")
         }),
        new("Tanques y compartimentos", "🛢️", new List<MenuItemModel>
    {
    new("Capacidad", typeof(CapacityPostView), "📏"),
        new("Capacidad del compartimento", typeof(CompartimentCapacityPostView), "📐"),
         new("Tanque EDS", typeof(EdsTankPostView), "🛢️"),
         new("Tanque", typeof(TankPostView), "🗂️"),
          new("Compartimento", typeof(CompartimentPostView), "📦"),
           new("Compartimento del producto", typeof(ProductCompartimentPostView), "🔗")
    }),
       new("EDS y otros", "🏪", new List<MenuItemModel>
    {
        new("Gasto", typeof(ExpendituresPostView), "💳"),
         new("Islero", typeof(IslanderPostView), "👤"),
           new("Isla", typeof(IslandPostView), "🏝️"),
 new("Formas de Pago", typeof(TypeOfCollectionPostView), "📝")
         }),
     new("Inventario", "📦", NavigateToInventoryCommand, isDirectNavigation: true)
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
            public string Icon { get; set; }
            public ICommand ShowPopupCommand { get; }
            public List<MenuItemModel> Items { get; set; }
            public bool IsDirectNavigation { get; set; }

            public CategoryModel(string title, string icon, List<MenuItemModel> items)
            {
                Title = title;
                Icon = icon;
                Items = items;
                IsDirectNavigation = false;
                ShowPopupCommand = new Command(async () =>
                {
                    // ✅ CORREGIDO: Crear nueva instancia local en cada ejecución
                    var popup = new CategoryPopup(items, title);
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        try
                        {
                            await Application.Current.MainPage.ShowPopupAsync(popup);
                        }
                        catch (ObjectDisposedException ex)
                        {
                            Debug.WriteLine($"CategoryPopup was disposed: {ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error showing CategoryPopup: {ex.Message}");
                        }
                    });
                });
            }

            public CategoryModel(string title, string icon, ICommand directCommand, bool isDirectNavigation = false)
            {
                Title = title;
                Icon = icon;
                Items = new List<MenuItemModel>();
                IsDirectNavigation = isDirectNavigation;
                ShowPopupCommand = directCommand;
            }
        }

        public class MenuItemModel
        {
            public string Title { get; set; }
            public string Name => Title;
            public string Icon { get; set; }
            public Type PageType { get; set; }
            public ICommand NavigateCommand { get; }

            public MenuItemModel(string title, Type pageType, string icon = "📄")
            {
                Title = title;
                PageType = pageType;
                Icon = icon;
                NavigateCommand = new Command(async () =>
                {
                    try
                    {
                        if (Application.Current?.MainPage is NavigationPage navPage)
                        {
                            // Crear instancia de la página
                            var pageInstance = Activator.CreateInstance(pageType) as Page;
                            if (pageInstance != null)
                            {
                                await navPage.PushAsync(pageInstance);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await Application.Current?.MainPage?.DisplayAlert("Error",
                            $"No se pudo navegar a {title}: {ex.Message}", "OK");
                    }
                });
            }
        }
    }
}

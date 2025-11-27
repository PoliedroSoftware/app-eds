namespace APP.Eds.Services.Navigation
{
    using APP.Eds.UsesCases.Billing;
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
    using APP.Eds.UsesCases.IoT;
    using APP.Eds.UsesCases.Island;
    using APP.Eds.UsesCases.Islander;
    using APP.Eds.UsesCases.Phone;
    using APP.Eds.UsesCases.PointOfSale;
    using APP.Eds.UsesCases.PowerBI;
    using APP.Eds.UsesCases.Product;
    using APP.Eds.UsesCases.ProductCompartiment;
    using APP.Eds.UsesCases.Provider;
    using APP.Eds.UsesCases.Shopping;
    using APP.Eds.UsesCases.StrongBox;
    using APP.Eds.UsesCases.Tank;
    using APP.Eds.UsesCases.TransferValidation;
    using APP.Eds.UsesCases.TypeOfCollection;
    using APP.Eds.UsesCases.Wizard;
    using APP.Eds.Views.Popups;
    using CommunityToolkit.Maui.Views;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Windows.Input;

    /// <summary>
    /// Defines the <see cref="MainService" />
    /// </summary>
    public class MainService : BindableObject
    {
        /// <summary>
        /// Gets or sets the Categories
        /// </summary>
        public ObservableCollection<CategoryModel> Categories { get; set; }

        /// <summary>
        /// Gets a value indicating whether IsIslander
        /// </summary>
        public bool IsIslander => Preferences.Get("userRole", "") == "User";

        /// <summary>
        /// Gets the NavigateToCourtCommand
        /// </summary>
        public ICommand NavigateToCourtCommand { get; }

        /// <summary>
        /// Gets the NavigateToWizardCommand
        /// </summary>
        public ICommand NavigateToWizardCommand { get; }

        /// <summary>
        /// Gets the application version string
        /// </summary>
        public string AppVersion
        {
            get
            {
                try
                {
                    var version = AppInfo.Current.VersionString;
                    var build = AppInfo.Current.BuildString;
                    return $"v{version} ({build})";
                }
                catch
                {
                    return "v1.0.1";
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainService"/> class.
        /// </summary>
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

            // Comando directo para Punto de Venta
            var NavigateToPointOfSaleCommand = new Command(async () =>
                   {
                       if (Application.Current?.MainPage is NavigationPage navPage)
                       {
                           await navPage.PushAsync(new PointOfSaleView());
                       }
                   });

            // Comando directo para Power BI Dashboard
            var NavigateToPowerBICommand = new Command(async () =>
           {
               if (Application.Current?.MainPage is NavigationPage navPage)
               {
                   await navPage.PushAsync(new PowerBIView());
               }
           });

            // Comando directo para Historial de Facturas
            var NavigateToInvoiceHistoryCommand = new Command(async () =>
                 {
                     if (Application.Current?.MainPage is NavigationPage navPage)
                     {
                         await navPage.PushAsync(new InvoiceHistoryView());
                     }
                 });

            // Comando directo para Listado de Pagos QR
            var NavigateToQRPaymentsCommand = new Command(async () =>
            {
                if (Application.Current?.MainPage is NavigationPage navPage)
                {
                    await navPage.PushAsync(new QRPaymentsListView());
                }
            });

            // ✨ NUEVO: Comando directo para Control IoT
            var NavigateToIoTControlCommand = new Command(async () =>
            {
                if (Application.Current?.MainPage is NavigationPage navPage)
                {
                    await navPage.PushAsync(new ValveControlPage());
                }
            });

            if (userRole == "Admin")
            {
                Categories = new ObservableCollection<CategoryModel>
                {
                    // PRIMERO: Administración
                    new("Administración", "⚙️", new List<MenuItemModel>
                    {
                        new("Corte", typeof(CourtPostView), "💰"),
                        new("Negocio", typeof(BusinessPostView), "🏢"),
                        new("EDS", typeof(EdsPostView), "🏪"),
                        new("Caja Fuerte", typeof(StrongBoxView), "💼"),
                        new("Telefonos", typeof(PhoneRegistrationView), "📱")
                    }),
                    new("Configuración Inicial", "🧙‍♂️", NavigateToWizardCommand, isDirectNavigation: true),
                    new("Punto de Venta", "💳", new List<MenuItemModel>
                    {
                        new("Facturación Electrónica", typeof(PointOfSaleView), "📝"),
                        new("Historial de Facturas", typeof(InvoiceHistoryView), "📄")
                    }),
                    new("Listado de Pagos QR", "💳", NavigateToQRPaymentsCommand, isDirectNavigation: true),
                    new("Power BI Dashboard", "📊", NavigateToPowerBICommand, isDirectNavigation: true),
                    // ✨ NUEVO: Sección IoT con acceso directo al Control de Válvula
                    new("Control IoT", "🎛️", NavigateToIoTControlCommand, isDirectNavigation: true),
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
                        new("Listado de Compras", typeof(ShoppingListView), "📋"),
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

        /// <summary>
        /// Defines the <see cref="CategoryModel" />
        /// </summary>
        public class CategoryModel
        {
            /// <summary>
            /// Gets or sets the Title
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// Gets or sets the Icon
            /// </summary>
            public string Icon { get; set; }

            /// <summary>
            /// Gets the ShowPopupCommand
            /// </summary>
            public ICommand ShowPopupCommand { get; }

            /// <summary>
            /// Gets or sets the Items
            /// </summary>
            public List<MenuItemModel> Items { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether IsDirectNavigation
            /// </summary>
            public bool IsDirectNavigation { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="CategoryModel"/> class.
            /// </summary>
            /// <param name="title">The title<see cref="string"/></param>
            /// <param name="icon">The icon<see cref="string"/></param>
            /// <param name="items">The items<see cref="List{MenuItemModel}"/></param>
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

            /// <summary>
            /// Initializes a new instance of the <see cref="CategoryModel"/> class.
            /// </summary>
            /// <param name="title">The title<see cref="string"/></param>
            /// <param name="icon">The icon<see cref="string"/></param>
            /// <param name="directCommand">The directCommand<see cref="ICommand"/></param>
            /// <param name="isDirectNavigation">The isDirectNavigation<see cref="bool"/></param>
            public CategoryModel(string title, string icon, ICommand directCommand, bool isDirectNavigation = false)
            {
                Title = title;
                Icon = icon;
                Items = new List<MenuItemModel>();
                IsDirectNavigation = isDirectNavigation;
                ShowPopupCommand = directCommand;
            }
        }

        /// <summary>
        /// Defines the <see cref="MenuItemModel" />
        /// </summary>
        public class MenuItemModel
        {
            /// <summary>
            /// Gets or sets the Title
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// Gets the Name
            /// </summary>
            public string Name => Title;

            /// <summary>
            /// Gets or sets the Icon
            /// </summary>
            public string Icon { get; set; }

            /// <summary>
            /// Gets or sets the PageType
            /// </summary>
            public Type PageType { get; set; }

            /// <summary>
            /// Gets the NavigateCommand
            /// </summary>
            public ICommand NavigateCommand { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="MenuItemModel"/> class.
            /// </summary>
            /// <param name="title">The title<see cref="string"/></param>
            /// <param name="pageType">The pageType<see cref="Type"/></param>
            /// <param name="icon">The icon<see cref="string"/></param>
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

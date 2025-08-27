using APP.Eds.Models.Expenditures;
using System.ComponentModel;
using System.Text.Json;
using System.Text;
using System.Windows.Input;
using APP.Eds.Services.Config;
using APP.Eds.Helpers;
using System.Net.Http.Headers;
using System.Collections.ObjectModel;

namespace APP.Eds.Services.Expenditures
{
    public class ExpenseItem
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }

    public class ExpendituresService : INotifyPropertyChanged
    {
        private string? _authToken;
        public event PropertyChangedEventHandler? PropertyChanged;
        
        // Collections
        public ObservableCollection<string> ExpenseCategories { get; set; } = new();
        public ObservableCollection<ExpenseItem> ExpensesList { get; set; } = new();

        private ExpendituresRequest Request { get; set; }
        private ExpendituresModel _expenditures;

        public ExpendituresModel Expenditures
        {
            get => _expenditures;
            set
            {
                _expenditures = value;
                OnPropertyChanged(nameof(Expenditures));
            }
        }

        // Form Properties
        private string _selectedCategory;
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));
            }
        }

        private string _amount;
        public string Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged(nameof(Amount));
                UpdateStatistics();
            }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        private DateTime _expenseDate = DateTime.Now;
        public DateTime ExpenseDate
        {
            get => _expenseDate;
            set
            {
                _expenseDate = value;
                OnPropertyChanged(nameof(ExpenseDate));
            }
        }

        // Statistics Properties
        private decimal _todaysExpenses;
        public decimal TodaysExpenses
        {
            get => _todaysExpenses;
            set
            {
                _todaysExpenses = value;
                OnPropertyChanged(nameof(TodaysExpenses));
            }
        }

        private decimal _weeksExpenses;
        public decimal WeeksExpenses
        {
            get => _weeksExpenses;
            set
            {
                _weeksExpenses = value;
                OnPropertyChanged(nameof(WeeksExpenses));
            }
        }

        private decimal _monthsExpenses;
        public decimal MonthsExpenses
        {
            get => _monthsExpenses;
            set
            {
                _monthsExpenses = value;
                OnPropertyChanged(nameof(MonthsExpenses));
            }
        }

        // Filter Properties
        private Color _filterButtonColor1 = Color.FromArgb("#D32F2F");
        public Color FilterButtonColor1
        {
            get => _filterButtonColor1;
            set
            {
                _filterButtonColor1 = value;
                OnPropertyChanged(nameof(FilterButtonColor1));
            }
        }

        private Color _filterButtonColor2 = Color.FromArgb("#9E9E9E");
        public Color FilterButtonColor2
        {
            get => _filterButtonColor2;
            set
            {
                _filterButtonColor2 = value;
                OnPropertyChanged(nameof(FilterButtonColor2));
            }
        }

        private Color _filterButtonColor3 = Color.FromArgb("#9E9E9E");
        public Color FilterButtonColor3
        {
            get => _filterButtonColor3;
            set
            {
                _filterButtonColor3 = value;
                OnPropertyChanged(nameof(FilterButtonColor3));
            }
        }

        // Commands
        public ICommand GetByIdExpendituresDataCommand { get; private set; }
        public ICommand SaveExpendituresDataCommand { get; private set; }
        public ICommand FilterTodayCommand { get; private set; }
        public ICommand FilterWeekCommand { get; private set; }
        public ICommand FilterMonthCommand { get; private set; }
        public ICommand DeleteExpenseCommand { get; private set; }

        public ExpendituresService()
        {
            InitializeCommands();
            InitializeCategories();
            _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
        }

        private void InitializeCommands()
        {
            GetByIdExpendituresDataCommand = new Command<int>(async (expendituresId) => await GetByIdExpendituresDataAsync(expendituresId));
            SaveExpendituresDataCommand = new Command(async () => await SaveExpendituresDataAsync());
            FilterTodayCommand = new Command(() => FilterExpenses("today"));
            FilterWeekCommand = new Command(() => FilterExpenses("week"));
            FilterMonthCommand = new Command(() => FilterExpenses("month"));
            DeleteExpenseCommand = new Command<ExpenseItem>(async (expense) => await DeleteExpenseAsync(expense));
        }

        private void InitializeCategories()
        {
            ExpenseCategories.Clear();
            ExpenseCategories.Add("Mantenimiento");
            ExpenseCategories.Add("Combustible");
            ExpenseCategories.Add("Servicios Públicos");
            ExpenseCategories.Add("Personal");
            ExpenseCategories.Add("Seguros");
            ExpenseCategories.Add("Impuestos");
            ExpenseCategories.Add("Suministros");
            ExpenseCategories.Add("Reparaciones");
            ExpenseCategories.Add("Transporte");
            ExpenseCategories.Add("Otros");
        }

        public async Task InitializeAsync()
        {
            await LoadExpensesAsync();
            UpdateStatistics();
            FilterExpenses("today"); // Default filter
        }

        private async Task LoadExpensesAsync()
        {
            try
            {
                // Simulate loading expenses from API
                // In real implementation, you would call your API here
                var sampleExpenses = new List<ExpenseItem>
                {
                    new ExpenseItem { Id = 1, Category = "Mantenimiento", Amount = 50000, Description = "Cambio de aceite dispensador", Date = DateTime.Now.AddDays(-1) },
                    new ExpenseItem { Id = 2, Category = "Servicios Públicos", Amount = 120000, Description = "Factura de electricidad", Date = DateTime.Now.AddDays(-3) },
                    new ExpenseItem { Id = 3, Category = "Suministros", Amount = 30000, Description = "Papel térmico para impresoras", Date = DateTime.Now.AddDays(-5) },
                    new ExpenseItem { Id = 4, Category = "Personal", Amount = 25000, Description = "Bonificación empleado", Date = DateTime.Now.AddDays(-7) },
                    new ExpenseItem { Id = 5, Category = "Combustible", Amount = 80000, Description = "Diesel para generador", Date = DateTime.Now.AddDays(-10) }
                };

                ExpensesList.Clear();
                foreach (var expense in sampleExpenses.OrderByDescending(x => x.Date))
                {
                    ExpensesList.Add(expense);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error cargando gastos: {ex.Message}", "OK");
            }
        }

        private void FilterExpenses(string period)
        {
            // Reset filter button colors
            FilterButtonColor1 = Color.FromArgb("#9E9E9E");
            FilterButtonColor2 = Color.FromArgb("#9E9E9E");
            FilterButtonColor3 = Color.FromArgb("#9E9E9E");

            // Set active button color and filter logic
            switch (period)
            {
                case "today":
                    FilterButtonColor1 = Color.FromArgb("#D32F2F");
                    // Filter today's expenses (implementation would filter the collection)
                    break;
                case "week":
                    FilterButtonColor2 = Color.FromArgb("#D32F2F");
                    // Filter this week's expenses
                    break;
                case "month":
                    FilterButtonColor3 = Color.FromArgb("#D32F2F");
                    // Filter this month's expenses
                    break;
            }
        }

        private async Task DeleteExpenseAsync(ExpenseItem expense)
        {
            try
            {
                var result = await Application.Current.MainPage.DisplayAlert(
                    "Confirmar eliminación",
                    $"¿Está seguro de eliminar el gasto '{expense.Description}'?",
                    "Eliminar",
                    "Cancelar");

                if (result)
                {
                    ExpensesList.Remove(expense);
                    UpdateStatistics();
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Gasto eliminado correctamente", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error eliminando gasto: {ex.Message}", "OK");
            }
        }

        private void UpdateStatistics()
        {
            var today = DateTime.Now.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            TodaysExpenses = ExpensesList.Where(x => x.Date.Date == today).Sum(x => x.Amount);
            WeeksExpenses = ExpensesList.Where(x => x.Date.Date >= weekStart).Sum(x => x.Amount);
            MonthsExpenses = ExpensesList.Where(x => x.Date.Date >= monthStart).Sum(x => x.Amount);
        }

        public async Task GetByIdExpendituresDataAsync(int expendituresId)
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return;
            }
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/expenditures/{expendituresId}");

                Expenditures = JsonSerializer.Deserialize<ExpendituresModel>(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar el dato: {ex.Message}", "OK");
            }
        }

        public async Task SaveExpendituresDataAsync()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return;
            }

            try
            {
                // Create comprehensive expense description including category
                var fullDescription = $"[{SelectedCategory}] {Description} - Monto: ${Amount}";

                Expenditures = new ExpendituresModel
                {
                    Description = fullDescription
                };

                Request = new ExpendituresRequest
                {
                    Request = Expenditures
                };

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var json = JsonSerializer.Serialize(Request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/expenditures", content);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Gasto registrado correctamente", "OK");
                    
                    // Add to local list
                    if (decimal.TryParse(Amount, out decimal amount))
                    {
                        var newExpense = new ExpenseItem
                        {
                            Id = ExpensesList.Count + 1,
                            Category = SelectedCategory,
                            Amount = amount,
                            Description = Description,
                            Date = ExpenseDate
                        };
                        
                        ExpensesList.Insert(0, newExpense); // Add at the beginning
                        UpdateStatistics();
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    
                    string userFriendlyError = "No se pudo registrar el gasto. Por favor, intente de nuevo más tarde.";
                    if (!string.IsNullOrEmpty(error))
                    {
                        if (error.Contains("validation error", StringComparison.OrdinalIgnoreCase))
                        {
                            userFriendlyError = "Error de validación en los datos del gasto.";
                        }
                        else if (error.Contains("server error", StringComparison.OrdinalIgnoreCase))
                        {
                            userFriendlyError = "Error del servidor. Por favor, intente de nuevo más tarde.";
                        }
                    }
                    await Application.Current.MainPage.DisplayAlert("Error", userFriendlyError, "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error al registrar el gasto: {ex.Message}", "OK");
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

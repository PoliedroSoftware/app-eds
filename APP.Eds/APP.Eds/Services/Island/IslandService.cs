using APP.Eds.Helpers;
using APP.Eds.Models.Island;
using APP.Eds.Services.Config;
using APP.Eds.Services.Eds;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows.Input;

namespace APP.Eds.Services.Island;

// Model class for editable pending islands
public class EditablePendingIsland : INotifyPropertyChanged
{
    private string _name;
    private int _number;

    public string Name 
    { 
        get => _name; 
        set 
        { 
            _name = value; 
            OnPropertyChanged(nameof(Name)); 
        } 
    }

    public int Number
    {
        get => _number;
        set
        {
            _number = value;
            Name = $"Isla {value}";
            OnPropertyChanged(nameof(Number));
        }
    }
    public string SelectedEdsName { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

// Model class for editable existing islands
public class EditableIsland : INotifyPropertyChanged
{
    private int _id;
    private string _description;
    private bool _isEditing;

    public int Id
    {
        get => _id;
        set
        {
            _id = value;
            OnPropertyChanged(nameof(Id));
        }
    }

    public string Description
    {
        get => _description;
        set
        {
            _description = value;
            OnPropertyChanged(nameof(Description));
        }
    }

    public bool IsEditing
    {
        get => _isEditing;
        set
        {
            _isEditing = value;
            OnPropertyChanged(nameof(IsEditing));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class IslandService : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<EditableIsland> IslandList { get; set; } = [];
    public ObservableCollection<EditablePendingIsland> PendingIslands { get; set; } = [];

    private string? _authToken;
    private IslandRequest Request { get; set; }
    private IslandModel _island;

    public IslandModel Island
    {
        get => _island;
        set
        {
            _island = value;
            OnPropertyChanged(nameof(Island));
        }
    }

    private int _numberOfIslands = 0;
    public int NumberOfIslands
    {
        get => _numberOfIslands;
        set
        {
            _numberOfIslands = value;
            OnPropertyChanged(nameof(NumberOfIslands));
            GeneratePendingIslands();
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

    // Property for the islands counter
    public int TotalIslandsCreated => IslandList.Count;

    // Property for displaying counter text with emoji and formatting
    public string IslandCounterText => $"📊 Total de Islas:";

    public ICommand GetByIdIslandDataCommand { get; }
    public ICommand SaveIslandDataCommand { get; }
    public ICommand SaveAllIslandsCommand { get; }
    public ICommand RemoveIslandCommand { get; }
    public ICommand ClearAllIslandsCommand { get; }
    public ICommand AddSingleIslandCommand { get; }
    public ICommand EditIslandCommand { get; }
    public ICommand SaveEditIslandCommand { get; }
    public ICommand CancelEditIslandCommand { get; }
    public ICommand DeleteIslandCommand { get; }

    public IslandService()
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);

        GetByIdIslandDataCommand = new Command<int>(async (islandId) => await GetByIdIslandDataAsync(islandId));
        SaveIslandDataCommand = new Command(async () => await SaveIslandDataAsync());
        SaveAllIslandsCommand = new Command(async () => await SaveAllIslandsAsync());
        RemoveIslandCommand = new Command<EditablePendingIsland>(RemoveIsland);
        ClearAllIslandsCommand = new Command(ClearAllIslands);
        AddSingleIslandCommand = new Command(AddSingleIsland);
        EditIslandCommand = new Command<EditableIsland>(EditIsland);
        SaveEditIslandCommand = new Command<EditableIsland>(async (island) => await SaveEditIslandAsync(island));
        CancelEditIslandCommand = new Command<EditableIsland>(CancelEditIsland);
        DeleteIslandCommand = new Command<EditableIsland>(async (island) => await DeleteIslandAsync(island));

        // Load existing islands first, then generate pending islands
        _ = Task.Run(async () => 
        {
            await GetIslandAsync();
            GeneratePendingIslands();
        });
    }

    private void EditIsland(EditableIsland island)
    {
        // Cancel any other editing
        foreach (var item in IslandList)
        {
            if (item != island)
                item.IsEditing = false;
        }
        
        island.IsEditing = true;
    }

    private void CancelEditIsland(EditableIsland island)
    {
        island.IsEditing = false;
        // Reload original data to cancel changes
        _ = Task.Run(async () => await GetIslandAsync());
    }

    private async Task SaveEditIslandAsync(EditableIsland island)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(island.Description))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "La descripción no puede estar vacía.", "OK");
                return;
            }

            var updateModel = new IslandModel
            {
                Description = island.Description
            };

            var updateRequest = new IslandRequest
            {
                Request = updateModel
            };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var json = JsonSerializer.Serialize(updateRequest, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync($"{Configuration.BaseUrl}/api/v1/island/{island.Id}", content);

            if (response.IsSuccessStatusCode)
            {
                await Application.Current.MainPage.DisplayAlert("Éxito", "Isla actualizada correctamente", "OK");
                island.IsEditing = false;
                await GetIslandAsync();
                GeneratePendingIslands();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo actualizar la isla: {response.StatusCode}\n{error}", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al actualizar la isla: {ex.Message}", "OK");
        }
    }

    private async Task DeleteIslandAsync(EditableIsland island)
    {
        try
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirmar", 
                $"¿Desea eliminar la isla '{island.Description}'?", "Sí", "No");
            
            if (!confirm)
                return;

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.DeleteAsync($"{Configuration.BaseUrl}/api/v1/island/{island.Id}");

            if (response.IsSuccessStatusCode)
            {
                await Application.Current.MainPage.DisplayAlert("Éxito", "Isla eliminada correctamente", "OK");
                await GetIslandAsync();
                GeneratePendingIslands();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo eliminar la isla: {response.StatusCode}\n{error}", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al eliminar la isla: {ex.Message}", "OK");
        }
    }

    private void GeneratePendingIslands()
    {
        PendingIslands.Clear();
        
        if (NumberOfIslands <= 0)
        {
            return;
        }

        int lastNumber = GetLastIslandNumber();
        
        for (int i = 1; i <= NumberOfIslands; i++)
        {
            int newNumber = lastNumber + i;
            PendingIslands.Add(new EditablePendingIsland 
            { 
                Number = newNumber 
            });
        }
    }

    private int GetLastIslandNumber()
    {
        if (!IslandList.Any())
        {
            return 0;
        }

        int maxNumber = 0;
        foreach (var island in IslandList)
        {
            if (island.Description.StartsWith("Isla "))
            {
                string numberPart = island.Description.Substring(5);
                if (int.TryParse(numberPart, out int number))
                {
                    maxNumber = Math.Max(maxNumber, number);
                }
            }
        }
        
        return maxNumber;
    }

    private void RemoveIsland(EditablePendingIsland island)
    {
        if (PendingIslands.Contains(island))
        {
            PendingIslands.Remove(island);
        }
    }

    private void AddSingleIsland()
    {
        int nextNumber = GetLastIslandNumber();
        if (PendingIslands.Any())
        {
            nextNumber = Math.Max(nextNumber, PendingIslands.Max(p => p.Number));
        }
        nextNumber++;

        PendingIslands.Add(new EditablePendingIsland 
        { 
            Number = nextNumber 
        });
    }

    private void ClearAllIslands()
    {
        PendingIslands.Clear();
    }

    private void ClearForm()
    {
        NumberOfIslands = 0;
        Description = string.Empty;
        PendingIslands.Clear();
    }

    private void UpdateCounterProperties()
    {
        OnPropertyChanged(nameof(TotalIslandsCreated));
        OnPropertyChanged(nameof(IslandCounterText));
    }

    public async Task SaveAllIslandsAsync()
    {
        try
        {
            if (!PendingIslands.Any())
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No hay islas para crear.", "OK");
                return;
            }

            // Check for duplicate numbers
            var duplicates = PendingIslands.GroupBy(x => x.Number)
                                          .Where(g => g.Count() > 1)
                                          .Select(x => x.Key);

            if (duplicates.Any())
            {
                await Application.Current.MainPage.DisplayAlert("Error", 
                    $"Hay números duplicados: {string.Join(", ", duplicates)}. Por favor, corrija antes de continuar.", "OK");
                return;
            }

            // Check for conflicts with existing islands
            var existingNumbers = IslandList.Where(i => i.Description.StartsWith("Isla "))
                                           .Select(i => 
                                           {
                                               if (int.TryParse(i.Description.Substring(5), out int num))
                                                   return num;
                                               return -1;
                                           })
                                           .Where(n => n > 0);

            var conflicts = PendingIslands.Where(p => existingNumbers.Contains(p.Number))
                                         .Select(p => p.Number);

            if (conflicts.Any())
            {
                bool confirm = await Application.Current.MainPage.DisplayAlert("Conflicto", 
                    $"Las siguientes islas ya existen: {string.Join(", ", conflicts.Select(n => $"Isla {n}"))}. ¿Desea continuar de todas formas?", 
                    "Sí", "No");
                
                if (!confirm)
                    return;
            }

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            int successCount = 0;
            int totalCount = PendingIslands.Count;

            foreach (var pendingIsland in PendingIslands.ToList())
            {
                try
                {
                    Island = new IslandModel
                    {
                        Description = pendingIsland.Name
                    };

                    Request = new IslandRequest
                    {
                        Request = Island
                    };

                    var json = JsonSerializer.Serialize(Request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/island", content);

                    if (response.IsSuccessStatusCode)
                    {
                        successCount++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creando isla {pendingIsland.Name}: {ex.Message}");
                }
            }

            if (successCount == totalCount)
            {
                await Application.Current.MainPage.DisplayAlert("Éxito", $"Se crearon {successCount} islas correctamente", "OK");
                ClearForm();
                await GetIslandAsync();
                GeneratePendingIslands();
            }
            else if (successCount > 0)
            {
                await Application.Current.MainPage.DisplayAlert("Parcial", $"Se crearon {successCount} de {totalCount} islas", "OK");
                await GetIslandAsync();
                GeneratePendingIslands();
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudieron crear las islas", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al crear las islas: {ex.Message}", "OK");
        }
    }

    public async Task GetByIdIslandDataAsync(int islandId)
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/island/{islandId}");
            Island = JsonSerializer.Deserialize<IslandModel>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar el dato: {ex.Message}", "OK");
        }
    }

    public async Task SaveIslandDataAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Description))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "El campo 'Description' no puede estar vacío.", "OK");
                return;
            }

            Island = new IslandModel
            {
                Description = Description
            };

            Request = new IslandRequest
            {
                Request = Island
            };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var json = JsonSerializer.Serialize(Request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/island", content);

            if (response.IsSuccessStatusCode)
            {
                await Application.Current.MainPage.DisplayAlert("Éxito", "Datos enviados correctamente", "OK");
                Description = string.Empty;
                await GetIslandAsync();
                GeneratePendingIslands();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo enviar el dato: {response.StatusCode}\n{error}", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al enviar los datos: {ex.Message}", "OK");
        }
    }

    public async Task GetIslandAsync()
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/island");
            var islands = JsonSerializer.Deserialize<IslandApiResponse>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            IslandList.Clear();
            if (islands?.Data != null)
            {
                foreach (var island in islands.Data)
                {
                    IslandList.Add(new EditableIsland
                    {
                        Id = island.Idisland,
                        Description = island.Description,
                        IsEditing = false
                    });
                }
            }
            
            UpdateCounterProperties();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
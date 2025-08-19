using APP.Eds.Helpers;
using APP.Eds.Models.Island;
using APP.Eds.Services.Config;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows.Input;

namespace APP.Eds.Services.Island;

public class IslandService : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<IslandResponse> IslandList { get; set; } = [];

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

    private int _numberOfIslands = 1;
    public int NumberOfIslands
    {
        get => _numberOfIslands;
        set
        {
            _numberOfIslands = value;
            OnPropertyChanged(nameof(NumberOfIslands));
        }
    }


    public ICommand GetByIdIslandDataCommand { get; }
    public ICommand SaveIslandDataCommand { get; }

    public IslandService()
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);

        GetByIdIslandDataCommand = new Command<int>(async (islandId) => await GetByIdIslandDataAsync(islandId));
        SaveIslandDataCommand = new Command(async () => await SaveIslandDataAsync());
        
        // Load existing islands
        _ = Task.Run(async () => await GetIslandAsync());
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
            if (NumberOfIslands <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "El número de islas debe ser mayor a 0.", "OK");
                return;
            }

            if (NumberOfIslands > 50)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "El número máximo de islas permitido es 50.", "OK");
                return;
            }

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            int successCount = 0;
            int failCount = 0;
            List<string> errors = new List<string>();

            // Get current islands to determine the starting number
            await GetIslandAsync();
            int startNumber = IslandList.Count + 1;

            for (int i = 0; i < NumberOfIslands; i++)
            {
                try
                {
                    var islandDescription = $"isla {startNumber + i}";
                    
                    Island = new IslandModel
                    {
                        Description = islandDescription
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
                    else
                    {
                        failCount++;
                        var error = await response.Content.ReadAsStringAsync();
                        errors.Add($"Error creando {islandDescription}: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    failCount++;
                    errors.Add($"Error creando isla {startNumber + i}: {ex.Message}");
                }
            }

            // Show results
            if (successCount > 0 && failCount == 0)
            {
                await Application.Current.MainPage.DisplayAlert("Éxito", $"Se crearon {successCount} islas correctamente", "OK");
            }
            else if (successCount > 0 && failCount > 0)
            {
                var errorDetails = string.Join("\n", errors.Take(3));
                await Application.Current.MainPage.DisplayAlert("Parcial", $"Se crearon {successCount} islas. {failCount} fallaron.\n{errorDetails}", "OK");
            }
            else
            {
                var errorDetails = string.Join("\n", errors.Take(3));
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo crear ninguna isla.\n{errorDetails}", "OK");
            }

            // Refresh the island list
            await GetIslandAsync();
            
            // Reset the form
            NumberOfIslands = 1;
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al crear las islas: {ex.Message}", "OK");
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
            foreach (var island in islands.Data)
            {
                IslandList.Add(island);
            }
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
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace APP.Eds.Services.Copilot
{
    public class CopilotService : INotifyPropertyChanged
    {
        private readonly string _baseUrl = "https://mg5tj7bmve.execute-api.us-east-2.amazonaws.com/eds/api/v1/poli";
        private string _response = string.Empty;
        private bool _isLoading;

        public string Response
        {
            get => _response;
            set
            {
                _response = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public async Task<string> GetHelpAsync(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                return "Por favor, ingrese una pregunta válida.";

            IsLoading = true;
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("X-Environment", "poli");
                
                var requestData = new
                {
                    userMessage = userMessage
                };

                var json = JsonSerializer.Serialize(requestData, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await httpClient.PostAsync(_baseUrl, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Response = responseContent;
                    return responseContent;
                }
                else
                {
                    var errorMessage = $"Error al obtener ayuda: {response.StatusCode}";
                    Response = errorMessage;
                    return errorMessage;
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error de conexión: {ex.Message}";
                Response = errorMessage;
                return errorMessage;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
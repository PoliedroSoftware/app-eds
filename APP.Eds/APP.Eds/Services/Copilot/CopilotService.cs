using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace APP.Eds.Services.Copilot
{
    public class CopilotService : INotifyPropertyChanged
    {
        private readonly string _baseUrl = "https://mg5tj7bmve.execute-api.us-east-2.amazonaws.com/eds/api/v1/poli";
        private string _response = string.Empty;
        private string _markdownResponse = string.Empty;
        private bool _isLoading;

        public string Response
        {
            get => _response;
            set
            {
                _response = value;
                MarkdownResponse = FormatToMarkdown(value);
                OnPropertyChanged();
            }
        }

        public string MarkdownResponse
        {
            get => _markdownResponse;
            set
            {
                _markdownResponse = value;
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

        public async Task<string> GetHelpAsync(string userMessage, string currentStepContext = "")
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                return "Por favor, ingrese una pregunta válida.";

            IsLoading = true;
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("X-Environment", "poli");
                
                // Agregar contexto del paso actual si está disponible
                string enhancedMessage = userMessage;
                if (!string.IsNullOrEmpty(currentStepContext))
                {
                    enhancedMessage = $"Contexto: Estoy en el paso '{currentStepContext}' del asistente de configuración. Pregunta: {userMessage}";
                }
                
                var requestData = new
                {
                    userMessage = enhancedMessage
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
                    var errorMessage = $"❌ Error al obtener ayuda: {response.StatusCode}";
                    Response = errorMessage;
                    return errorMessage;
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"❌ Error de conexión: {ex.Message}";
                Response = errorMessage;
                return errorMessage;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private string FormatToMarkdown(string rawResponse)
        {
            if (string.IsNullOrEmpty(rawResponse))
                return string.Empty;

            try
            {
                // Intentar extraer texto legible primero
                string cleanText = ExtractReadableText(rawResponse);
                
                // Convertir a formato Markdown
                return ConvertToMarkdown(cleanText);
            }
            catch
            {
                // Si falla el formateo, devolver el texto original limpiado
                return CleanText(rawResponse);
            }
        }

        private string ExtractReadableText(string text)
        {
            // Remover caracteres de escape comunes
            text = text.Replace("\\n", "\n")
                      .Replace("\\r", "\r")
                      .Replace("\\t", "\t")
                      .Replace("\\\"", "\"");

            // Buscar texto entre comillas que podría ser la respuesta real
            var matches = Regex.Matches(text, @"""([^""\\]*(\\.[^""\\]*)*)""");
            if (matches.Count > 0)
            {
                // Tomar la cadena más larga que probablemente sea la respuesta
                var longestMatch = matches.Cast<Match>()
                    .OrderByDescending(m => m.Groups[1].Value.Length)
                    .FirstOrDefault();
                
                if (longestMatch != null && longestMatch.Groups[1].Value.Length > 20)
                {
                    return longestMatch.Groups[1].Value;
                }
            }

            return CleanText(text);
        }

        private string ConvertToMarkdown(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Convertir saltos de línea dobles en párrafos
            text = Regex.Replace(text, @"\n\s*\n", "\n\n");
            
            // Detectar y formatear títulos (texto seguido de dos puntos al final de línea)
            text = Regex.Replace(text, @"^([^:\n]+:)\s*$", "## $1", RegexOptions.Multiline);
            
            // Agregar formato de lista si detectamos elementos numerados o con viñetas
            text = Regex.Replace(text, @"^(\d+\.\s)", "**$1**", RegexOptions.Multiline);
            text = Regex.Replace(text, @"^(•\s|[*-]\s)", "- ", RegexOptions.Multiline);
            
            // Resaltar textos entre comillas como código
            text = Regex.Replace(text, @"'([^']+)'", "`$1`");
            
            // Resaltar palabras importantes en negrita
            text = Regex.Replace(text, @"\b(EDS|tanque|dispensador|islero|negocio|configuración|paso)\b", "**$1**", RegexOptions.IgnoreCase);
            
            return text.Trim();
        }

        private string CleanText(string text)
        {
            // Limpiar caracteres especiales y formatear para legibilidad
            text = Regex.Replace(text, @"[{}[\]""]", "");
            text = Regex.Replace(text, @"\\[nrt]", " ");
            text = Regex.Replace(text, @"\s+", " ");
            return text.Trim();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
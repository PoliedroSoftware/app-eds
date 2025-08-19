using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using APP.Eds.Services.Wizard;

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
                
                // Obtener el contexto completo del modelo de negocio
                string businessModelContext = GetBusinessModelContext();
                
                // Obtener el contexto actual del wizard si está disponible
                string wizardContext = await GetCurrentWizardContextAsync();
                
                // Construir mensaje enriquecido con todos los contextos
                string enhancedMessage = BuildEnhancedMessage(userMessage, currentStepContext, businessModelContext, wizardContext);
                
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

        private string GetBusinessModelContext()
        {
            return @"
CONTEXTO DEL MODELO DE NEGOCIO EDS (Estaciones de Servicio):

## Reglas de Negocio y Dependencias:

### 1. ESTRUCTURA JERÁRQUICA OBLIGATORIA:
- **NEGOCIO** (1 mínimo) → **EDS** (1+ por negocio) → **OPERACIONES**

### 2. INFRAESTRUCTURA FÍSICA (por EDS):
- **TANQUES**: Mínimo 1 por EDS
- **COMPARTIMIENTOS**: Mínimo 1 por tanque (pueden ser varios)
- **ISLAS**: Mínimo 1 por EDS (pueden ser varias)
- **DISPENSADORES**: Mínimo 1 por isla (pueden ser varios por isla)
- **MANGUERAS**: Mínimo 1 por dispensador (pueden ser varias por dispensador)

### 3. RECURSOS HUMANOS:
- **ISLEROS**: Mínimo 1 por EDS (operarios que atienden las islas)

### 4. CATÁLOGO Y CADENA DE SUMINISTRO:
- **PRODUCTOS**: Mínimo 1 por EDS (combustibles/lubricantes asociados a mangueras y compartimientos)
- **PROVEEDORES**: Mínimo 1 por EDS (necesarios para registrar compras)

### 5. FLUJO DE CONFIGURACIÓN RECOMENDADO:
1. Negocio → 2. EDS → 3. Islas → 4. Tanques → 5. Compartimientos → 
6. Dispensadores → 7. Mangueras → 8. Productos → 9. Isleros → 10. Proveedores

### 6. RELACIONES CRÍTICAS:
- Las **mangueras** se conectan a **productos** y **compartimientos**
- Los **productos** se almacenan en **compartimientos** específicos
- Los **dispensadores** pertenecen a **islas** específicas
- Todo debe estar asociado a una **EDS** específica

### 7. VALIDACIONES DE OPERACIÓN:
- No se puede operar sin al menos: 1 negocio, 1 EDS, 1 isla, 1 tanque, 1 compartimiento, 1 dispensador, 1 manguera, 1 producto, 1 islero, 1 proveedor
- Cada elemento debe estar correctamente relacionado con su elemento padre

Este contexto te permite entender qué necesita el usuario para tener un sistema EDS funcional.
";
        }

        private async Task<string> GetCurrentWizardContextAsync()
        {
            try
            {
                var wizardService = new WizardService();
                
                var completedSteps = wizardService.Steps.Where(s => s.IsCompleted).Select(s => s.Title).ToList();
                var currentStep = wizardService.CurrentStep?.Title ?? "No definido";
                var pendingSteps = wizardService.Steps.Where(s => !s.IsCompleted && !s.IsActive).Select(s => s.Title).ToList();
                
                var context = $@"
## ESTADO ACTUAL DEL ASISTENTE DE CONFIGURACIÓN:

### Pasos Completados ✅:
{(completedSteps.Any() ? string.Join(", ", completedSteps) : "Ninguno")}

### Paso Actual 🔄:
{currentStep}

### Pasos Pendientes ⏳:
{(pendingSteps.Any() ? string.Join(", ", pendingSteps) : "Ninguno")}

### Progreso: {wizardService.GetCompletionPercentage():F0}% completado
";
                
                return context;
            }
            catch (Exception ex)
            {
                return $"## ESTADO DEL WIZARD: Error al obtener estado - {ex.Message}";
            }
        }

        private string BuildEnhancedMessage(string userMessage, string currentStepContext, string businessModelContext, string wizardContext)
        {
            var messageBuilder = new StringBuilder();
            
            // Agregar contexto del modelo de negocio
            messageBuilder.AppendLine(businessModelContext);
            
            // Agregar contexto del wizard actual
            messageBuilder.AppendLine(wizardContext);
            
            // Agregar contexto del paso actual si está disponible
            if (!string.IsNullOrEmpty(currentStepContext))
            {
                messageBuilder.AppendLine($"## PASO ACTUAL: {currentStepContext}");
            }
            
            messageBuilder.AppendLine("---");
            messageBuilder.AppendLine("## PREGUNTA DEL USUARIO:");
            messageBuilder.AppendLine(userMessage);
            
            messageBuilder.AppendLine();
            messageBuilder.AppendLine("## INSTRUCCIONES PARA LA RESPUESTA:");
            messageBuilder.AppendLine("- Proporciona ayuda específica basada en el modelo de negocio EDS");
            messageBuilder.AppendLine("- Indica qué pasos están completados y cuáles faltan");
            messageBuilder.AppendLine("- Explica las dependencias entre componentes si es relevante");
            messageBuilder.AppendLine("- Sugiere el siguiente paso lógico en la configuración");
            messageBuilder.AppendLine("- Usa formato Markdown para estructurar la respuesta");
            
            return messageBuilder.ToString();
        }

        private string FormatToMarkdown(string rawResponse)
        {
            if (string.IsNullOrEmpty(rawResponse))
                return string.Empty;

            try
            {
                string cleanText = ExtractReadableText(rawResponse);
                
                return ConvertToMarkdown(cleanText);
            }
            catch
            {
                return CleanText(rawResponse);
            }
        }

        private string ExtractReadableText(string text)
        {
            text = text.Replace("\\n", "\n")
                      .Replace("\\r", "\r")
                      .Replace("\\t", "\t")
                      .Replace("\\\"", "\"");

            var matches = Regex.Matches(text, @"""([^""\\]*(\\.[^""\\]*)*)""");
            if (matches.Count > 0)
            {
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

            text = Regex.Replace(text, @"\n\s*\n", "\n\n");
            
            text = Regex.Replace(text, @"^([^:\n]+:)\s*$", "## $1", RegexOptions.Multiline);
            
            text = Regex.Replace(text, @"^(\d+\.\s)", "**$1**", RegexOptions.Multiline);
            text = Regex.Replace(text, @"^(•\s|[*-]\s)", "- ", RegexOptions.Multiline);
            
            text = Regex.Replace(text, @"'([^']+)'", "`$1`");
            
            text = Regex.Replace(text, @"\b(EDS|tanque|dispensador|islero|negocio|configuración|paso|manguera|compartimiento|isla|producto|proveedor)\b", "**$1**", RegexOptions.IgnoreCase);
            
            return text.Trim();
        }

        private string CleanText(string text)
        {
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
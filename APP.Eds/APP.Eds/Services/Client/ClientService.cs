using APP.Eds.Helpers;
using APP.Eds.Models.Client;
using APP.Eds.Models.Rut;
using APP.Eds.Services.Config;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace APP.Eds.Services.Client;

/// <summary>
/// Service for managing client operations (Natural and Legal persons)
/// </summary>
public class ClientService
{
    private readonly string? _authToken;

    public ClientService()
    {
        _authToken = TokenHelper.LoadToken();
    }

    /// <summary>
    /// Creates a natural person client from RUT data
    /// </summary>
    public async Task<ClientNaturalModel?> CreateNaturalClientFromRutAsync(RutParseResponse rutData)
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            System.Diagnostics.Debug.WriteLine("? No authentication token available");
            return null;
        }

        try
        {
            // Map RUT data to Natural Client model
            var naturalClient = new ClientNaturalModel
            {
                DocumentNumber = rutData.DocumentNumber,
                DocumentTypeId = MapDocumentTypeToId(rutData.DocumentType),
                Email = rutData.Email,
                Name = rutData.FullName.FirstName,
                MiddleName = rutData.FullName.MiddleNames,
                LastName = rutData.FullName.LastName,
                SecondSurname = rutData.FullName.SecondLastName
            };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("X-Environment", "clients");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            var json = JsonSerializer.Serialize(naturalClient, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            System.Diagnostics.Debug.WriteLine($"?? Creating natural client: {naturalClient.FullName}");
            
            var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/client/natural", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var createdClient = JsonSerializer.Deserialize<ClientNaturalModel>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                System.Diagnostics.Debug.WriteLine($"? Natural client created successfully: ID {createdClient?.Id}");
                return createdClient;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"? Failed to create natural client: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Error: {errorContent}");
                return null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Error creating natural client: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Creates a legal person client from RUT data
    /// </summary>
    public async Task<ClientLegalModel?> CreateLegalClientFromRutAsync(RutParseResponse rutData)
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            System.Diagnostics.Debug.WriteLine("? No authentication token available");
            return null;
        }

        try
        {
            // Extract verification digit from NIT
            int verificationDigit = 0;
            if (!string.IsNullOrEmpty(rutData.Dv) && int.TryParse(rutData.Dv, out int dv))
            {
                verificationDigit = dv;
            }

            // Determine tax responsibilities
            bool isVatResponsible = rutData.Responsibilities?.Any(r => 
                r.Code == "01" || r.Description.Contains("IVA", StringComparison.OrdinalIgnoreCase)) ?? false;
            
            bool isWithholdingAgent = rutData.Responsibilities?.Any(r => 
                r.Code == "07" || r.Description.Contains("Retención", StringComparison.OrdinalIgnoreCase)) ?? false;

            bool isSimpleTaxRegime = rutData.Responsibilities?.Any(r => 
                r.Code == "49" || r.Description.Contains("Simple", StringComparison.OrdinalIgnoreCase)) ?? false;

            // Map RUT data to Legal Client model
            var legalClient = new ClientLegalModel
            {
                Name = rutData.FullName.Display,
                DocumentNumber = rutData.DocumentNumber,
                DocumentTypeId = MapDocumentTypeToId(rutData.DocumentType),
                VerificationDigit = verificationDigit,
                Email = rutData.Email,
                VatResponsibleParty = isVatResponsible,
                WithholdingAgent = isWithholdingAgent,
                SimpleTaxRegime = isSimpleTaxRegime,
                LargeTaxpayer = false, // Not typically in RUT
                SelfRetainer = false   // Not typically in RUT
            };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("X-Environment", "clients");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            var json = JsonSerializer.Serialize(legalClient, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            System.Diagnostics.Debug.WriteLine($"?? Creating legal client: {legalClient.Name}");
            
            var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/client/legal", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var createdClient = JsonSerializer.Deserialize<ClientLegalModel>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                System.Diagnostics.Debug.WriteLine($"? Legal client created successfully: ID {createdClient?.Id}");
                return createdClient;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"? Failed to create legal client: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Error: {errorContent}");
                return null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Error creating legal client: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Maps document type string to document type ID
    /// </summary>
    private int MapDocumentTypeToId(string documentType)
    {
        return documentType?.ToLowerInvariant() switch
        {
            var dt when dt.Contains("cédula") || dt.Contains("cedula") => 1, // Cédula de Ciudadanía
            var dt when dt.Contains("nit") => 2, // NIT
            var dt when dt.Contains("pasaporte") => 3, // Pasaporte
            var dt when dt.Contains("extranjería") => 4, // Cédula de Extranjería
            var dt when dt.Contains("tarjeta de identidad") => 5, // Tarjeta de Identidad
            _ => 1 // Default to Cédula
        };
    }

    /// <summary>
    /// Checks if a client already exists by document number
    /// </summary>
    public async Task<(bool Exists, ClientLegalModel? Client)> CheckClientExistsAsync(string documentNumber)
    {
        if (string.IsNullOrEmpty(_authToken) || string.IsNullOrWhiteSpace(documentNumber))
        {
            return (false, null);
        }

        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("X-Environment", "clients");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            // Check in legal clients
            var legalUrl = $"{Configuration.BaseUrl}/api/v1/client/legal";
            var legalResponse = await httpClient.GetStringAsync(legalUrl);
            var legalClients = JsonSerializer.Deserialize<ClientLegalResponse>(legalResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var existingLegal = legalClients?.Data?.FirstOrDefault(c => 
                c.DocumentNumber?.Trim().Equals(documentNumber, StringComparison.OrdinalIgnoreCase) == true);

            if (existingLegal != null)
            {
                return (true, existingLegal);
            }

            // Check in natural clients
            var naturalUrl = $"{Configuration.BaseUrl}/api/v1/client/natural";
            var naturalResponse = await httpClient.GetStringAsync(naturalUrl);
            var naturalClients = JsonSerializer.Deserialize<ClientNaturalResponse>(naturalResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var existingNatural = naturalClients?.Data?.FirstOrDefault(c => 
                c.DocumentNumber?.Trim().Equals(documentNumber, StringComparison.OrdinalIgnoreCase) == true);

            if (existingNatural != null)
            {
                // Convert to ClientLegalModel for compatibility
                var convertedClient = new ClientLegalModel
                {
                    Id = existingNatural.Id,
                    Name = existingNatural.FullName,
                    DocumentNumber = existingNatural.DocumentNumber,
                    DocumentTypeId = existingNatural.DocumentTypeId,
                    Email = existingNatural.Email
                };
                return (true, convertedClient);
            }

            return (false, null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Error checking client existence: {ex.Message}");
            return (false, null);
        }
    }
}

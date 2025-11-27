using APP.Eds.Models.Rut;
using APP.Eds.Services.Config;
using System.Net.Http.Headers;
using System.Text.Json;

namespace APP.Eds.Services.Rut;

/// <summary>
/// Service to parse RUT (Registro Único Tributario) from PDF files
/// Uses the extractor-pdf API to extract tax information
/// </summary>
public class RutParserService
{
    private readonly string _baseUrl = "https://mg5tj7bmve.execute-api.us-east-2.amazonaws.com/eds/api/v1/rut/parse";
    private readonly string? _authToken;

    public RutParserService(string? authToken)
    {
        _authToken = authToken;
    }

    /// <summary>
    /// Parses a RUT PDF file and extracts tax information
    /// </summary>
    /// <param name="filePath">Path to the PDF file</param>
    /// <returns>Parsed RUT data or null if parsing fails</returns>
    public async Task<RutParseResponse?> ParseRutFromPdfAsync(string filePath)
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            System.Diagnostics.Debug.WriteLine("? No authentication token available");
            return null;
        }

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            System.Diagnostics.Debug.WriteLine($"? Invalid file path: {filePath}");
            return null;
        }

        try
        {
            System.Diagnostics.Debug.WriteLine($"?? Parsing RUT from file: {filePath}");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("X-Environment", "extractor-pdf");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            // Read the file bytes
            byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
            string fileName = Path.GetFileName(filePath);

            // Create multipart form data
            using var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            content.Add(fileContent, "file", fileName);

            System.Diagnostics.Debug.WriteLine($"?? Sending PDF to RUT parser API ({fileBytes.Length} bytes)");

            // Send request
            var response = await httpClient.PostAsync(_baseUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"? RUT parsing successful");
                System.Diagnostics.Debug.WriteLine($"Response: {responseContent}");

                var rutData = JsonSerializer.Deserialize<RutParseResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (rutData != null)
                {
                    System.Diagnostics.Debug.WriteLine($"? RUT Data parsed successfully:");
                    System.Diagnostics.Debug.WriteLine($"   - NIT: {rutData.Nit}");
                    System.Diagnostics.Debug.WriteLine($"   - Name: {rutData.FullName.Display}");
                    System.Diagnostics.Debug.WriteLine($"   - Email: {rutData.Email}");
                    System.Diagnostics.Debug.WriteLine($"   - Document: {rutData.DocumentNumber}");
                }

                return rutData;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"? RUT parsing failed: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Error: {errorContent}");
                return null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Error parsing RUT: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// Parses a RUT PDF from a byte array
    /// </summary>
    /// <param name="fileBytes">PDF file bytes</param>
    /// <param name="fileName">Name of the file</param>
    /// <returns>Parsed RUT data or null if parsing fails</returns>
    public async Task<RutParseResponse?> ParseRutFromBytesAsync(byte[] fileBytes, string fileName)
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            System.Diagnostics.Debug.WriteLine("? No authentication token available");
            return null;
        }

        if (fileBytes == null || fileBytes.Length == 0)
        {
            System.Diagnostics.Debug.WriteLine("? Invalid file bytes");
            return null;
        }

        try
        {
            System.Diagnostics.Debug.WriteLine($"?? Parsing RUT from bytes: {fileName}");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("X-Environment", "extractor-pdf");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            // Create multipart form data
            using var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            content.Add(fileContent, "file", fileName);

            System.Diagnostics.Debug.WriteLine($"?? Sending PDF to RUT parser API ({fileBytes.Length} bytes)");

            // Send request
            var response = await httpClient.PostAsync(_baseUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"? RUT parsing successful");

                var rutData = JsonSerializer.Deserialize<RutParseResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (rutData != null)
                {
                    System.Diagnostics.Debug.WriteLine($"? RUT Data parsed successfully:");
                    System.Diagnostics.Debug.WriteLine($"   - NIT: {rutData.Nit}");
                    System.Diagnostics.Debug.WriteLine($"   - Name: {rutData.FullName.Display}");
                    System.Diagnostics.Debug.WriteLine($"   - Email: {rutData.Email}");
                }

                return rutData;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"? RUT parsing failed: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Error: {errorContent}");
                return null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Error parsing RUT: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return null;
        }
    }
}

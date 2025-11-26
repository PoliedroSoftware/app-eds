using APP.Eds.Models.Court;
using APP.Eds.Services.Config;
using System.Net.Http.Headers;

namespace APP.Eds.Services.Files;

/// <summary>
/// Servicio especializado para la subida de archivos al servidor
/// </summary>
public class FileUploadService(string? authToken)
{

    /// <summary>
    /// Sube una colección de documentos al servidor con el ID del corte asociado
    /// </summary>
    /// <param name="documents">Colección de documentos a subir</param>
    /// <param name="courtId">ID del corte al que pertenecen los documentos</param>
    /// <returns>Resultado de la operación con detalles de éxito y errores</returns>
    public async Task<FileUploadResult> UploadDocumentsAsync(IEnumerable<CourtDocument> documents, int courtId)
    {
        if (documents == null || !documents.Any())
        {
            return new FileUploadResult
            {
                Success = true,
                Message = "No hay documentos para subir"
            };
        }

        if (courtId <= 0)
        {
            return new FileUploadResult
            {
                Success = false,
                Message = "ID de corte inválido"
            };
        }

        if (string.IsNullOrEmpty(authToken))
        {
            return new FileUploadResult
            {
                Success = false,
                Message = "Token de autenticación no encontrado"
            };
        }

        var result = new FileUploadResult { Success = true };
        string apiUrl = $"{Configuration.BaseUrl}/api/v1/files/upload";

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

        foreach (var doc in documents)
        {
            try
            {
                var uploadResult = await UploadSingleDocumentAsync(client, apiUrl, doc, courtId);

                if (!uploadResult.Success)
                {
                    result.Success = false;
                    result.FailedUploads.Add(uploadResult);
                    System.Diagnostics.Debug.WriteLine($"FileUploadService: Error al subir {doc.DocumentName} - {uploadResult.Message}");
                }
                else
                {
                    result.SuccessfulUploads.Add(uploadResult);
                    System.Diagnostics.Debug.WriteLine($"FileUploadService: Archivo subido exitosamente - {doc.DocumentName}");
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                var errorResult = new SingleFileUploadResult
                {
                    Success = false,
                    FileName = doc.DocumentName,
                    Message = $"Excepción: {ex.Message}"
                };
                result.FailedUploads.Add(errorResult);
                System.Diagnostics.Debug.WriteLine($"FileUploadService: Excepción subiendo {doc.DocumentName} - {ex.Message}");
            }
        }

        // Generar mensaje de resumen
        result.Message = GenerateSummaryMessage(result);

        return result;
    }

    /// <summary>
    /// Sube un único documento al servidor con el ID del corte
    /// </summary>
    private async Task<SingleFileUploadResult> UploadSingleDocumentAsync(
        HttpClient client,
        string apiUrl,
        CourtDocument document,
        int courtId)
    {
        try
        {
            byte[] fileBytes = Convert.FromBase64String(document.Descripcion);
            using var fileStream = new MemoryStream(fileBytes);
            using var contentFile = new MultipartFormDataContent();
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            // Agregar el archivo con el nombre del campo esperado por la API
            contentFile.Add(fileContent, "Files", document.DocumentName);

            // Agregar el CourtId como campo del formulario
            contentFile.Add(new StringContent(courtId.ToString()), "CourtId");

            HttpResponseMessage response = await client.PostAsync(apiUrl, contentFile);

            if (response.IsSuccessStatusCode)
            {
                return new SingleFileUploadResult
                {
                    Success = true,
                    FileName = document.DocumentName,
                    Message = "Archivo subido exitosamente"
                };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return new SingleFileUploadResult
                {
                    Success = false,
                    FileName = document.DocumentName,
                    StatusCode = (int)response.StatusCode,
                    Message = $"Error HTTP: {response.StatusCode} - {response.ReasonPhrase}\nDetalle: {errorContent}"
                };
            }
        }
        catch (FormatException formatEx)
        {
            return new SingleFileUploadResult
            {
                Success = false,
                FileName = document.DocumentName,
                Message = $"Error de formato Base64: {formatEx.Message}"
            };
        }
        catch (Exception ex)
        {
            return new SingleFileUploadResult
            {
                Success = false,
                FileName = document.DocumentName,
                Message = $"Error inesperado: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Genera un mensaje de resumen basado en los resultados de la subida
    /// </summary>
    private string GenerateSummaryMessage(FileUploadResult result)
    {
        int total = result.SuccessfulUploads.Count + result.FailedUploads.Count;
        int successful = result.SuccessfulUploads.Count;
        int failed = result.FailedUploads.Count;

        if (failed == 0)
        {
            return $"✅ Todos los archivos subidos exitosamente ({successful}/{total})";
        }
        else if (successful == 0)
        {
            return $"❌ Error al subir todos los archivos ({failed}/{total})";
        }
        else
        {
            return $"⚠️ Subida parcial: {successful} exitosos, {failed} fallidos de {total} archivos";
        }
    }
}

/// <summary>
/// Resultado de la operación de subida de múltiples archivos
/// </summary>
public class FileUploadResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public List<SingleFileUploadResult> SuccessfulUploads { get; set; } = new();
    public List<SingleFileUploadResult> FailedUploads { get; set; } = new();

    /// <summary>
    /// Indica si hubo algún archivo que no se pudo subir
    /// </summary>
    public bool HasFailures => FailedUploads.Any();

    /// <summary>
    /// Total de archivos procesados
    /// </summary>
    public int TotalFiles => SuccessfulUploads.Count + FailedUploads.Count;
}

/// <summary>
/// Resultado de la subida de un archivo individual
/// </summary>
public class SingleFileUploadResult
{
    public bool Success { get; set; }
    public string FileName { get; set; }
    public int? StatusCode { get; set; }
    public string Message { get; set; }
}

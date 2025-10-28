using System.Net.Http.Headers;
using APP.Eds.Models.Court;
using APP.Eds.Services.Config;

namespace APP.Eds.Services.Files;

/// <summary>
/// Servicio especializado para la subida de archivos al servidor
/// </summary>
public class FileUploadService(string? authToken)
{
    /// <summary>
    /// Sube una colección de documentos al servidor (modo individual, compatibilidad).
    /// </summary>
    public async Task<FileUploadResult> UploadDocumentsAsync(IEnumerable<CourtDocument> documents)
    {
        if (documents == null || !documents.Any())
        {
            return new FileUploadResult
            {
                Success = true,
                Message = "No hay documentos para subir"
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
                var uploadResult = await UploadSingleDocumentAsync(client, apiUrl, doc);

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

        result.Message = GenerateSummaryMessage(result);
        return result;
    }

    /// <summary>
    /// Sube una colección de documentos al servidor en un solo batch (una sola petición HTTP).
    /// </summary>
    /// <param name="documents">Archivos a subir (Descripcion en Base64 y nombre)</param>
    /// <param name="courtId">Id del corte (opcional) para asociar los archivos en el backend</param>
    /// <param name="courtIdFieldName">Nombre del campo form-data para el Id del corte</param>
    public async Task<FileUploadResult> UploadDocumentsBatchAsync(
        IEnumerable<CourtDocument> documents,
        int? courtId = null,
        string courtIdFieldName = "courtId")
    {
        var result = new FileUploadResult { Success = true };

        if (documents == null || !documents.Any())
        {
            result.Message = "No hay documentos para subir";
            return result;
        }
        if (string.IsNullOrEmpty(authToken))
        {
            return new FileUploadResult { Success = false, Message = "Token de autenticación no encontrado" };
        }

        string apiUrl = $"{Configuration.BaseUrl}/api/v1/files/upload";
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

        using var form = new MultipartFormDataContent();

        // Metadatos opcionales
        if (courtId.HasValue)
            form.Add(new StringContent(courtId.Value.ToString()), courtIdFieldName);

        // Agregar todos los archivos al mismo formulario
        foreach (var doc in documents)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(doc?.Descripcion))
                {
                    result.Success = false;
                    result.FailedUploads.Add(new SingleFileUploadResult
                    {
                        Success = false,
                        FileName = doc?.DocumentName ?? "(sin nombre)",
                        Message = "El contenido Base64 está vacío o es nulo"
                    });
                    continue;
                }

                var bytes = Convert.FromBase64String(doc.Descripcion);
                var fileContent = new ByteArrayContent(bytes);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetContentTypeFromName(doc.DocumentName));
                form.Add(fileContent, "files", doc.DocumentName ?? "archivo.bin");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.FailedUploads.Add(new SingleFileUploadResult
                {
                    Success = false,
                    FileName = doc?.DocumentName ?? "(sin nombre)",
                    Message = $"Error preparando archivo: {ex.Message}"
                });
            }
        }

        var response = await client.PostAsync(apiUrl, form);

        if (response.IsSuccessStatusCode)
        {
            foreach (var ok in documents.Where(d => !result.FailedUploads.Any(f => f.FileName == d.DocumentName)))
            {
                result.SuccessfulUploads.Add(new SingleFileUploadResult
                {
                    Success = true,
                    FileName = ok.DocumentName,
                    Message = "Archivo subido exitosamente"
                });
            }
        }
        else
        {
            result.Success = false;
            var error = await response.Content.ReadAsStringAsync();
            result.Message = $"Error HTTP: {(int)response.StatusCode} - {response.ReasonPhrase}\n{error}";
        }

        result.Message = GenerateSummaryMessage(result);
        return result;
    }

    /// <summary>
    /// Sube un único documento (ruta individual)
    /// </summary>
    private async Task<SingleFileUploadResult> UploadSingleDocumentAsync(
        HttpClient client,
        string apiUrl,
        CourtDocument document)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(document?.Descripcion))
            {
                return new SingleFileUploadResult
                {
                    Success = false,
                    FileName = document?.DocumentName ?? "(sin nombre)",
                    Message = "El contenido Base64 está vacío o es nulo"
                };
            }

            byte[] fileBytes = Convert.FromBase64String(document.Descripcion);
            using var fileStream = new MemoryStream(fileBytes);
            using var contentFile = new MultipartFormDataContent();
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetContentTypeFromName(document.DocumentName));

            contentFile.Add(fileContent, "files", document.DocumentName);

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
                return new SingleFileUploadResult
                {
                    Success = false,
                    FileName = document.DocumentName,
                    StatusCode = (int)response.StatusCode,
                    Message = $"Error HTTP: {response.StatusCode} - {response.ReasonPhrase}"
                };
            }
        }
        catch (FormatException formatEx)
        {
            return new SingleFileUploadResult
            {
                Success = false,
                FileName = document?.DocumentName ?? "(sin nombre)",
                Message = $"Error de formato Base64: {formatEx.Message}"
            };
        }
        catch (Exception ex)
        {
            return new SingleFileUploadResult
            {
                Success = false,
                FileName = document?.DocumentName ?? "(sin nombre)",
                Message = $"Error inesperado: {ex.Message}"
            };
        }
    }

    private static string GetContentTypeFromName(string? fileName)
    {
        var ext = Path.GetExtension(fileName ?? "").ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png"            => "image/png",
            ".pdf"            => "application/pdf",
            _                 => "application/octet-stream"
        };
    }

    private string GenerateSummaryMessage(FileUploadResult result)
    {
        int total = result.SuccessfulUploads.Count + result.FailedUploads.Count;
        int successful = result.SuccessfulUploads.Count;
        int failed = result.FailedUploads.Count;

        if (failed == 0)
            return $"✅ Todos los archivos subidos exitosamente ({successful}/{total})";
        else if (successful == 0)
            return $"❌ Error al subir todos los archivos ({failed}/{total})";
        else
            return $"⚠️ Subida parcial: {successful} exitosos, {failed} fallidos de {total} archivos";
    }
}

/// <summary>Resultado de la operación de subida de múltiples archivos</summary>
public class FileUploadResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<SingleFileUploadResult> SuccessfulUploads { get; set; } = new();
    public List<SingleFileUploadResult> FailedUploads { get; set; } = new();
    public bool HasFailures => FailedUploads.Any();
    public int TotalFiles => SuccessfulUploads.Count + FailedUploads.Count;
}

/// <summary>Resultado de la subida de un archivo individual</summary>
public class SingleFileUploadResult
{
    public bool Success { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int? StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
}

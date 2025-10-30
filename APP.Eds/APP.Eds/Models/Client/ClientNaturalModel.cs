using System.Text.Json.Serialization;

namespace APP.Eds.Models.Client;

public class ClientNaturalModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("documentNumber")]
    public string DocumentNumber { get; set; } = string.Empty;

    [JsonPropertyName("documentTypeId")]
    public int DocumentTypeId { get; set; }

    [JsonPropertyName("documentType")]
    public string? DocumentType { get; set; }

    [JsonPropertyName("electronicInvoiceEmail")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("middleName")]
    public string? MiddleName { get; set; }

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("secondSurname")]
    public string? SecondSurname { get; set; }

    // Propiedad calculada para nombre completo
    public string FullName
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(Name)) parts.Add(Name);
            if (!string.IsNullOrEmpty(MiddleName)) parts.Add(MiddleName);
            if (!string.IsNullOrEmpty(LastName)) parts.Add(LastName);
            if (!string.IsNullOrEmpty(SecondSurname)) parts.Add(SecondSurname);
            return string.Join(" ", parts);
        }
    }
}

public class ClientNaturalResponse
{
    [JsonPropertyName("data")]
    public List<ClientNaturalModel> Data { get; set; } = new();

    [JsonPropertyName("totalRecords")]
    public int TotalRecords { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("pageNumber")]
    public int PageNumber { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }
}

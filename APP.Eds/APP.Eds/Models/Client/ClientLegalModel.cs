using System.Text.Json.Serialization;

namespace APP.Eds.Models.Client;

public class ClientLegalModel
{
    [JsonPropertyName("id")]
  public int Id { get; set; }

  [JsonPropertyName("companyName")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("documentTypeId")]
    public int DocumentTypeId { get; set; }

    [JsonPropertyName("documentNumber")]
    public string DocumentNumber { get; set; } = string.Empty;

    [JsonPropertyName("electronicInvoiceEmail")]
 public string Email { get; set; } = string.Empty;

    // Campos adicionales del API
  [JsonPropertyName("verificationDigit")]
    public int VerificationDigit { get; set; }

    [JsonPropertyName("vatResponsibleParty")]
    public bool VatResponsibleParty { get; set; }

    [JsonPropertyName("largeTaxpayer")]
    public bool LargeTaxpayer { get; set; }

    [JsonPropertyName("selfRetainer")]
    public bool SelfRetainer { get; set; }

    [JsonPropertyName("withholdingAgent")]
    public bool WithholdingAgent { get; set; }

    [JsonPropertyName("simpleTaxRegime")]
    public bool SimpleTaxRegime { get; set; }

    // Propiedades opcionales (pueden ser null en el JSON)
    [JsonPropertyName("name")]
    public string? FirstName { get; set; }

  [JsonPropertyName("middleName")]
    public string? MiddleName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("secondSurname")]
    public string? SecondSurname { get; set; }

    // Propiedades calculadas
    public string DocumentType
    {
        get
        {
         return DocumentTypeId switch
   {
            1 => "NIT",
                2 => "CC", // Cédula de Ciudadanía
        3 => "CE", // Cédula de Extranjería
     4 => "PAS", // Pasaporte
             _ => "DOC"
     };
        }
    }

    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    // Propiedad calculada para mostrar en el selector con formato completo
    public string DisplayText
    {
  get
      {
          // Si es el cliente "Sin Cliente", mostrar solo el nombre
  if (Id == 0)
       {
        return Name;
    }

      // Si falta el nombre de la compañía, usar mensaje genérico
  if (string.IsNullOrEmpty(Name))
    {
       return $"Cliente {Id}";
            }

            // Formato: "Nombre Compañía - TipoDoc Número"
            // Ejemplo: "Empresa XYZ - NIT 123456789-1"
            var docType = DocumentType;
     var docNumber = string.IsNullOrEmpty(DocumentNumber) ? "N/A" : DocumentNumber;
            
      // Agregar dígito de verificación si es NIT
            if (DocumentTypeId == 1 && VerificationDigit > 0)
            {
 docNumber = $"{docNumber}-{VerificationDigit}";
      }
      
            return $"{Name} - {docType} {docNumber}";
        }
    }
}

public class ClientLegalResponse
{
    [JsonPropertyName("data")]
    public List<ClientLegalModel> Data { get; set; } = new();

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

using System.Text.Json.Serialization;

namespace APP.Eds.Models.Rut;

/// <summary>
/// Model for RUT parse API response from PDF extraction service
/// </summary>
public class RutParseResponse
{
    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("formNumber")]
    public string FormNumber { get; set; } = string.Empty;

    [JsonPropertyName("nit")]
    public string Nit { get; set; } = string.Empty;

    [JsonPropertyName("dv")]
    public string Dv { get; set; } = string.Empty;

    [JsonPropertyName("contributorType")]
    public string ContributorType { get; set; } = string.Empty;

    [JsonPropertyName("documentType")]
    public string DocumentType { get; set; } = string.Empty;

    [JsonPropertyName("documentNumber")]
    public string DocumentNumber { get; set; } = string.Empty;

    [JsonPropertyName("fullName")]
    public RutFullName FullName { get; set; } = new();

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;

    [JsonPropertyName("department")]
    public string Department { get; set; } = string.Empty;

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("postalCode")]
    public string PostalCode { get; set; } = string.Empty;

    [JsonPropertyName("economicActivities")]
    public List<RutEconomicActivity> EconomicActivities { get; set; } = new();

    [JsonPropertyName("responsibilities")]
    public List<RutResponsibility> Responsibilities { get; set; } = new();

    [JsonPropertyName("issueDate")]
    public string IssueDate { get; set; } = string.Empty;

    [JsonPropertyName("pdfGeneratedAt")]
    public string PdfGeneratedAt { get; set; } = string.Empty;

    [JsonPropertyName("raw")]
    public RutRawData Raw { get; set; } = new();
}

public class RutFullName
{
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("middleNames")]
    public string MiddleNames { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("secondLastName")]
    public string SecondLastName { get; set; } = string.Empty;

    [JsonPropertyName("display")]
    public string Display { get; set; } = string.Empty;
}

public class RutEconomicActivity
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("startDate")]
    public string StartDate { get; set; } = string.Empty;
}

public class RutResponsibility
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public class RutRawData
{
    [JsonPropertyName("dianSectional")]
    public string DianSectional { get; set; } = string.Empty;
}

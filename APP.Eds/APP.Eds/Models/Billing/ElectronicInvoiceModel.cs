using System.Text.Json.Serialization;

namespace APP.Eds.Models.Billing;

public class ElectronicInvoiceModel
{
    [JsonPropertyName("invoiceHash")]
    public string InvoiceHash { get; set; } 

    [JsonPropertyName("invoiceNumber")]
    public string InvoiceNumber { get; set; }

    [JsonPropertyName("prefix")]
    public string Prefix { get; set; }

    [JsonPropertyName("fullInvoiceNumber")]
    public string FullInvoiceNumber => $"{Prefix}-{InvoiceNumber}";

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("clientName")]
    public string ClientName { get; set; }

    [JsonPropertyName("clientDocumentNumber")]
    public string ClientDocumentNumber { get; set; }

    [JsonPropertyName("totalAmount")]
    public double TotalAmount { get; set; }

    [JsonPropertyName("taxAmount")]
    public double TaxAmount { get; set; }

    [JsonPropertyName("subtotal")]
    public double Subtotal { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "Emitida";

    [JsonPropertyName("paymentMethod")]
    public string PaymentMethod { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("qrCode")]
    public string QRCode { get; set; }

   
    [JsonPropertyName("techProviderFootNote")]
    public string TechProviderFootNote { get; set; }

    // ✨ NEW: Islander and EDS information
    [JsonPropertyName("islanderId")]
    public string IslanderId { get; set; }

    [JsonPropertyName("islanderName")]
    public string IslanderName { get; set; }

    [JsonPropertyName("edsId")]
    public string EdsId { get; set; }

    [JsonPropertyName("edsName")]
    public string EdsName { get; set; }

    public string DateFormatted => Date.ToString("dd/MM/yyyy HH:mm");
    public string TotalAmountFormatted => $"${TotalAmount:N2}";
    public string StatusIcon => Status switch
    {
        "Emitida" => "✅",
        "Anulada" => "❌",
        _ => "⚠️"
    };
    public string PaymentMethodIcon => PaymentMethod switch
    {
        "Efectivo" => "💵",
        "Tarjeta" => "💳",
        _ => "💰"
    };

    public bool IsPdfAvailable => !string.IsNullOrEmpty(InvoiceHash);
    public string CudeInfo => !string.IsNullOrEmpty(InvoiceHash)
        ? $"CUDE: {InvoiceHash.Substring(0, Math.Min(16, InvoiceHash.Length))}..."
      : "Sin CUDE";

    // ✨ NEW: Formatted display properties
    public string IslanderInfo => !string.IsNullOrWhiteSpace(IslanderName) 
        ? $"👤 {IslanderName}" 
        : "👤 No registrado";

    public string EdsInfo => !string.IsNullOrWhiteSpace(EdsName) 
        ? $"🏪 {EdsName}" 
        : "🏪 No registrado";
}

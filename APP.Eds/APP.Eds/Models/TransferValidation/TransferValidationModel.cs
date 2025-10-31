using System.Text.Json.Serialization;

namespace APP.Eds.Models.TransferValidation;

/// <summary>
/// Modelo para un registro individual de validación de transferencia QR
/// </summary>
public class TransferValidationModel
{
    [JsonPropertyName("idTransferValidation")]
    public int IdTransferValidation { get; set; }

    [JsonPropertyName("uniqueId")]
    public string UniqueId { get; set; } = string.Empty;

    [JsonPropertyName("customerName")]
    public string CustomerName { get; set; } = string.Empty;

    [JsonPropertyName("transactionAmount")]
    public double TransactionAmount { get; set; }

    [JsonPropertyName("transactionDate")]
    public string TransactionDate { get; set; } = string.Empty;

    [JsonPropertyName("transactionTime")]
    public string TransactionTime { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("confirmedBy")]
    public string ConfirmedBy { get; set; } = string.Empty;

    /// <summary>
    /// Devuelve el color correspondiente al estado de la transacción
    /// </summary>
    public Color StatusColor => Status switch
    {
        "CONFIRMADA" => Color.FromArgb("#4CAF50"),
        "PENDIENTE" => Color.FromArgb("#FF9800"),
        "RECHAZADA" => Color.FromArgb("#F44336"),
        _ => Color.FromArgb("#9E9E9E")
    };

    /// <summary>
    /// Devuelve el icono correspondiente al estado de la transacción
    /// </summary>
    public string StatusIcon => Status switch
    {
        "CONFIRMADA" => "✓",
        "PENDIENTE" => "⏳",
        "RECHAZADA" => "✗",
        _ => "❓"
    };

    /// <summary>
    /// Devuelve la fecha y hora formateadas
    /// </summary>
    public string FormattedDateTime => $"{TransactionDate} {TransactionTime}";

    /// <summary>
    /// Devuelve el monto formateado
    /// </summary>
    public string FormattedAmount => $"${TransactionAmount:N0}";
}

/// <summary>
/// Respuesta de la API para validaciones de transferencias
/// </summary>
public class TransferValidationResponse
{
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public List<TransferValidationModel> Data { get; set; } = new();
}

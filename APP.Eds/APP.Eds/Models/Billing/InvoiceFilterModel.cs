using System.Text.Json.Serialization;

namespace APP.Eds.Models.Billing;

/// <summary>
/// Modelo para filtrar el historial de facturas según rol y criterios
/// </summary>
public class InvoiceFilterModel
{
    /// <summary>
    /// ID de la EDS a filtrar (opcional para Admin/Contador)
    /// </summary>
    [JsonPropertyName("edsId")]
    public string EdsId { get; set; }

    /// <summary>
    /// Nombre de la EDS a filtrar
    /// </summary>
    [JsonPropertyName("edsName")]
    public string EdsName { get; set; }

    /// <summary>
    /// ID del islero a filtrar (opcional para Admin/Contador, forzado para Islero)
    /// </summary>
    [JsonPropertyName("islanderId")]
    public string IslanderId { get; set; }

    /// <summary>
    /// Nombre del islero a filtrar
    /// </summary>
    [JsonPropertyName("islanderName")]
    public string IslanderName { get; set; }

    /// <summary>
    /// Estado de la factura: "SIN_EMITIR", "EMITIDA", "ANULADA", "TODOS"
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = "TODOS";

    /// <summary>
    /// Fecha desde (formato yyyy-MM-dd)
    /// </summary>
    [JsonPropertyName("dateFrom")]
    public DateTime? DateFrom { get; set; }

    /// <summary>
    /// Fecha hasta (formato yyyy-MM-dd)
    /// </summary>
    [JsonPropertyName("dateTo")]
    public DateTime? DateTo { get; set; }

    /// <summary>
    /// Página actual para paginación
    /// </summary>
    [JsonPropertyName("page")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Tamaño de página para paginación
    /// </summary>
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// Rol del usuario que ejecuta el filtro (para validación en servidor)
    /// </summary>
    [JsonPropertyName("userRole")]
    public string UserRole { get; set; }

    /// <summary>
    /// ID del usuario actual (para validación en servidor)
    /// </summary>
    [JsonPropertyName("currentUserId")]
    public string CurrentUserId { get; set; }

    /// <summary>
    /// Crea una instancia del filtro con valores por defecto para el último mes
    /// </summary>
    public static InvoiceFilterModel CreateDefault()
    {
        return new InvoiceFilterModel
        {
            Status = "TODOS",
            DateFrom = DateTime.Now.AddDays(-30),
            DateTo = DateTime.Now,
            Page = 1,
            PageSize = 50
        };
    }

    /// <summary>
    /// Crea una instancia del filtro para un usuario Islero (solo sus facturas emitidas)
    /// </summary>
    public static InvoiceFilterModel CreateForIslander(string islanderId, string islanderName)
    {
        return new InvoiceFilterModel
        {
            IslanderId = islanderId,
            IslanderName = islanderName,
            Status = "EMITIDA",  // Solo facturas emitidas para isleros
            DateFrom = DateTime.Now.AddDays(-30),
            DateTo = DateTime.Now,
            Page = 1,
            PageSize = 50,
            UserRole = "User"
        };
    }

    /// <summary>
    /// Valida si el filtro es válido para el rol del usuario
    /// </summary>
    public bool IsValidForRole()
    {
        // Si es islero, debe tener IslanderId y solo puede ver facturas emitidas
        if (UserRole == "User" && (string.IsNullOrWhiteSpace(IslanderId) || Status != "EMITIDA"))
        {
            return false;
        }

        return true;
    }
}

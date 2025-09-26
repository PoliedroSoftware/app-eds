using System.ComponentModel.DataAnnotations;

namespace APP.Eds.Models.Phone;

public class PhoneModel
{
    public int IdPhone { get; set; }
    
    [Required(ErrorMessage = "El número de teléfono es requerido")]
    public string Number { get; set; } = string.Empty;
    
    public string CountryCode { get; set; } = "57"; // Colombia por defecto
    
    public string FullNumber => $"+{CountryCode}{Number}";
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

// Modelo simplificado para el request que espera el backend
public class PhoneRequest
{
    public PhoneRequestData Request { get; set; } = new();
}

public class PhoneRequestData
{
    public List<string> Numbers { get; set; } = new();
}

public class PhoneResponseModel
{
    public List<PhoneModel> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
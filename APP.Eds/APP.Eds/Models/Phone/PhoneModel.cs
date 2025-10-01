using System.ComponentModel.DataAnnotations;

namespace APP.Eds.Models.Phone;

public class PhoneModel
{
    public int IdPhone { get; set; }
    
    [Required(ErrorMessage = "El número de teléfono es requerido")]
    public string Number { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El nombre del propietario es requerido")]
    public string Name { get; set; } = string.Empty; // Nombre del propietario
    
    public string CountryCode { get; set; } = "57"; // Colombia por defecto
    
    public string FullNumber => $"+{CountryCode}{Number}";
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

// Modelo para un teléfono individual con su propietario
public class PhoneItemModel
{
    public string Number { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

// Modelo para el request que espera el backend (basado en el error que recibiste)
public class PhoneRequest
{
    public PhoneRequestData Request { get; set; } = new();
}

public class PhoneRequestData
{
    public List<PhoneItemModel> Phones { get; set; } = new(); // El backend espera "Phones"
}

public class PhoneResponseModel
{
    public List<PhoneModel> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
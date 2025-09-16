using System.Text.Json.Serialization;

namespace APP.Eds.Models.Compartiment;


public class TankResponse
{
    [JsonPropertyName("idTank")]
    public int IdTank { get; set; }

    [JsonPropertyName("number")]
    public string Number { get; set; }

    [JsonPropertyName("compartment")]
    public int Compartment { get; set; }

    [JsonPropertyName("ability")]
    public double Ability { get; set; }

    [JsonPropertyName("stock")]
    public double Stock { get; set; }

    /// <summary>
    /// Propiedad calculada para mostrar el tanque en formato "Tanque # - capacidad del tanque"
    /// </summary>
    public string DisplayText => $"Tanque {Number} - Capacidad {Ability:N0} L";
}

namespace APP.Eds.Models.Capacity;

public class CapacityModel
{
    public int IdCapacity { get; set; }
    public string Code { get; set; } = string.Empty;
    public double Height { get; set; } = 0;
    public double Gallon { get; set; } = 0;
    public int Liters { get; set; } = 0;
}

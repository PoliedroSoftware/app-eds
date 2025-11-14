namespace APP.Eds.Models.CompartimentCapacity;

public class CompartimentCapacityModel
{
    public int IdCompartiment { get; set; }
    public int IdCapacity { get; set; }
    public int Default { get; set; } // <- si el backend lo define como entero
}

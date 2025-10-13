namespace APP.Eds.Models.Business;

public class BusinessModel
{
    public int IdBusiness  { get; set; }
    public string Name { get; set; } = string.Empty;

    // Campo añadido solución error context
    public string Context { get; set; } = "AppEDS";
    public string KeycloakId { get; set; } = string.Empty;
}

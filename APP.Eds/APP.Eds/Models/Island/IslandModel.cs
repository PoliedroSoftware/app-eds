using System.ComponentModel.DataAnnotations.Schema;

namespace APP.Eds.Models.Island;

public class IslandModel
{
    public string Description { get; set; }
    public int? IdEds { get; set; }
    public string? NameEDS { get; set; }
}

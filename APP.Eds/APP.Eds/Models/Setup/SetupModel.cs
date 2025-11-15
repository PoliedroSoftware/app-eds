using APP.Eds.Models.Compartiment;
using APP.Eds.Models.Dispensers;
using APP.Eds.Models.Hose;
using APP.Eds.Models.Island;
using APP.Eds.Models.Islander;
using APP.Eds.Models.Product;
using APP.Eds.Models.Provider;
using APP.Eds.Models.Tank;
namespace APP.Eds.Models.Setup;

public class SetupModel
{
    public Business.BusinessModel Bussiness { get; set; }
    public List<Eds.EdsModel> EDS { get; set; }
    public List<IslandModel> Islands { get; set; }
    public List<TankModel> Tanks { get; set; }
    public List<CompartimentModel> Compartiments { get; set; }
    public List<DispensersModel> Dispensers { get; set; }
    public List<HoseModel> Hoses { get; set; }
    public List<ProductModel> Products { get; set; }
    public List<IslanderModel> Islanders { get; set; }
    public List<ProviderModel> Providers { get; set; }
}
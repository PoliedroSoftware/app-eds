using APP.Eds.Models.Eds;
using APP.Eds.Models.Product;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace APP.Eds.Models.Court;

public class HoseCourtModel
{
    [JsonPropertyName("idHose")]
    public int IdHose { get; set; }

    [JsonPropertyName("idDispensers")]
    public int IdDispensers { get; set; }

    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("accumulatedAmount")]
    public double AccumulatedAmount { get; set; }

    [JsonPropertyName("accumulatedGallons")]
    public double AccumulatedGallons { get; set; }

    [JsonPropertyName("idProductType")]
    public int IdProductType { get; set; }

    [JsonPropertyName("price")]
    public double Price { get; set; }

    [JsonPropertyName("sellPrice")]
    public double SellPrice { get; set; }

    [JsonPropertyName("dispensersEntity")]
    public DispenserCourtModel DispensersEntity { get; set; }

    [JsonPropertyName("productTypeEntity")]
    public ProductTypeModelResponse ProductTypeEntity { get; set; }

    [JsonPropertyName("productEntity")]
    public ProductEntity ProductEntity { get; set; }

    [JsonPropertyName("edsEntity")]
    public EdsResponse EdsEntity { get; set; }

    // Propiedad computada que retorna sellPrice desde productEntity si existe, sino usa las propiedades directas como fallback
    public double EffectiveSellPrice
    {
        get
        {
            // Prioridad 1: sellPrice desde productEntity
            if (ProductEntity?.SellPrice > 0)
                return ProductEntity.SellPrice;

            // Prioridad 2: sellPrice directo
            if (SellPrice > 0)
                return SellPrice;

            // Prioridad 3: price como fallback
            return Price;
        }
    }

    public string ProductName => ProductTypeEntity?.Description ?? "Unknown";
    public int DispensersNumber => DispensersEntity?.Number ?? 0;
    public string EdsName => EdsEntity?.Name ?? "Unknown";
    public int EdsId => DispensersEntity?.Eds?.IdEds ?? 0;

    public string DisplayText =>
     $"Dispenser: {DispensersNumber}\nHose: {Number}\nProduct: {ProductName}\n-------------------------------------------------";
}

// Clase para mapear la entidad del producto que viene del backend
public class ProductEntity
{
    [JsonPropertyName("idProduct")]
    public int IdProduct { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("idProductType")]
    public int IdProductType { get; set; }

    [JsonPropertyName("purchasePrice")]
    public double PurchasePrice { get; set; }

    [JsonPropertyName("sellPrice")]
    public double SellPrice { get; set; }

    [JsonPropertyName("stock")]
    [JsonConverter(typeof(SafeIntConverter))]
    public int Stock { get; set; }

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    public class SafeIntConverter : JsonConverter<int>
    {

        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Number:
                    if (reader.TryGetInt32(out var i)) return i;
                    if (reader.TryGetDouble(out var d)) return (int)Math.Round(d, MidpointRounding.AwayFromZero);
                    break;

                case JsonTokenType.String:
                    string? s = reader.GetString();
                    if (string.IsNullOrWhiteSpace(s)) return 0;

                    if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i2))
                        return i2;

                    if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d2))
                        return (int)Math.Round(d2, MidpointRounding.AwayFromZero);
                    break;

                case JsonTokenType.Null:
                    return 0;
            }
            return 0;
        }
      
        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }            
}
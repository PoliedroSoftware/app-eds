using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace APP.Eds.Models.Court
{
    public class ProductCourtModel
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
    }

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

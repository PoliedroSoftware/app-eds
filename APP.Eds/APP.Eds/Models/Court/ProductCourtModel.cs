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
                    if (reader.TryGetInt32(out int value))
                        return value;
                    if (reader.TryGetDouble(out double doubleValue))
                        return (int)Math.Round(doubleValue);
                    break;
                    
                case JsonTokenType.String:
                    string stringValue = reader.GetString();
                    if (int.TryParse(stringValue, out int parsedValue))
                        return parsedValue;
                    if (double.TryParse(stringValue, out double parsedDouble))
                        return (int)Math.Round(parsedDouble);
                    break;
                    
                case JsonTokenType.Null:
                    return 0; // Default value for null
            }
            
            return 0; // Default fallback value
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}

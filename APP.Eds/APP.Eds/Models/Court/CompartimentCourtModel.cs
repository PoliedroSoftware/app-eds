using System.Text.Json.Serialization;

namespace APP.Eds.Models.Court
{
    public class CompartimentCourtModel
    {
        [JsonPropertyName("id_compartiment")]
        public int IdCompartiment { get; set; }

        [JsonPropertyName("number")]
        public int Number { get; set; }

        [JsonPropertyName("nominal")]
        public double Nominal { get; set; }

        [JsonPropertyName("operative")]
        public double Operative { get; set; }

        [JsonPropertyName("stock")]
        public double Stock { get; set; }

        [JsonPropertyName("height")]
        public double Height { get; set; }

        [JsonPropertyName("id_tank")]
        public int IdTank { get; set; }
    }
}

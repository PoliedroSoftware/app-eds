using System.Text.Json.Serialization;

namespace APP.Eds.Models.Court
{
    public class ExpendituresCourtModel
    {
        [JsonPropertyName("id_court_expenditure")]
        public int IdCourtExpenditure { get; set; }

        [JsonPropertyName("id_court")]
        public int IdCourt { get; set; }

        [JsonPropertyName("id_expenditures")]
        public int IdExpenditure { get; set; }

        [JsonPropertyName("amount")]
        public double Amount { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

    }
}

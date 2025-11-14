using System.Text.Json.Serialization;

namespace APP.Eds.Models.Court
{
    /// <summary>
    /// Respuesta del servidor al crear un nuevo corte
    /// </summary>
    public class CourtCreateResponse
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public CourtResponseData Data { get; set; }
    }

    /// <summary>
    /// Datos del corte creado
    /// </summary>
    public class CourtResponseData
    {
        [JsonPropertyName("idCourt")]
        public int IdCourt { get; set; }

        [JsonPropertyName("consecutive")]
        public int Consecutive { get; set; }

        [JsonPropertyName("idBusiness")]
        public int IdBusiness { get; set; }

        [JsonPropertyName("idEds")]
        public int IdEds { get; set; }

        [JsonPropertyName("idIslander")]
        public int IdIslander { get; set; }

        [JsonPropertyName("dateStarttime")]
        public string DateStarttime { get; set; }

        [JsonPropertyName("starttime")]
        public string Starttime { get; set; }

        [JsonPropertyName("dateEndtime")]
        public string DateEndtime { get; set; }

        [JsonPropertyName("endtime")]
        public string Endtime { get; set; }
    }
}

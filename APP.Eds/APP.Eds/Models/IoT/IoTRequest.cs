using System.Text.Json.Serialization;

namespace APP.Eds.Models.IoT;

/// <summary>
/// Modelo para el mensaje IoT que se enviará a AWS
/// </summary>
public class IoTMessage
{
    [JsonPropertyName("input1")]
    public string Input1 { get; set; } = "0";

    [JsonPropertyName("input2")]
    public string Input2 { get; set; } = "0";

    [JsonPropertyName("output1")]
    public string Output1 { get; set; } = "0";

    [JsonPropertyName("output2")]
    public string Output2 { get; set; } = "0";
}

/// <summary>
/// Modelo para la solicitud completa IoT
/// </summary>
public class IoTRequest
{
    [JsonPropertyName("message")]
    public IoTMessage Message { get; set; } = new IoTMessage();

    [JsonPropertyName("topic")]
    public string Topic { get; set; } = "psr/dev/psr-4g-at/cmd/";
}

/// <summary>
/// Modelo para la respuesta de la API IoT
/// </summary>
public class IoTResponse
{
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public IoTResponseData? Data { get; set; }
}

/// <summary>
/// Datos de la respuesta IoT
/// </summary>
public class IoTResponseData
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("topic")]
    public string? Topic { get; set; }
}

using APP.Eds.Models.RabbitMQ;
using APP.Eds.Services.Config;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace APP.Eds.Services.RabbitMQ;

public interface IRabbitMQService
{
    Task<bool> PublishDocumentAsync(DocumentMessage document);
    Task<bool> PublishDocumentsAsync(IEnumerable<DocumentMessage> documents);
}

public class RabbitMQService : IRabbitMQService, IDisposable
{
    private IConnection? _connection;
    private IModel? _channel;
    private readonly object _lock = new object();
    private bool _disposed = false;

    public RabbitMQService()
    {
        InitializeConnection();
    }

    private void InitializeConnection()
    {
        try
        {
            var factory = new ConnectionFactory()
            {
                HostName = Configuration.RabbitMQHostName,
                UserName = Configuration.RabbitMQUserName,
                Password = Configuration.RabbitMQPassword,
                Port = 5672, // Puerto estándar de RabbitMQ
                VirtualHost = "/",
                RequestedHeartbeat = TimeSpan.FromSeconds(60),
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                AutomaticRecoveryEnabled = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declarar la cola de documentos como durable
            _channel.QueueDeclare(
                queue: Configuration.RabbitMQQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            System.Diagnostics.Debug.WriteLine("? RabbitMQ: Conexión establecida correctamente");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? RabbitMQ: Error al conectar: {ex.Message}");
            throw new InvalidOperationException($"No se pudo conectar a RabbitMQ: {ex.Message}", ex);
        }
    }

    public async Task<bool> PublishDocumentAsync(DocumentMessage document)
    {
        return await Task.Run(() =>
        {
            lock (_lock)
            {
                try
                {
                    if (_channel == null || _connection == null || !_connection.IsOpen)
                    {
                        System.Diagnostics.Debug.WriteLine("?? RabbitMQ: Reestableciendo conexión...");
                        InitializeConnection();
                    }

                    var json = JsonSerializer.Serialize(document, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    var body = Encoding.UTF8.GetBytes(json);

                    var properties = _channel.CreateBasicProperties();
                    properties.Persistent = true; // Hacer el mensaje persistente
                    properties.ContentType = "application/json";
                    properties.MessageId = Guid.NewGuid().ToString();
                    properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                    // Agregar headers con metadatos adicionales
                    properties.Headers = new Dictionary<string, object>
                    {
                        ["documentName"] = document.DocumentName,
                        ["courtId"] = document.CourtId.ToString(),
                        ["fileSize"] = document.FileSize.ToString(),
                        ["contentType"] = document.ContentType,
                        ["publishedAt"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                    };

                    _channel.BasicPublish(
                        exchange: "",
                        routingKey: Configuration.RabbitMQQueue,
                        basicProperties: properties,
                        body: body
                    );

                    System.Diagnostics.Debug.WriteLine($"? RabbitMQ: Documento '{document.DocumentName}' publicado en cola '{Configuration.RabbitMQQueue}'");
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"? RabbitMQ: Error publicando documento '{document.DocumentName}': {ex.Message}");
                    return false;
                }
            }
        });
    }

    public async Task<bool> PublishDocumentsAsync(IEnumerable<DocumentMessage> documents)
    {
        var results = new List<bool>();
        
        foreach (var document in documents)
        {
            var result = await PublishDocumentAsync(document);
            results.Add(result);
        }

        var successCount = results.Count(r => r);
        var totalCount = results.Count;

        System.Diagnostics.Debug.WriteLine($"?? RabbitMQ: {successCount}/{totalCount} documentos publicados correctamente");

        return successCount == totalCount;
    }

    public void Dispose()
    {
        if (_disposed) return;

        lock (_lock)
        {
            try
            {
                _channel?.Close();
                _channel?.Dispose();
                _connection?.Close();
                _connection?.Dispose();
                
                System.Diagnostics.Debug.WriteLine("?? RabbitMQ: Conexión cerrada correctamente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? RabbitMQ: Error al cerrar conexión: {ex.Message}");
            }
            finally
            {
                _channel = null;
                _connection = null;
                _disposed = true;
            }
        }
    }
}
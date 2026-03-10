using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace MadaTaskar.Services;

public class BoardEvent
{
    public string Type { get; set; } = "";
    public string From { get; set; } = "mada-task-ar";
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");
    public int? TaskId { get; set; }
    public string? TaskTitle { get; set; }
    public string? Assignee { get; set; }
    public string? Phase { get; set; }
    public string? Column { get; set; }
    public string? Details { get; set; }
}

public class EventBusService : IDisposable
{
    private readonly ILogger<EventBusService> _logger;
    private IConnection? _connection;
    private IChannel? _channel;
    private bool _connected = false;
    private readonly string _host;
    private readonly string _user;
    private readonly string _pass;

    public EventBusService(IConfiguration config, ILogger<EventBusService> logger)
    {
        _logger = logger;
        _host = config["RabbitMQ:Host"] ?? "mssql";
        _user = config["RabbitMQ:User"] ?? "guest";
        _pass = config["RabbitMQ:Pass"] ?? "guest";
        _ = ConnectAsync();
    }

    private async Task ConnectAsync()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _host,
                UserName = _user,
                Password = _pass,
                RequestedHeartbeat = TimeSpan.FromSeconds(30)
            };
            _connection = await factory.CreateConnectionAsync("mada-task-ar");
            _channel = await _connection.CreateChannelAsync();

            // Declare exchanges
            await _channel.ExchangeDeclareAsync("agents.broadcast", ExchangeType.Fanout, durable: true);
            await _channel.ExchangeDeclareAsync("agents.direct", ExchangeType.Direct, durable: true);

            _connected = true;
            _logger.LogInformation("EventBusService: połączono z RabbitMQ @ {Host}", _host);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("EventBusService: nie można połączyć z RabbitMQ: {Msg}", ex.Message);
        }
    }

    public async Task PublishAsync(BoardEvent evt, string? directRoutingKey = null)
    {
        if (!_connected || _channel is null)
        {
            _logger.LogDebug("EventBusService: brak połączenia, pomijam event {Type}", evt.Type);
            return;
        }

        try
        {
            var json = JsonSerializer.Serialize(evt);
            var body = Encoding.UTF8.GetBytes(json);
            var props = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent
            };

            // Broadcast to all
            await _channel.BasicPublishAsync("agents.broadcast", "", false, props, body);

            // Direct to assignee if known
            if (!string.IsNullOrEmpty(directRoutingKey))
                await _channel.BasicPublishAsync("agents.direct", directRoutingKey.ToLower(), false, props, body);

            _logger.LogDebug("EventBusService: opublikowano {Type} → broadcast{Direct}",
                evt.Type, directRoutingKey != null ? $" + direct:{directRoutingKey}" : "");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("EventBusService: błąd publish: {Msg}", ex.Message);
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}

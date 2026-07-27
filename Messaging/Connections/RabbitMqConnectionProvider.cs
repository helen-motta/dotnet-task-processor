using RabbitMQ.Client;
using TaskProcessor.Settings;

namespace TaskProcessor.Messaging.Connections;

public sealed class RabbitMqConnectionProvider :
    IRabbitMqConnectionProvider,
    IHostedService
{
    private readonly ConnectionFactory _connectionFactory;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private IConnection? _connection;

    public RabbitMqConnectionProvider(RabbitMqSettings settings)
    {
        _connectionFactory = new ConnectionFactory
        {
            HostName = settings.HostName,
            Port = settings.Port,
            UserName = settings.UserName,
            Password = settings.Password,
            ClientProvidedName = "task-processor-publisher",
            AutomaticRecoveryEnabled = true
        };
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await GetConnectionAsync(cancellationToken);
    }

    public async Task<IConnection> GetConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        var currentConnection = Volatile.Read(ref _connection);

        if (currentConnection is { IsOpen: true })
        {
            return currentConnection;
        }

        await _connectionLock.WaitAsync(cancellationToken);

        try
        {
            if (_connection is { IsOpen: true })
            {
                return _connection;
            }

            if (_connection is not null)
            {
                await _connection.DisposeAsync();
            }

            _connection = await _connectionFactory.CreateConnectionAsync(
                cancellationToken);

            return _connection;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _connectionLock.WaitAsync(CancellationToken.None);

        try
        {
            if (_connection is null)
            {
                return;
            }

            var connection = _connection;
            _connection = null;

            try
            {
                if (connection.IsOpen)
                {
                    await connection.CloseAsync(cancellationToken);
                }
            }
            finally
            {
                await connection.DisposeAsync();
            }
        }
        finally
        {
            _connectionLock.Release();
        }
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Producer.Helpers;
using RabbitMQ.Producer.Interfaces;
using System;
using System.Text.Json;
using System.Text;

public class RabbitMQService : IRabbitMQService
{
    private readonly ILogger<RabbitMQService> _logger;
    private IConnection _connection;
    private IModel _channel;
    private readonly ConnectionFactory _factory;

    public RabbitMQService(ILogger<RabbitMQService> logger, IConfiguration configuration)
    {
        _logger = logger;

        _factory = new ConnectionFactory()
        {
            HostName = configuration["RabbitMQ:HostName"],
            Port = int.Parse(configuration["RabbitMQ:Port"]),
            UserName = configuration["RabbitMQ:UserName"],
            Password = configuration["RabbitMQ:Password"],
            VirtualHost = configuration["RabbitMQ:VirtualHost"]
        };
    }

    public void SetupConnection()
    {
        try
        {
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
        }
        catch (RabbitMQClientException ex)
        {
            throw;
        }
    }

    public void DeclareExchange(string exchange, string exchangeType)
    {
        try
        {
            _channel.ExchangeDeclare(exchange: exchange, type: exchangeType, durable: true, autoDelete: false);
        }
        catch (OperationInterruptedException ex)
        {
            // Check if there is a exchange already existing
            var shutdownReason = ex.ShutdownReason;
            _logger.LogError(ex, "Exchange declaration failed: {0}", shutdownReason?.ReplyText);
            if (shutdownReason?.ReplyCode == 406 && shutdownReason.ReplyText.Contains("PRECONDITION_FAILED"))
            {
                _logger.LogWarning("Exchange '{0}' already exists with different attributes.", exchange);
            }
            else
            {
                // Throw the exception if it's not a known issue
                throw;
            }
        }
    }
    public Task PublishAsync(string message, string exchange, string routingKey)
    {
        var messageBody = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(messageBody);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;

        _logger.LogInformation($"Publishing message to RabbitMQ: {message} in exchange: {exchange} with routing key: {routingKey}");

        _channel.BasicPublish(exchange: exchange, routingKey: routingKey, basicProperties: properties, body: body);

        return Task.CompletedTask;
    }


    public void Dispose()
    {
        if (_channel != null)
        {
            _channel.Close();
            _channel.Dispose();
        }

        if (_connection != null)
        {
            _connection.Close();
            _connection.Dispose();
        }
    }
}

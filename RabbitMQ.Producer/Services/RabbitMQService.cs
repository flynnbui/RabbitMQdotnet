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
            Console.WriteLine("Initialize RabbitMQ connection successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection.");
            Environment.Exit(1);
        }
    }

    public bool DeclareExchange(string exchange, string exchangeType)
    {
        try
        {
            _channel.ExchangeDeclare(exchange: exchange, type: exchangeType, durable: true, autoDelete: false);
            return true; 
        }
        catch (OperationInterruptedException ex)
        {
            var shutdownReason = ex.ShutdownReason;
            if (shutdownReason?.ReplyCode == 406)
            {
                _logger.LogError("Exchange declaration failed: {Reason}", shutdownReason?.ReplyText);
                return false; 
            }
            else
            {
                _logger.LogError("An unexpected error occurred: {Message}", ex.Message);
                return false; 
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
    }
}

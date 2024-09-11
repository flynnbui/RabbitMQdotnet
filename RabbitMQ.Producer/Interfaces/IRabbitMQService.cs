namespace RabbitMQ.Producer.Interfaces
{
    public interface IRabbitMQService
    {
        void SetupConnection();
        bool DeclareExchange(string exchange, string exchangeType);
        void Dispose();
        Task PublishAsync(string message, string exchange, string routingKey);
    }
}

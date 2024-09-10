namespace RabbitMQ.Producer.Interfaces
{
    internal interface IRabbitMQService
    {
        void SetupConnection();
        void DeclareExchange(string exchange, string exchangeType);
        void Dispose();
        Task PublishAsync(string message, string exchange, string routingKey);
    }
}

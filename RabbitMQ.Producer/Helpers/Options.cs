using CommandLine;


namespace RabbitMQ.Producer.Helpers
{
    public class Options
    {

        [Option('e', "exchange", Required = true, HelpText = "Specify the exchange name.")]
        public string Exchange { get; set; }

        [Option('t', "type", Required = true, HelpText = "Specify the exchange type (direct, fanout, topic, headers).")]
        public string ExchangeType { get; set; }

        [Option('r', "routingkey", Required = true, HelpText = "Specify the routing key.")]
        public string RoutingKey { get; set; }

        [Option('m', "message", Required = false, HelpText = "Specify the message to be published.")]
        public string Message { get; set; }
    }
}

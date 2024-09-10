using Microsoft.Extensions.Logging;

namespace RabbitMQ.Producer.Helpers
{
    public static class UserInputHelper
    {
        public static string GetUserInput(string prompt)
        {
            Console.WriteLine(prompt);
            return Console.ReadLine();
        }

        public static string GetRoutingKeyInput()
        {
            return GetUserInput("Please enter the routing key:");
        }

        public static string GetExchangeTypeInput(ILogger logger)
        {
            string exchangeType = null;
            while (string.IsNullOrWhiteSpace(exchangeType) || !IsValidExchangeType(exchangeType))
            {
                exchangeType = GetUserInput("Please enter the exchange type (direct, fanout, topic, headers):");
                if (!IsValidExchangeType(exchangeType))
                {
                    logger.LogWarning("Invalid exchange type. Please use one of: direct, fanout, topic, headers.");
                }
            }
            return exchangeType;
        }

        public static string GetExchangeInput(ILogger logger)
        {
            string exchange = "";
            while (string.IsNullOrWhiteSpace(exchange))
            {
                exchange = GetUserInput("Please enter the exchange name:");
                if (string.IsNullOrWhiteSpace(exchange))
                {
                    logger.LogWarning("Exchange name cannot be empty.");
                }
            }
            return exchange;
        }

        public static bool IsValidExchangeType(string exchangeType)
        {
            var validTypes = new[] { "direct", "fanout", "topic", "headers" };
            return Array.Exists(validTypes, type => type.Equals(exchangeType, StringComparison.OrdinalIgnoreCase));
        }

        public static void DisplayHelp()
        {
            Console.WriteLine("RabbitMQ Publisher Help:");
            Console.WriteLine("Usage: [program] [options]");
            Console.WriteLine("Options:");
            Console.WriteLine("/h                : Show help");
            Console.WriteLine("Exchange Types:");
            Console.WriteLine(" - direct  : Routes messages to queues where the routing key matches the binding key exactly.");
            Console.WriteLine(" - fanout  : Broadcasts messages to all queues bound to the exchange, ignoring the routing key.");
            Console.WriteLine(" - topic   : Routes messages based on pattern matching between the routing key and binding key.");
            Console.WriteLine(" - headers : Routes messages based on matching headers rather than routing key.");
        }
    }
}

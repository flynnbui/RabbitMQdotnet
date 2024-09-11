using Microsoft.Extensions.Logging;
using RabbitMQ.Producer.Interfaces;
using System;

namespace RabbitMQ.Producer.Helpers
{
    public class UserInputHelper
    {
        private IRabbitMQService _rabbitService;
        private ILogger<UserInputHelper> _logger;

        public UserInputHelper(IRabbitMQService rabbitMQService, ILogger<UserInputHelper> logger)
        {
            _rabbitService = rabbitMQService;
            _logger = logger;
        }

        public string GetUserInput(string prompt)
        {
            Console.WriteLine(prompt);
            return Console.ReadLine();
        }

        public string GetRoutingKeyInput()
        {
            return GetUserInput("Please enter the routing key:");
        }

        public string GetExchangeTypeInput()
        {
            string exchangeType = null;
            while (string.IsNullOrWhiteSpace(exchangeType) || !IsValidExchangeType(exchangeType))
            {
                exchangeType = GetUserInput("Please enter the exchange type (direct, fanout, topic, headers):");
                if (!IsValidExchangeType(exchangeType))
                {
                    _logger.LogWarning("Invalid exchange type. Please use one of: direct, fanout, topic, headers.");
                }
            }
            return exchangeType;
        }

        public string GetExchangeInput()
        {
            string exchange = "";
            while (string.IsNullOrWhiteSpace(exchange))
            {
                exchange = GetUserInput("Please enter the exchange name:");
                if (string.IsNullOrWhiteSpace(exchange))
                {
                    _logger.LogWarning("Exchange name cannot be empty.");
                }
            }
            return exchange;
        }

        public bool IsValidExchangeType(string exchangeType)
        {
            var validTypes = new[] { "direct", "fanout", "topic", "headers" };
            return Array.Exists(validTypes, type => type.Equals(exchangeType, StringComparison.OrdinalIgnoreCase));
        }

        public static void DisplayHelp()
        {
            Console.WriteLine("Exchange Types:");
            Console.WriteLine(" - direct  : Routes messages to queues where the routing key matches the binding key exactly.");
            Console.WriteLine(" - fanout  : Broadcasts messages to all queues bound to the exchange, ignoring the routing key.");
            Console.WriteLine(" - topic   : Routes messages based on pattern matching between the routing key and binding key.");
            Console.WriteLine(" - headers : Routes messages based on matching headers rather than routing key.");
        }

        public async Task ContinueWithUserInput()
        {
            _rabbitService.SetupConnection();

            while (true)
            {
                string exchange = GetExchangeInput();
                string exchangeType = GetExchangeTypeInput();
                string routingKey = GetRoutingKeyInput();

                try
                {
                    _rabbitService.DeclareExchange(exchange, exchangeType);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to declare exchange.");
                    return;
                }

                Console.WriteLine("Please enter the message to publish:");
                string message = Console.ReadLine();

                await _rabbitService.PublishAsync(exchange, exchangeType, message);
                _rabbitService.Dispose();
            }
        }
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Producer.Helpers;
using RabbitMQ.Producer.Interfaces;

class Program
{
    static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        var rabbitMQService = host.Services.GetRequiredService<IRabbitMQService>();

        // Display help if "/h" is passed as a command-line argument
        if (args.Length == 1 && args[0] == "/h")
        {
            UserInputHelper.DisplayHelp();
            return;
        }

        // Setup RabbitMQ connection
        try
        {
            rabbitMQService.SetupConnection();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize RabbitMQ connection.");
            System.Environment.Exit(1);
        }

        // Ask user for the required inputs: exchange, exchange type, and routing key
        string exchange = UserInputHelper.GetExchangeInput(logger);
        string exchangeType = UserInputHelper.GetExchangeTypeInput(logger);
        string routingKey = UserInputHelper.GetRoutingKeyInput();

        // Attempt to declare the exchange, or catch the error if it already exists
        try
        {
            rabbitMQService.DeclareExchange(exchange, exchangeType);
            Console.WriteLine($"Exchange '{exchange}' declared successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exchange declaration failed. Likely due to it already existing with different attributes.");
            Console.WriteLine($"Warning: {ex.Message}");
            Console.WriteLine("You can still publish messages to the existing exchange.");
        }

        Console.WriteLine("Please enter the message to publish:");
        string message = Console.ReadLine();

        try
        {
            await rabbitMQService.PublishAsync(exchange, exchangeType, message);
            Console.WriteLine("Message published successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish the message.");
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            // Clean up resources
            rabbitMQService.Dispose();
        }
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(configure => configure.AddConsole());
                services.AddSingleton<IRabbitMQService, RabbitMQService>();
            });
}

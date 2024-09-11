using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Producer.Helpers;
using RabbitMQ.Producer.Interfaces;

class Program
{
    static async Task Main(string[] args)
    {
        var parserResult = Parser.Default.ParseArguments<Options>(args);
        var host = CreateHostBuilder(args).Build();
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        var rabbitService = host.Services.GetRequiredService<IRabbitMQService>();
        var userInputHelper = host.Services.GetRequiredService<UserInputHelper>();
        UserInputHelper.DisplayHelp();

        await parserResult
            .WithParsedAsync(async opts =>
            {
                rabbitService.SetupConnection();
                try
                {
                    rabbitService.DeclareExchange(opts.Exchange, opts.ExchangeType);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to declare exchange.");
                    return;
                }

                if (string.IsNullOrEmpty(opts.Message))
                {
                    Console.WriteLine("Please enter the message to publish:");
                    opts.Message = Console.ReadLine();
                }

                await rabbitService.PublishAsync(opts.Exchange, opts.ExchangeType, opts.Message);
                rabbitService.Dispose();
                await userInputHelper.ContinueWithUserInput();
            })
            .ConfigureAwait(false);

        if (parserResult.Tag == ParserResultType.NotParsed)
        {
            // Continue with user input for required fields
            await userInputHelper.ContinueWithUserInput();
        }
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((context, services) =>
        {
            services.AddLogging(configure => configure.AddConsole());
            services.AddSingleton<IRabbitMQService, RabbitMQService>();
            services.AddScoped<UserInputHelper>();
        });

}

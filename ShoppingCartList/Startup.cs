using System;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShoppingCartList;


[assembly: FunctionsStartup(typeof(Startup))]

namespace ShoppingCartList;

public class Startup : FunctionsStartup
{
    private static readonly IConfigurationRoot Configuration = new ConfigurationBuilder()
        .SetBasePath(Environment.CurrentDirectory)
        .AddJsonFile("appsettings.json", true)
        .AddEnvironmentVariables()
        .Build();

    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddSingleton(s =>
        {
            var connectionString = Configuration["CosmosDBConnection"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Please specify a valid CosmosDBConnection in the appSettings.json file or your Azure Functions SettingsF");
            }

            return new CosmosClientBuilder(connectionString).Build();
        });
    }
}
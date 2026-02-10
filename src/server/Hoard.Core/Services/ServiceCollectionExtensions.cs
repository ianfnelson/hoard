using Clarion;
using Hoard.Core.Services.YahooFinance;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hoard.Core.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHoardServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<QuoteService, YahooFinanceClient>();
        services.AddScoped<PriceService, YahooFinanceClient>();
        services.AddScoped<ClarionClient>(_ => ClarionClient.Create());

        // Azure Blob Storage
        var blobConnectionString = configuration.GetConnectionString("AzureBlobStorage")
            ?? throw new InvalidOperationException("Azure Blob Storage connection string not configured");
        var containerName = configuration["BlobStorage:ContainerName"] ?? "documents";

        services.AddSingleton<IBlobStorageService>(sp =>
            new BlobStorageService(blobConnectionString, containerName));

        return services;
    }
}
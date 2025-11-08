using Hoard.Core.Application.Transactions;
using Microsoft.Extensions.DependencyInjection;

namespace Hoard.Core.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHoardApplication(this IServiceCollection services)
    {
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<ICommandHandler<DeleteTransactionCommand>, DeleteTransactionHandler>();
        
        return services;
    }
}
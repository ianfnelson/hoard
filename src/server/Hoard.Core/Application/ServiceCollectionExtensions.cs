using Hoard.Core.Application.Transactions;
using Microsoft.Extensions.DependencyInjection;

namespace Hoard.Core.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHoardApplication(this IServiceCollection services)
    {
        services.AddScoped<IMediator, Mediator>();
        
        services.AddScoped<ITransactionFactory, TransactionFactory>();
        
        AddCommandAndQueryHandlers(services);
        
        return services;
    }

    private static IServiceCollection AddCommandAndQueryHandlers(IServiceCollection services)
    {
        services.Scan(s => s
            .FromAssemblyOf<ICommand>()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddScoped(typeof(ICommandHandler<>), typeof(TriggerCommandHandler<>));

        return services;
    }
}
using Hoard.Core.Application.Transactions;
using Microsoft.Extensions.DependencyInjection;

namespace Hoard.Core.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHoardApplication(this IServiceCollection services)
    {
        AddMapping(services);
        AddMediator(services);
        
        return services;
    }

    private static IServiceCollection AddMediator(IServiceCollection services)
    {
        services.AddScoped<IMediator, Mediator>();
        
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

    private static IServiceCollection AddMapping(IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblyOf<MapperFacade>()
            .AddClasses(classes => classes.AssignableTo(typeof(IMapper<,>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime()
        );
        services.AddSingleton<IMapper, MapperFacade>();
        
        return services;
    }
}
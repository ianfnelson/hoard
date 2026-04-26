using FluentValidation;
using Hoard.Core.Application.Alerts;
using Hoard.Core.Application.News;
using Hoard.Core.Application.Portfolios;
using Hoard.Core.Application.RunLog;
using Hoard.Core.Application.Transactions;
using Microsoft.Extensions.DependencyInjection;

namespace Hoard.Core.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHoardApplication(this IServiceCollection services)
    {
        AddMapping(services);
        AddMediator(services);
        AddValidation(services);
        AddServices(services);
        AddProviders(services);

        return services;
    }

    private static IServiceCollection AddProviders(IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<INewsAlertSuppressionRuleProvider, NewsAlertSuppressionRuleProvider>();
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
        // Register individual mappers
        services.Scan(scan => scan
            .FromAssemblyOf<MapperFacade>()
            .AddClasses(classes => classes.AssignableTo(typeof(IMapper<,>)))
            .AsSelf()
            .WithTransientLifetime()
        );

        // Register MapperFacade with factory that collects all mappers
        services.AddSingleton<IMapper>(sp =>
        {
            var assembly = typeof(MapperFacade).Assembly;
            var mapperTypes = assembly.GetTypes()
                .Where(t => t.GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapper<,>)));

            var mappers = mapperTypes.Select(sp.GetRequiredService).ToList();
            return new MapperFacade(mappers);
        });

        return services;
    }

    private static IServiceCollection AddValidation(IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<TransactionWriteDtoValidator>();
        return services;
    }

    private static IServiceCollection AddServices(IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<IRebalanceService, RebalanceService>();
        services.AddScoped<IRunLogService, RunLogService>();

        services.Scan(s => s
            .FromAssemblyOf<ICommand>()
            .AddClasses(c => c.AssignableTo<IPortfolioAlertEvaluator>())
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.Scan(s => s
            .FromAssemblyOf<ICommand>()
            .AddClasses(c => c.AssignableTo<ICloseOfDayPortfolioAlertEvaluator>())
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
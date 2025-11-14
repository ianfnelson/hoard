namespace Hoard.Core.Application;

public interface IMapper
{
    TDestination Map<TSource, TDestination>(TSource source);
    void Map<TSource, TDestination>(TSource source, TDestination destination);
}

public interface IMapper<TSource, TDestination>
{
    TDestination Map(TSource source);
    void Map(TSource source, TDestination destination);
}

public class MapperFacade : IMapper
{
    private readonly Dictionary<(Type, Type), object> _mappers;

    public MapperFacade(IEnumerable<object> mappers)
    {
        // Gather every registered IMapper<,>
        _mappers = mappers
            .Select(m => new
            {
                Mapper = m,
                Interface = m.GetType()
                    .GetInterfaces()
                    .First(i => i.IsGenericType 
                                && i.GetGenericTypeDefinition() == typeof(IMapper<,>))
            })
            .ToDictionary(
                x =>
                {
                    var args = x.Interface.GetGenericArguments();
                    return (args[0], args[1]);
                },
                x => x.Mapper);
    }

    public TDestination Map<TSource, TDestination>(TSource source)
    {
        var mapper = GetMapper<TSource, TDestination>();
        return mapper.Map(source);
    }

    public void Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        var mapper = GetMapper<TSource, TDestination>();
        mapper.Map(source, destination);
    }

    private IMapper<TSource, TDestination> GetMapper<TSource, TDestination>()
    {
        var key = (typeof(TSource), typeof(TDestination));

        if (_mappers.TryGetValue(key, out var mapperObj))
        {
            return (IMapper<TSource, TDestination>)mapperObj;
        }

        throw new InvalidOperationException(
            $"No mapper registered for {typeof(TSource).Name} → {typeof(TDestination).Name}");
    }
}
using Hoard.Core.Application;

namespace Hoard.Api.Models;

public sealed class SeriesOptions
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public Bucket? Bucket { get; set; }
}
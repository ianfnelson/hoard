using System.Text.Json.Serialization;

namespace Hoard.Core.Application.Portfolios;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RebalanceActionType
{
    Add,
    Reduce
}
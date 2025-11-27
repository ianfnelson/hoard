using Hoard.Messages;

namespace Hoard.Core.Extensions;

public static class PipelineModeExtensions
{
    public static bool IsNight(this PipelineMode mode) =>
        mode is PipelineMode.NightPreMidnight or PipelineMode.NightPostMidnight;

    public static bool IsDay(this PipelineMode mode) =>
        mode == PipelineMode.DaytimeReactive;

    public static bool IsBackfill(this PipelineMode mode) =>
        mode == PipelineMode.Backfill;
}
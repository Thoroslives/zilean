namespace Zilean.Database.Services.Common;

public static partial class StageTimingLogger
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Batch {BatchNumber} stage timings: parse={ParseMs}ms populate={PopulateMs}ms match={MatchMs}ms upsert={UpsertMs}ms")]
    public static partial void LogBatchStageTimings(
        this ILogger logger,
        int batchNumber,
        long parseMs,
        long populateMs,
        long matchMs,
        long upsertMs);
}

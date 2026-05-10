namespace Zilean.Shared.Features.Configuration;

public static class ParserConcurrency
{
    public const int MaxThreadPoolWorkers = 8;

    public static int ResolveMaxConcurrentTasks() =>
        Math.Min(Environment.ProcessorCount, MaxThreadPoolWorkers);
}

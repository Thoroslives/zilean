using Microsoft.Extensions.Logging;
using Zilean.Database.Services.Common;

namespace Zilean.Tests.Tests;

public class StageTimingLoggerTests
{
    [Fact]
    public void LogBatchStageTimings_IncludesAllFields()
    {
        var logger = new CapturingLogger();

        logger.LogBatchStageTimings(batchNumber: 7, parseMs: 1234, populateMs: 5000, matchMs: 200, upsertMs: 50);

        logger.Captures.Should().ContainSingle();
        var entry = logger.Captures[0];
        entry.Level.Should().Be(LogLevel.Information);
        entry.Message.Should().Contain("Batch 7");
        entry.Message.Should().Contain("parse=1234ms");
        entry.Message.Should().Contain("populate=5000ms");
        entry.Message.Should().Contain("match=200ms");
        entry.Message.Should().Contain("upsert=50ms");
    }

    [Fact]
    public void LogBatchStageTimings_LogsAtInformationLevel()
    {
        var logger = new CapturingLogger();

        logger.LogBatchStageTimings(batchNumber: 1, parseMs: 0, populateMs: 0, matchMs: 0, upsertMs: 0);

        logger.Captures.Should().ContainSingle();
        logger.Captures[0].Level.Should().Be(LogLevel.Information);
    }

    private sealed class CapturingLogger : ILogger
    {
        public List<(LogLevel Level, string Message)> Captures { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = formatter(state, exception);
            Captures.Add((logLevel, message));
        }
    }
}

using System.Reflection;
using Microsoft.Extensions.Logging;
using Zilean.Database.Services.FuzzyString;
using Zilean.Database.Services.Lucene;

namespace Zilean.Tests.Tests;

public class MatcherLoggerLevelTests
{
    [Theory]
    [InlineData(typeof(ImdbLuceneMatchingServiceLogger), "TorrentUpdated")]
    [InlineData(typeof(ImdbLuceneMatchingServiceLogger), "TorrentRetained")]
    [InlineData(typeof(ImdbLuceneMatchingServiceLogger), "NoSuitableMatchFound")]
    [InlineData(typeof(ImdbFuzzyStringMatchingServiceLogger), "TorrentUpdated")]
    [InlineData(typeof(ImdbFuzzyStringMatchingServiceLogger), "TorrentRetained")]
    [InlineData(typeof(ImdbFuzzyStringMatchingServiceLogger), "NoSuitableMatchFound")]
    public void PerTorrentLogger_IsTaggedDebug(Type loggerType, string methodName)
    {
        var method = loggerType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static)!;
        var attribute = method.GetCustomAttribute<LoggerMessageAttribute>()!;
        attribute.Level.Should().Be(LogLevel.Debug,
            $"{loggerType.Name}.{methodName} is on the per-torrent hot path; it must be Debug so default Serilog Information level filters it out.");
    }
}

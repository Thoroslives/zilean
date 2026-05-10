namespace Zilean.Shared.Features.Configuration;

public class ParsingConfiguration
{
    public int BatchSize { get; set; } = 5000;

    /// <summary>
    /// When true, emits a log line for every parsed torrent title (Python loguru) and
    /// keeps the per-match C# matcher logs visible. Default false because the per-title
    /// log calls dominate parse-stage wall time on long ingests.
    /// Operators wanting full per-torrent C# matcher visibility can also set Serilog
    /// `MinimumLevel: Debug` independently.
    /// </summary>
    public bool VerboseLogging { get; set; } = false;
}

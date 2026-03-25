namespace Zilean.Shared.Features.Configuration;

public class ZileanConfiguration
{
    private static readonly JsonSerializerOptions? _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = null,
    };

    public string? ApiKey { get; set; } = Utilities.ApiKey.Generate();
    public bool FirstRun { get; set; } = true;
    public bool EnableDashboard { get; set; } = false;
    public DmmConfiguration Dmm { get; set; } = new();
    public TorznabConfiguration Torznab { get; set; } = new();
    public DatabaseConfiguration Database { get; set; } = new();
    public TorrentsConfiguration Torrents { get; set; } = new();
    public ImdbConfiguration Imdb { get; set; } = new();
    public IngestionConfiguration Ingestion { get; set; } = new();
    public ParsingConfiguration Parsing { get; set; } = new();

    public static void EnsureExists()
    {
        var settingsFilePath = Path.Combine(AppContext.BaseDirectory, ConfigurationLiterals.ConfigurationFolder, ConfigurationLiterals.SettingsConfigFilename);
        if (!File.Exists(settingsFilePath))
        {
            File.WriteAllText(settingsFilePath, DefaultConfigurationContents());
        }
    }

    /// <summary>
    /// Validates the configuration and returns a list of error messages. Empty list means valid.
    /// </summary>
    public List<string> Validate()
    {
        var errors = new List<string>();

        if (Dmm.MaxFilteredResults <= 0)
            errors.Add("Dmm.MaxFilteredResults must be greater than 0");

        if (Dmm.MinimumScoreMatch is < 0 or > 1)
            errors.Add("Dmm.MinimumScoreMatch must be between 0 and 1");

        if (Dmm.MinimumReDownloadIntervalMinutes < 0)
            errors.Add("Dmm.MinimumReDownloadIntervalMinutes must be non-negative");

        if (!IsValidCronExpression(Dmm.ScrapeSchedule))
            errors.Add($"Dmm.ScrapeSchedule '{Dmm.ScrapeSchedule}' is not a valid cron expression");

        if (!IsValidCronExpression(Ingestion.ScrapeSchedule))
            errors.Add($"Ingestion.ScrapeSchedule '{Ingestion.ScrapeSchedule}' is not a valid cron expression");

        if (Parsing.BatchSize <= 0)
            errors.Add("Parsing.BatchSize must be greater than 0");

        if (string.IsNullOrWhiteSpace(Database.ConnectionString))
            errors.Add("Database.ConnectionString is empty — check POSTGRES_* or Zilean__Database__ConnectionString env vars");

        return errors;
    }

    private static bool IsValidCronExpression(string? cron)
    {
        if (string.IsNullOrWhiteSpace(cron))
            return false;

        var parts = cron.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 5;
    }

    private static string DefaultConfigurationContents()
    {
        var mainSettings = new Dictionary<string, object>
        {
            [ConfigurationLiterals.MainSettingsSectionName] = new ZileanConfiguration(),
        };

        return JsonSerializer.Serialize(mainSettings, _jsonSerializerOptions);
    }
}

using Npgsql;

namespace Zilean.ApiService.Features.HealthChecks;

public static class HealthCheckEndpoints
{
    private const string GroupName = "healthchecks";
    private const string Ping = "/ping";
    private const string Ready = "/ready";

    public static WebApplication MapHealthCheckEndpoints(this WebApplication app)
    {
        app.MapGroup(GroupName)
            .WithTags(GroupName)
            .HealthChecks()
            .DisableAntiforgery()
            .AllowAnonymous();

        return app;
    }

    private static RouteGroupBuilder HealthChecks(this RouteGroupBuilder group)
    {
        group.MapGet(Ping, RespondPong);
        group.MapGet(Ready, CheckReadiness);

        return group;
    }

    private static string RespondPong(HttpContext context) => $"[{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}]: Pong!";

    private static async Task<IResult> CheckReadiness(
        ZileanConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<ReadinessCheck> logger)
    {
        var databaseHealthy = false;

        try
        {
            await using var connection = new NpgsqlConnection(configuration.Database.ConnectionString);
            await connection.OpenAsync();
            databaseHealthy = true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database health check failed");
        }

        bool? pythonAvailable = null;

        if (configuration.EnableDashboard)
        {
            var ptn = serviceProvider.GetService<ParseTorrentNameService>();
            pythonAvailable = ptn?.IsAvailable ?? false;
        }

        var status = !databaseHealthy
            ? "unhealthy"
            : pythonAvailable == false
                ? "degraded"
                : "healthy";

        var statusCode = databaseHealthy ? 200 : 503;

        return Results.Json(new
        {
            status,
            database = databaseHealthy,
            pythonAvailable,
            timestamp = DateTime.UtcNow,
        }, statusCode: statusCode);
    }
}

internal sealed class ReadinessCheck;

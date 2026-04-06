using Microsoft.EntityFrameworkCore.Migrations;
using Zilean.Database.Functions;

#nullable disable

namespace Zilean.Database.Migrations;

/// <inheritdoc />
public partial class SearchV7BookThreshold : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(SearchTorrentsMetaV6.Remove);
        migrationBuilder.Sql(SearchTorrentsMetaV6.Create);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Down is a no-op - the function signature is unchanged,
        // only the threshold logic differs
    }
}

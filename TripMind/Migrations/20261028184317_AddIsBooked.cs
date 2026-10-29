using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripMind.Migrations
{
    /// <inheritdoc />
    public partial class AddIsBooked : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Trips\" ADD COLUMN IF NOT EXISTS \"From\" text NOT NULL DEFAULT '';");
            migrationBuilder.Sql("ALTER TABLE \"Trips\" ADD COLUMN IF NOT EXISTS \"IsBooked\" boolean NOT NULL DEFAULT false;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Trips\" DROP COLUMN IF EXISTS \"IsBooked\";");
        }
    }
}

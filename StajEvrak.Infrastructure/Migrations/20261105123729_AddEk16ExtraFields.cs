using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajEvrak.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEk16ExtraFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HealthStatus",
                table: "Ek16Data",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviousInternshipDays",
                table: "Ek16Data",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HealthStatus",
                table: "Ek16Data");

            migrationBuilder.DropColumn(
                name: "PreviousInternshipDays",
                table: "Ek16Data");
        }
    }
}

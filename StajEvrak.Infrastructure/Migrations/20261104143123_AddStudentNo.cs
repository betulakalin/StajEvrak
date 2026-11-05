using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajEvrak.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentNo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_StudentRecords_StudentRecordId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_StudentRecordId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StudentRecordId",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "StudentNo",
                table: "Users",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StudentNo",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "StudentRecordId",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_StudentRecordId",
                table: "Users",
                column: "StudentRecordId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_StudentRecords_StudentRecordId",
                table: "Users",
                column: "StudentRecordId",
                principalTable: "StudentRecords",
                principalColumn: "Id");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VIPP.Migrations
{
    /// <inheritdoc />
    public partial class DateTimeForDateOfBirth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "datetime2",
                oldClrType: typeof(DateTime),
                oldType: "datetime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "datetime",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }
    }
}

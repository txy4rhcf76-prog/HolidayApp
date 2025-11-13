using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HolidayApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Holidays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountryCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LocalName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Global = table.Column<bool>(type: "bit", nullable: false),
                    Counties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LaunchYear = table.Column<int>(type: "int", nullable: true),
                    Types = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holidays", x => x.Id);
                    table.UniqueConstraint("AK_Holidays_CountryCode_Date_LocalName", x => new { x.CountryCode, x.Date, x.LocalName });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_CountryCode",
                table: "Holidays",
                column: "CountryCode");

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_CountryCode_Date",
                table: "Holidays",
                columns: new[] { "CountryCode", "Date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Holidays");
        }
    }
}

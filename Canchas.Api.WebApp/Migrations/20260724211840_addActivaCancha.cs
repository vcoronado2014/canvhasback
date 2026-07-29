using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Canchas.Api.WebApp.Migrations
{
    /// <inheritdoc />
    public partial class addActivaCancha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activa",
                table: "Canchas",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "IntervaloMinutos",
                table: "Canchas",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Activa",
                table: "Canchas");

            migrationBuilder.DropColumn(
                name: "IntervaloMinutos",
                table: "Canchas");
        }
    }
}

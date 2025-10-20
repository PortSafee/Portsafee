using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CondominioEntregas.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnidadeFieldsFromCondominio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bloco",
                table: "Condominios");

            migrationBuilder.DropColumn(
                name: "CEP",
                table: "Condominios");

            migrationBuilder.DropColumn(
                name: "NumeroApartamento",
                table: "Condominios");

            migrationBuilder.DropColumn(
                name: "NumeroCasa",
                table: "Condominios");

            migrationBuilder.DropColumn(
                name: "Rua",
                table: "Condominios");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bloco",
                table: "Condominios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CEP",
                table: "Condominios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroApartamento",
                table: "Condominios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumeroCasa",
                table: "Condominios",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rua",
                table: "Condominios",
                type: "text",
                nullable: true);
        }
    }
}

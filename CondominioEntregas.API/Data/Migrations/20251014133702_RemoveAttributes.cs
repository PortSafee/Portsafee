using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CondominioEntregas.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cidade",
                table: "Condominios");

            migrationBuilder.DropColumn(
                name: "Endereco",
                table: "Condominios");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Condominios");

            migrationBuilder.RenameColumn(
                name: "Senha",
                table: "Moradores",
                newName: "SenhaHash");

            migrationBuilder.RenameColumn(
                name: "Nome",
                table: "Condominios",
                newName: "NomeDoCondominio");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SenhaHash",
                table: "Moradores",
                newName: "Senha");

            migrationBuilder.RenameColumn(
                name: "NomeDoCondominio",
                table: "Condominios",
                newName: "Nome");

            migrationBuilder.AddColumn<string>(
                name: "Cidade",
                table: "Condominios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Endereco",
                table: "Condominios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Condominios",
                type: "text",
                nullable: true);
        }
    }
}

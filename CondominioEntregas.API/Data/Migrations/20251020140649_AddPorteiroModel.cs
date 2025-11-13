using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CondominioEntregas.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPorteiroModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Condominios_CondominioId",
                table: "Usuarios");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Condominios_CondominioId1",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Condominios");

            migrationBuilder.RenameColumn(
                name: "Bloco",
                table: "UnidadesApartamento",
                newName: "Torre");

            migrationBuilder.AddColumn<string>(
                name: "Matricula",
                table: "Usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Morador_Telefone",
                table: "Usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "Condominios",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Condominios_CondominioId",
                table: "Usuarios",
                column: "CondominioId",
                principalTable: "Condominios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Condominios_CondominioId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Matricula",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Morador_Telefone",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Condominios");

            migrationBuilder.RenameColumn(
                name: "Torre",
                table: "UnidadesApartamento",
                newName: "Bloco");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Condominios",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Condominios_CondominioId",
                table: "Usuarios",
                column: "CondominioId",
                principalTable: "Condominios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Condominios_CondominioId1",
                table: "Usuarios",
                column: "CondominioId",
                principalTable: "Condominios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

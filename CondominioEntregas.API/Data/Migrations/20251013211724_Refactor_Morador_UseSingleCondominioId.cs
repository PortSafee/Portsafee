using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CondominioEntregas.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class Refactor_Morador_UseSingleCondominioId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Moradores_Condominios_CondApartamentoId",
                table: "Moradores");

            migrationBuilder.DropForeignKey(
                name: "FK_Moradores_Condominios_CondCasaId",
                table: "Moradores");

            migrationBuilder.DropIndex(
                name: "IX_Moradores_CondApartamentoId",
                table: "Moradores");

            migrationBuilder.DropIndex(
                name: "IX_Moradores_CondCasaId",
                table: "Moradores");

            migrationBuilder.DropColumn(
                name: "CondApartamentoId",
                table: "Moradores");

            migrationBuilder.DropColumn(
                name: "CondCasaId",
                table: "Moradores");

            migrationBuilder.DropColumn(
                name: "Complemento",
                table: "Condominios");

            migrationBuilder.AlterColumn<int>(
                name: "NumeroCasa",
                table: "Condominios",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CondApartamentoId",
                table: "Moradores",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CondCasaId",
                table: "Moradores",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroCasa",
                table: "Condominios",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Complemento",
                table: "Condominios",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Moradores_CondApartamentoId",
                table: "Moradores",
                column: "CondApartamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Moradores_CondCasaId",
                table: "Moradores",
                column: "CondCasaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Moradores_Condominios_CondApartamentoId",
                table: "Moradores",
                column: "CondApartamentoId",
                principalTable: "Condominios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Moradores_Condominios_CondCasaId",
                table: "Moradores",
                column: "CondCasaId",
                principalTable: "Condominios",
                principalColumn: "Id");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CondominioEntregas.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedataBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Condominios_CondominioId1",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_CondominioId1",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "CondominioId1",
                table: "Usuarios");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Condominios_CondominioId1",
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
                name: "FK_Usuarios_Condominios_CondominioId1",
                table: "Usuarios");

            migrationBuilder.AddColumn<int>(
                name: "CondominioId1",
                table: "Usuarios",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_CondominioId1",
                table: "Usuarios",
                column: "CondominioId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Condominios_CondominioId1",
                table: "Usuarios",
                column: "CondominioId1",
                principalTable: "Condominios",
                principalColumn: "Id");
        }
    }
}

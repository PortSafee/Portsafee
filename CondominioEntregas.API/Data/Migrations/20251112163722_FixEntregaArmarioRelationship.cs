using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CondominioEntregas.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixEntregaArmarioRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entregas_Armarios_ArmarioId",
                table: "Entregas");

            migrationBuilder.DropIndex(
                name: "IX_Entregas_ArmarioId",
                table: "Entregas");

            migrationBuilder.DropColumn(
                name: "ArmarioId",
                table: "Entregas");

            migrationBuilder.CreateIndex(
                name: "IX_Entregas_ArmariumId",
                table: "Entregas",
                column: "ArmariumId");

            migrationBuilder.AddForeignKey(
                name: "FK_Entregas_Armarios_ArmariumId",
                table: "Entregas",
                column: "ArmariumId",
                principalTable: "Armarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entregas_Armarios_ArmariumId",
                table: "Entregas");

            migrationBuilder.DropIndex(
                name: "IX_Entregas_ArmariumId",
                table: "Entregas");

            migrationBuilder.AddColumn<int>(
                name: "ArmarioId",
                table: "Entregas",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Entregas_ArmarioId",
                table: "Entregas",
                column: "ArmarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Entregas_Armarios_ArmarioId",
                table: "Entregas",
                column: "ArmarioId",
                principalTable: "Armarios",
                principalColumn: "Id");
        }
    }
}

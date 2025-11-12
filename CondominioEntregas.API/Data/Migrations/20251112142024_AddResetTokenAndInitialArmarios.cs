using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CondominioEntregas.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddResetTokenAndInitialArmarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResetToken",
                table: "Usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetTokenExpiracao",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            // Adicionar 5 armários de teste
            migrationBuilder.InsertData(
                table: "Armarios",
                columns: new[] { "Numero", "Status", "UltimaAbertura", "UltimoFechamento" },
                values: new object[,]
                {
                    { "1", 0, null, null },
                    { "2", 0, null, null },
                    { "3", 0, null, null },
                    { "4", 0, null, null },
                    { "5", 0, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResetToken",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "ResetTokenExpiracao",
                table: "Usuarios");
        }
    }
}

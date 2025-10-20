using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CondominioEntregas.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddingNewTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Moradores_Condominios_CondominioId",
                table: "Moradores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Moradores",
                table: "Moradores");

            migrationBuilder.RenameTable(
                name: "Moradores",
                newName: "Usuarios");

            migrationBuilder.RenameIndex(
                name: "IX_Moradores_CondominioId",
                table: "Usuarios",
                newName: "IX_Usuarios_CondominioId");

            migrationBuilder.AlterColumn<int>(
                name: "CondominioId",
                table: "Usuarios",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "CPF",
                table: "Usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCriacao",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "Usuarios",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Condominios_CondominioId",
                table: "Usuarios",
                column: "CondominioId",
                principalTable: "Condominios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Condominios_CondominioId",
                table: "Usuarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "CPF",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "DataCriacao",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Usuarios");

            migrationBuilder.RenameTable(
                name: "Usuarios",
                newName: "Moradores");

            migrationBuilder.RenameIndex(
                name: "IX_Usuarios_CondominioId",
                table: "Moradores",
                newName: "IX_Moradores_CondominioId");

            migrationBuilder.AlterColumn<int>(
                name: "CondominioId",
                table: "Moradores",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Moradores",
                table: "Moradores",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Moradores_Condominios_CondominioId",
                table: "Moradores",
                column: "CondominioId",
                principalTable: "Condominios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

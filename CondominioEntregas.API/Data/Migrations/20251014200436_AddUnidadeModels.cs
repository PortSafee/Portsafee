using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CondominioEntregas.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUnidadeModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UnidadeId",
                table: "Usuarios",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Unidade",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CondominioId = table.Column<int>(type: "integer", nullable: false),
                    MoradorId = table.Column<int>(type: "integer", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unidade", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Unidade_Condominios_CondominioId",
                        column: x => x.CondominioId,
                        principalTable: "Condominios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnidadesApartamento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Bloco = table.Column<string>(type: "text", nullable: false),
                    NumeroApartamento = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnidadesApartamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnidadesApartamento_Unidade_Id",
                        column: x => x.Id,
                        principalTable: "Unidade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UnidadesCasa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Rua = table.Column<string>(type: "text", nullable: false),
                    NumeroCasa = table.Column<int>(type: "integer", nullable: false),
                    CEP = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnidadesCasa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnidadesCasa_Unidade_Id",
                        column: x => x.Id,
                        principalTable: "Unidade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_UnidadeId",
                table: "Usuarios",
                column: "UnidadeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Unidade_CondominioId",
                table: "Unidade",
                column: "CondominioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Unidade_UnidadeId",
                table: "Usuarios",
                column: "UnidadeId",
                principalTable: "Unidade",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Unidade_UnidadeId",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "UnidadesApartamento");

            migrationBuilder.DropTable(
                name: "UnidadesCasa");

            migrationBuilder.DropTable(
                name: "Unidade");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_UnidadeId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "UnidadeId",
                table: "Usuarios");
        }
    }
}

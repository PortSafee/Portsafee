using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CondominioEntregas.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Armarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UltimaAbertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UltimoFechamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Armarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Condominios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: true),
                    Endereco = table.Column<string>(type: "text", nullable: true),
                    Cidade = table.Column<string>(type: "text", nullable: true),
                    Estado = table.Column<string>(type: "text", nullable: true),
                    CEP = table.Column<string>(type: "text", nullable: true),
                    Discriminator = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    Bloco = table.Column<string>(type: "text", nullable: true),
                    NumeroApartamento = table.Column<string>(type: "text", nullable: true),
                    Rua = table.Column<string>(type: "text", nullable: true),
                    NumeroCasa = table.Column<string>(type: "text", nullable: true),
                    Complemento = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Condominios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Entregas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NomeDestinatario = table.Column<string>(type: "text", nullable: false),
                    NumeroCasa = table.Column<string>(type: "text", nullable: false),
                    EnderecoGerado = table.Column<string>(type: "text", nullable: true),
                    ArmariumId = table.Column<int>(type: "integer", nullable: true),
                    ArmarioId = table.Column<int>(type: "integer", nullable: true),
                    CodigoEntrega = table.Column<string>(type: "text", nullable: true),
                    SenhaAcesso = table.Column<string>(type: "text", nullable: true),
                    DataHoraRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataHoraRetirada = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TelefoneWhatsApp = table.Column<string>(type: "text", nullable: true),
                    MensagemEnviada = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entregas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Entregas_Armarios_ArmarioId",
                        column: x => x.ArmarioId,
                        principalTable: "Armarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Moradores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    CondominioId = table.Column<int>(type: "integer", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Senha = table.Column<string>(type: "text", nullable: false),
                    Telefone = table.Column<string>(type: "text", nullable: true),
                    Photo = table.Column<string>(type: "text", nullable: true),
                    CondApartamentoId = table.Column<int>(type: "integer", nullable: true),
                    CondCasaId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Moradores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Moradores_Condominios_CondApartamentoId",
                        column: x => x.CondApartamentoId,
                        principalTable: "Condominios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Moradores_Condominios_CondCasaId",
                        column: x => x.CondCasaId,
                        principalTable: "Condominios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Moradores_Condominios_CondominioId",
                        column: x => x.CondominioId,
                        principalTable: "Condominios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entregas_ArmarioId",
                table: "Entregas",
                column: "ArmarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Moradores_CondApartamentoId",
                table: "Moradores",
                column: "CondApartamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Moradores_CondCasaId",
                table: "Moradores",
                column: "CondCasaId");

            migrationBuilder.CreateIndex(
                name: "IX_Moradores_CondominioId",
                table: "Moradores",
                column: "CondominioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Entregas");

            migrationBuilder.DropTable(
                name: "Moradores");

            migrationBuilder.DropTable(
                name: "Armarios");

            migrationBuilder.DropTable(
                name: "Condominios");
        }
    }
}

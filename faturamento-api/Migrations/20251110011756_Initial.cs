using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace faturamento_api.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "nota_numeracao_sequence");

            migrationBuilder.CreateTable(
                name: "ChaveIdempotencia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Chave = table.Column<string>(type: "text", nullable: false),
                    Rota = table.Column<string>(type: "text", nullable: false),
                    StatusHttp = table.Column<int>(type: "integer", nullable: false),
                    RespostaJson = table.Column<string>(type: "text", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChaveIdempotencia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Nota",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('nota_numeracao_sequence')"),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nota", x => x.Id);
                    table.CheckConstraint("CK_Nota_Status", "\"Status\" IN ('Aberta','Fechada')");
                });

            migrationBuilder.CreateTable(
                name: "NotaItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NotaId = table.Column<int>(type: "integer", nullable: false),
                    ProdutoId = table.Column<int>(type: "integer", nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false),
                    Preco = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotaItem", x => x.Id);
                    table.CheckConstraint("CK_NotaItem_Quantidade_Pos", "\"Quantidade\" > 0");
                    table.ForeignKey(
                        name: "FK_NotaItem_Nota_NotaId",
                        column: x => x.NotaId,
                        principalTable: "Nota",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChaveIdempotencia_Chave_Rota",
                table: "ChaveIdempotencia",
                columns: new[] { "Chave", "Rota" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChaveIdempotencia_DataCriacao",
                table: "ChaveIdempotencia",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_Nota_Numero",
                table: "Nota",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotaItem_NotaId_ProdutoId",
                table: "NotaItem",
                columns: new[] { "NotaId", "ProdutoId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChaveIdempotencia");

            migrationBuilder.DropTable(
                name: "NotaItem");

            migrationBuilder.DropTable(
                name: "Nota");

            migrationBuilder.DropSequence(
                name: "nota_numeracao_sequence");
        }
    }
}

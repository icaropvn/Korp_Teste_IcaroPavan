using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace faturamento_api.Migrations
{
    /// <inheritdoc />
    public partial class AjustesSchemaFaturamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChaveIdempotencia");

            migrationBuilder.DropTable(
                name: "NotaItem");

            migrationBuilder.CreateTable(
                name: "ItemNota",
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
                    table.PrimaryKey("PK_ItemNota", x => x.Id);
                    table.CheckConstraint("CK_NotaItem_Quantidade_Pos", "\"Quantidade\" > 0");
                    table.ForeignKey(
                        name: "FK_ItemNota_Nota_NotaId",
                        column: x => x.NotaId,
                        principalTable: "Nota",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemNota_NotaId_ProdutoId",
                table: "ItemNota",
                columns: new[] { "NotaId", "ProdutoId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemNota");

            migrationBuilder.CreateTable(
                name: "ChaveIdempotencia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Chave = table.Column<string>(type: "text", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    RespostaJson = table.Column<string>(type: "text", nullable: false),
                    Rota = table.Column<string>(type: "text", nullable: false),
                    StatusHttp = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChaveIdempotencia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotaItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NotaId = table.Column<int>(type: "integer", nullable: false),
                    Preco = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ProdutoId = table.Column<int>(type: "integer", nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false)
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
                name: "IX_NotaItem_NotaId_ProdutoId",
                table: "NotaItem",
                columns: new[] { "NotaId", "ProdutoId" });
        }
    }
}

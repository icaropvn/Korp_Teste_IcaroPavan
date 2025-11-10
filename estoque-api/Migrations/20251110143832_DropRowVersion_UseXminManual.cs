using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace estoque_api.Migrations
{
    /// <inheritdoc />
    public partial class DropRowVersion_UseXminManual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Produto");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Produto",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Produto");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Produto",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}

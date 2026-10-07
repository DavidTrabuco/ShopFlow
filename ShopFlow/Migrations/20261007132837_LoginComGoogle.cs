using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopFlow.Migrations
{
    /// <inheritdoc />
    public partial class LoginComGoogle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "senha_hash",
                table: "usuarios",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "google_id",
                table: "usuarios",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_google_id",
                table: "usuarios",
                column: "google_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_usuarios_google_id",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "google_id",
                table: "usuarios");

            migrationBuilder.AlterColumn<string>(
                name: "senha_hash",
                table: "usuarios",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}

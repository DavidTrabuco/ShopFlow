using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopFlow.Migrations
{
    /// <inheritdoc />
    public partial class AtributosVarianteTabela : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "atributos_variante",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    valor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    variante_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_atributos_variante", x => x.id);
                    table.ForeignKey(
                        name: "fk_atributos_variante_variantes_variante_id",
                        column: x => x.variante_id,
                        principalTable: "variantes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_atributos_variante_variante_id_nome",
                table: "atributos_variante",
                columns: new[] { "variante_id", "nome" },
                unique: true);

            // Preserva os atributos que já estavam no jsonb antes de apagar a coluna
            migrationBuilder.Sql(@"
                INSERT INTO atributos_variante (id, nome, valor, variante_id)
                SELECT gen_random_uuid(), a.key, a.value, v.id
                FROM variantes v, jsonb_each_text(v.atributos) AS a;");

            migrationBuilder.DropColumn(
                name: "atributos",
                table: "variantes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "atributos_variante");

            migrationBuilder.AddColumn<string>(
                name: "atributos",
                table: "variantes",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }
    }
}

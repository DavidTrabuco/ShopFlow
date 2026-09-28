using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopFlow.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categorias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    categoria_pai_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ativa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categorias", x => x.id);
                    table.ForeignKey(
                        name: "fk_categorias_categorias_categoria_pai_id",
                        column: x => x.categoria_pai_id,
                        principalTable: "categorias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    email = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    senha_hash = table.Column<string>(type: "text", nullable: false),
                    cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    papel = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    email_confirmado = table.Column<bool>(type: "boolean", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "produtos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    categoria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    slug = table.Column<string>(type: "character varying(170)", maxLength: 170, nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    marca = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_produtos", x => x.id);
                    table.ForeignKey(
                        name: "fk_produtos_categorias_categoria_id",
                        column: x => x.categoria_id,
                        principalTable: "categorias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "carrinhos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    token_anonimo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cupom_id = table.Column<Guid>(type: "uuid", nullable: true),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_carrinhos", x => x.id);
                    table.ForeignKey(
                        name: "fk_carrinhos_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "enderecos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    apelido = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    cep = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    logradouro = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    numero = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    complemento = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    bairro = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    cidade = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    uf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    principal = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_enderecos", x => x.id);
                    table.ForeignKey(
                        name: "fk_enderecos_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "variantes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sku = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    atributos = table.Column<string>(type: "jsonb", nullable: false),
                    preco = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    preco_promocional = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    peso_gramas = table.Column<int>(type: "integer", nullable: false),
                    ativa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_variantes", x => x.id);
                    table.CheckConstraint("ck_variantes_preco", "preco > 0");
                    table.CheckConstraint("ck_variantes_preco_promocional", "preco_promocional IS NULL OR preco_promocional < preco");
                    table.ForeignKey(
                        name: "fk_variantes_produtos_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "estoques",
                columns: table => new
                {
                    variante_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade_fisica = table.Column<int>(type: "integer", nullable: false),
                    quantidade_reservada = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estoques", x => x.variante_id);
                    table.CheckConstraint("ck_estoques_fisica", "quantidade_fisica >= 0");
                    table.CheckConstraint("ck_estoques_reservada", "quantidade_reservada >= 0 AND quantidade_reservada <= quantidade_fisica");
                    table.ForeignKey(
                        name: "fk_estoques_variantes_variante_id",
                        column: x => x.variante_id,
                        principalTable: "variantes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "imagens_produto",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    variante_id = table.Column<Guid>(type: "uuid", nullable: true),
                    url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_imagens_produto", x => x.id);
                    table.ForeignKey(
                        name: "fk_imagens_produto_produtos_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_imagens_produto_variantes_variante_id",
                        column: x => x.variante_id,
                        principalTable: "variantes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "itens_carrinho",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    carrinho_id = table.Column<Guid>(type: "uuid", nullable: false),
                    variante_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_itens_carrinho", x => x.id);
                    table.CheckConstraint("ck_itens_carrinho_quantidade", "quantidade BETWEEN 1 AND 10");
                    table.ForeignKey(
                        name: "fk_itens_carrinho_carrinhos_carrinho_id",
                        column: x => x.carrinho_id,
                        principalTable: "carrinhos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_itens_carrinho_variantes_variante_id",
                        column: x => x.variante_id,
                        principalTable: "variantes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_carrinhos_token_anonimo",
                table: "carrinhos",
                column: "token_anonimo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_carrinhos_usuario_id",
                table: "carrinhos",
                column: "usuario_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_categorias_categoria_pai_id",
                table: "categorias",
                column: "categoria_pai_id");

            migrationBuilder.CreateIndex(
                name: "ix_categorias_slug",
                table: "categorias",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_enderecos_usuario_id",
                table: "enderecos",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_imagens_produto_produto_id",
                table: "imagens_produto",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_imagens_produto_variante_id",
                table: "imagens_produto",
                column: "variante_id");

            migrationBuilder.CreateIndex(
                name: "ix_itens_carrinho_carrinho_id_variante_id",
                table: "itens_carrinho",
                columns: new[] { "carrinho_id", "variante_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_itens_carrinho_variante_id",
                table: "itens_carrinho",
                column: "variante_id");

            migrationBuilder.CreateIndex(
                name: "ix_produtos_categoria_id",
                table: "produtos",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "ix_produtos_slug",
                table: "produtos",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_cpf",
                table: "usuarios",
                column: "cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_email",
                table: "usuarios",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_variantes_produto_id",
                table: "variantes",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_variantes_sku",
                table: "variantes",
                column: "sku",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "enderecos");

            migrationBuilder.DropTable(
                name: "estoques");

            migrationBuilder.DropTable(
                name: "imagens_produto");

            migrationBuilder.DropTable(
                name: "itens_carrinho");

            migrationBuilder.DropTable(
                name: "carrinhos");

            migrationBuilder.DropTable(
                name: "variantes");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "produtos");

            migrationBuilder.DropTable(
                name: "categorias");
        }
    }
}

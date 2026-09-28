using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ShopFlow.Domain.Entidades;

namespace ShopFlow.Infrastruture.Data
{
    public class ShopFlowDbContext : DbContext
    {
        public ShopFlowDbContext(DbContextOptions<ShopFlowDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Endereco> Enderecos => Set<Endereco>();
        public DbSet<Categoria> Categorias => Set<Categoria>();
        public DbSet<Produto> Produtos => Set<Produto>();
        public DbSet<Variante> Variantes => Set<Variante>();
        public DbSet<ImagemProduto> ImagensProduto => Set<ImagemProduto>();
        public DbSet<Estoque> Estoques => Set<Estoque>();
        public DbSet<Carrinho> Carrinhos => Set<Carrinho>();
        public DbSet<ItemCarrinho> ItensCarrinho => Set<ItemCarrinho>();

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<decimal>().HavePrecision(12, 2);
            configurationBuilder.Properties<Enum>().HaveConversion<string>().HaveMaxLength(30);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(120);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(180);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.SenhaHash).IsRequired();
                entity.Property(e => e.Cpf).HasMaxLength(11);
                entity.HasIndex(e => e.Cpf).IsUnique();
                entity.Property(e => e.Telefone).HasMaxLength(20);
            });

            modelBuilder.Entity<Endereco>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Apelido).IsRequired().HasMaxLength(40);
                entity.Property(e => e.Cep).IsRequired().HasMaxLength(8);
                entity.Property(e => e.Logradouro).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Numero).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Complemento).HasMaxLength(80);
                entity.Property(e => e.Bairro).IsRequired().HasMaxLength(80);
                entity.Property(e => e.Cidade).IsRequired().HasMaxLength(80);
                entity.Property(e => e.Uf).IsRequired().HasMaxLength(2);

                entity.HasOne(e => e.Usuario)
                      .WithMany(u => u.Enderecos)
                      .HasForeignKey(e => e.UsuarioId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(80);
                entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Slug).IsUnique();

                entity.HasOne(e => e.CategoriaPai)
                      .WithMany(c => c.Subcategorias)
                      .HasForeignKey(e => e.CategoriaPaiId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Produto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Slug).IsRequired().HasMaxLength(170);
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.Property(e => e.Descricao).HasColumnType("text");
                entity.Property(e => e.Marca).HasMaxLength(80);

                entity.HasOne(e => e.Categoria)
                      .WithMany(c => c.Produtos)
                      .HasForeignKey(e => e.CategoriaId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Variante>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Sku).IsRequired().HasMaxLength(40);
                entity.HasIndex(e => e.Sku).IsUnique();
                entity.Ignore(e => e.PrecoEfetivo);

                entity.Property(e => e.Atributos)
                      .HasColumnType("jsonb")
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                          v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>(),
                          new ValueComparer<Dictionary<string, string>>(
                              (a, b) => JsonSerializer.Serialize(a, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(b, (JsonSerializerOptions?)null),
                              v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null).GetHashCode(),
                              v => new Dictionary<string, string>(v)));

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("ck_variantes_preco", "preco > 0");
                    t.HasCheckConstraint("ck_variantes_preco_promocional", "preco_promocional IS NULL OR preco_promocional < preco");
                });

                entity.HasOne(e => e.Produto)
                      .WithMany(p => p.Variantes)
                      .HasForeignKey(e => e.ProdutoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ImagemProduto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Url).IsRequired().HasMaxLength(500);

                entity.HasOne(e => e.Produto)
                      .WithMany(p => p.Imagens)
                      .HasForeignKey(e => e.ProdutoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Variante)
                      .WithMany()
                      .HasForeignKey(e => e.VarianteId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Estoque>(entity =>
            {
                entity.HasKey(e => e.VarianteId);
                entity.Ignore(e => e.Disponivel);
                entity.Property(e => e.Versao).IsRowVersion();

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("ck_estoques_fisica", "quantidade_fisica >= 0");
                    t.HasCheckConstraint("ck_estoques_reservada", "quantidade_reservada >= 0 AND quantidade_reservada <= quantidade_fisica");
                });

                entity.HasOne(e => e.Variante)
                      .WithOne(v => v.Estoque)
                      .HasForeignKey<Estoque>(e => e.VarianteId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Carrinho>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TokenAnonimo).HasMaxLength(100);
                entity.HasIndex(e => e.UsuarioId).IsUnique();
                entity.HasIndex(e => e.TokenAnonimo).IsUnique();

                entity.HasOne(e => e.Usuario)
                      .WithMany()
                      .HasForeignKey(e => e.UsuarioId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ItemCarrinho>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.CarrinhoId, e.VarianteId }).IsUnique();
                entity.ToTable(t => t.HasCheckConstraint("ck_itens_carrinho_quantidade", "quantidade BETWEEN 1 AND 10"));

                entity.HasOne(e => e.Carrinho)
                      .WithMany(c => c.Itens)
                      .HasForeignKey(e => e.CarrinhoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Variante)
                      .WithMany()
                      .HasForeignKey(e => e.VarianteId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

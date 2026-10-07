using Microsoft.EntityFrameworkCore;
using ShopFlow.Domain.Entidades;
using ShopFlow.Domain.Exceptions;
using ShopFlow.Domain.Interfaces.Variantes;
using ShopFlow.Infrastructure.Data;

namespace ShopFlow.Application.Service
{
    // Todo método segue o mesmo roteiro: 1. BUSCAR -> 2. CHECAR (regras) -> 3. MONTAR -> 4. SALVAR
    public class VarianteService : IVarianteService
    {
        private readonly IVarianteRepository _varianteRepository;
        private readonly ShopFlowDbContext _db;

        public VarianteService(IVarianteRepository varianteRepository, ShopFlowDbContext db)
        {
            _varianteRepository = varianteRepository;
            _db = db;
        }

        public async Task<IEnumerable<Variante>> ObterVariantesAsync()
        {
            return await _varianteRepository.ObterVariantesAsync();
        }

        public async Task<Variante?> ObterPorIdAsync(Guid id)
        {
            return await _varianteRepository.ObterPorIdAsync(id);
        }

        // VAR-01
        public async Task<Variante> CriarAsync(Guid produtoId, string sku, IEnumerable<AtributoVariante> atributos, decimal preco, decimal? precoPromocional, int pesoGramas, int quantidadeInicial)
        {
            // 2. CHECAR
            if (!await _db.Produtos.AnyAsync(p => p.Id == produtoId))
                throw new NaoEncontradoException("Produto não encontrado.");

            sku = sku.Trim();
            if (await _db.Variantes.AnyAsync(v => v.Sku == sku))
                throw new ConflitoException("Já existe uma variante com esse SKU.");

            ValidarPrecos(preco, precoPromocional);     

            if (quantidadeInicial < 0)
                throw new ValidacaoException("A quantidade inicial não pode ser negativa.");

            // 3. MONTAR
            var variante = new Variante
            {
                Id = Guid.NewGuid(),
                ProdutoId = produtoId,
                Sku = sku,
                Atributos = MontarAtributos(atributos),
                Preco = preco,
                PrecoPromocional = precoPromocional,
                PesoGramas = pesoGramas,
                Ativa = true
            };

            var estoque = new Estoque
            {
                VarianteId = variante.Id,
                QuantidadeFisica = quantidadeInicial,
                QuantidadeReservada = 0
            };
            variante.Estoque = estoque;

            // 4. SALVAR
            _db.Variantes.Add(variante);
            _db.Estoques.Add(estoque);
            await _db.SaveChangesAsync();

            return variante;
        }

        // VAR-02
        public async Task<Variante> AtualizarAsync(Guid varianteId, string sku, IEnumerable<AtributoVariante> atributos, decimal preco, decimal? precoPromocional, int pesoGramas)
        {
            // 1. BUSCAR
            var variante = await _db.Variantes.Include(v => v.Atributos).FirstOrDefaultAsync(v => v.Id == varianteId)
                ?? throw new NaoEncontradoException("Variante não encontrada.");

            sku = sku.Trim();

            // 2. CHECAR
            // RN-07: SKU único, ignorando a própria variante
            if (await _db.Variantes.AnyAsync(v => v.Sku == sku && v.Id != varianteId))
                throw new ConflitoException("Já existe uma variante com esse SKU.");

            ValidarPrecos(preco, precoPromocional); // RN-08

            // 3. MONTAR
            variante.Sku = sku;
            variante.Atributos.Clear(); // os antigos saem, os novos entram
            foreach (var atributo in MontarAtributos(atributos))
                variante.Atributos.Add(atributo);
            variante.Preco = preco;
            variante.PrecoPromocional = precoPromocional;
            variante.PesoGramas = pesoGramas;

            // 4. SALVAR
            await _db.SaveChangesAsync();

            return variante;
        }

        // VAR-03
        public async Task DesativarAsync(Guid varianteId)
        {
            // 1. BUSCAR
            var variante = await _db.Variantes.Include(v => v.Produto).FirstOrDefaultAsync(v => v.Id == varianteId)
                ?? throw new NaoEncontradoException("Variante não encontrada.");

            if (!variante.Ativa)
                return;

            // 2. CHECAR
            
            if (variante.Produto is { Ativo: true })
            {
                var temOutraAtiva = await _db.Variantes.AnyAsync(v => v.ProdutoId == variante.ProdutoId && v.Id != varianteId && v.Ativa);
                if (!temOutraAtiva)
                    throw new ConflitoException("Não é possível desativar a última variante ativa de um produto ativo. Desative o produto antes.");
            }

            // 3. MONTAR  (RN-12: soft delete, a linha continua existindo)
            variante.Ativa = false;

            // 4. SALVAR
            await _db.SaveChangesAsync();
        }

        
        public async Task<Estoque> AjustarEstoqueAsync(Guid varianteId, int quantidade)
        {
            // 1. BUSCAR
            var estoque = await _db.Estoques.FirstOrDefaultAsync(e => e.VarianteId == varianteId)
                ?? throw new NaoEncontradoException("Variante não encontrada.");

            // 2. CHECAR
            
            if (quantidade < 0)
                throw new RegraDeNegocioException("A quantidade física não pode ser negativa.");
            if (quantidade < estoque.QuantidadeReservada)
                throw new RegraDeNegocioException("A quantidade física não pode ser menor que a quantidade reservada.");

            // 3. MONTAR
            estoque.QuantidadeFisica = quantidade;

       
            await _db.SaveChangesAsync();

            return estoque;
        }

        private static List<AtributoVariante> MontarAtributos(IEnumerable<AtributoVariante> atributos)
        {
            var lista = atributos
                .Select(a => new AtributoVariante { Nome = a.Nome.Trim(), Valor = a.Valor.Trim() })
                .ToList();

            if (lista.Any(a => a.Nome.Length == 0 || a.Valor.Length == 0))
                throw new ValidacaoException("Nome e valor do atributo são obrigatórios.");

            if (lista.GroupBy(a => a.Nome, StringComparer.OrdinalIgnoreCase).Any(g => g.Count() > 1))
                throw new ValidacaoException("Não é possível repetir o mesmo atributo na variante.");

            return lista;
        }

  
        private static void ValidarPrecos(decimal preco, decimal? precoPromocional)
        {
            if (preco <= 0)
                throw new ValidacaoException("O preço deve ser maior que zero.");

            if (precoPromocional is not null && (precoPromocional <= 0 || precoPromocional >= preco))
                throw new ValidacaoException("O preço promocional deve ser maior que zero e menor que o preço.");
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopFlow.API.Extensao;
using ShopFlow.Application.DTO.Request;
using ShopFlow.Application.DTO.Response;
using ShopFlow.Domain.Interfaces.Produtos;

namespace ShopFlow.API.Controllers
{
    [Authorize(Policy = Policies.Admin)]
    [ApiController]
    [Route("api/v1/admin/produtos")]
    public class ProdutoController : ControllerBase
    {
        private readonly IProdutoService _produtoService;

        public ProdutoController(IProdutoService produtoService)
        {
            _produtoService = produtoService;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var produtos = await _produtoService.ListarAsync();
            return Ok(produtos.Select(ProdutoResponse.De)); 
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> ObterPorId(Guid id)
        {
            var produto = await _produtoService.ObterPorIdAsync(id);
            return Ok(ProdutoResponse.De(produto));
        }

        [HttpPost]
        public async Task<IActionResult> Criar(CriarProdutoRequest request)
        {
            var produto = await _produtoService.CriarAsync(
                request.Nome, request.Descricao, request.Marca, request.CategoriaId);

            return CreatedAtAction(nameof(ObterPorId), new { id = produto.Id }, ProdutoResponse.De(produto));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Atualizar(Guid id, AtualizarProdutoRequest request)
        {
            var produto = await _produtoService.AtualizarAsync(
                id, request.Nome, request.Descricao, request.Marca, request.CategoriaId);

            return Ok(ProdutoResponse.De(produto));
        }

        [HttpPatch("{id:guid}/ativar")]
        public async Task<IActionResult> Ativar(Guid id)
        {
            await _produtoService.AtivarAsync(id);
            return NoContent();
        }

        [HttpPatch("{id:guid}/desativar")]
        public async Task<IActionResult> Desativar(Guid id)
        {
            await _produtoService.DesativarAsync(id);
            return NoContent();
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopFlow.API.Extensao;
using ShopFlow.Application.DTO.Request;
using ShopFlow.Application.DTO.Response;
using ShopFlow.Domain.Interfaces;

namespace ShopFlow.API.Controllers
{
    [Authorize(Policy = Policies.Admin)]
    [ApiController]
    [Route("api/v1/admin/categorias")]
    public class CategoriaController : ControllerBase
    {
        private readonly ICategoriaService _categoriaService;

        public CategoriaController(ICategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var categorias = await _categoriaService.ListarAsync();
            return Ok(categorias.Select(CategoriaResponse.De));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> ObterPorId(Guid id)
        {
            var categoria = await _categoriaService.ObterPorIdAsync(id);
            return Ok(CategoriaResponse.De(categoria));
        }

        [HttpPost]
        public async Task<IActionResult> Criar(CriarCategoriaRequest request)
        {
            var categoria = await _categoriaService.CriarAsync(request.Nome, request.CategoriaPaiId);
            return CreatedAtAction(nameof(ObterPorId), new { id = categoria.Id }, CategoriaResponse.De(categoria));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Atualizar(Guid id, AtualizarCategoriaRequest request)
        {
            var categoria = await _categoriaService.AtualizarAsync(id, request.Nome, request.CategoriaPaiId);
            return Ok(CategoriaResponse.De(categoria));
        }

        [HttpPatch("{id:guid}/desativar")]
        public async Task<IActionResult> Desativar(Guid id)
        {
            await _categoriaService.DesativarAsync(id);
            return NoContent();
        }
    }
}

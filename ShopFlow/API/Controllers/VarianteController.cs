using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopFlow.API.Extensao;
using ShopFlow.Application.DTO.Request;
using ShopFlow.Application.DTO.Response;
using ShopFlow.Domain.Entidades;
using ShopFlow.Domain.Exceptions;
using ShopFlow.Domain.Interfaces.Variantes;

namespace ShopFlow.API.Controllers
{
    [Authorize(Policy = Policies.Admin)]
    [ApiController]
    [Route("api/v1/admin")]
    public class VarianteController : ControllerBase
    {
        private readonly IVarianteService _varianteService;

        public VarianteController(IVarianteService varianteService)
        {
            _varianteService = varianteService;
        }

        // Só existe para o Location do 201 do POST
        [HttpGet("variantes/{id:guid}")]
        public async Task<IActionResult> ObterPorId(Guid id)
        {
            var variante = await _varianteService.ObterPorIdAsync(id)
                ?? throw new NaoEncontradoException("Variante não encontrada.");

            return Ok(VarianteResponse.De(variante));
        }

        [HttpPost("produtos/{produtoId:guid}/variantes")]
        public async Task<IActionResult> Criar(Guid produtoId, CriarVarianteRequest request)
        {
            var variante = await _varianteService.CriarAsync(
                produtoId, request.Sku, ParaEntidades(request.Atributos), request.Preco,
                request.PrecoPromocional, request.PesoGramas, request.QuantidadeInicial);

            return CreatedAtAction(nameof(ObterPorId), new { id = variante.Id }, VarianteResponse.De(variante));
        }

        [HttpPut("variantes/{id:guid}")]
        public async Task<IActionResult> Atualizar(Guid id, AtualizarVarianteRequest request)
        {
            var variante = await _varianteService.AtualizarAsync(
                id, request.Sku, ParaEntidades(request.Atributos), request.Preco, request.PrecoPromocional, request.PesoGramas);

            return Ok(VarianteResponse.De(variante));
        }

        private static IEnumerable<AtributoVariante> ParaEntidades(IEnumerable<AtributoVarianteRequest> atributos) =>
            atributos.Select(a => new AtributoVariante { Nome = a.Nome, Valor = a.Valor });

        [HttpPatch("variantes/{id:guid}/desativar")]
        public async Task<IActionResult> Desativar(Guid id)
        {
            await _varianteService.DesativarAsync(id);
            return NoContent();
        }

        [HttpPut("variantes/{id:guid}/estoque")]
        public async Task<IActionResult> AjustarEstoque(Guid id, AjustarEstoqueRequest request)
        {
            var estoque = await _varianteService.AjustarEstoqueAsync(id, request.Quantidade);
            return Ok(EstoqueResponse.De(estoque));
        }
    }
}

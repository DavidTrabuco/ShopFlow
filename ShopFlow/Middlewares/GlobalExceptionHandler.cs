using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ShopFlow.Domain.Exceptions;

namespace ShopFlow.Middlewares
{
    // Ponto único que transforma QUALQUER exceção não tratada em ProblemDetails.
    // Service só lança; quem decide o status HTTP é esta classe.
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetails;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(IProblemDetailsService problemDetails, ILogger<GlobalExceptionHandler> logger)
        {
            _problemDetails = problemDetails;
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception ex, CancellationToken ct)
        {
            var (status, titulo, detalhe) = ex switch
            {
                ValidacaoException      => (StatusCodes.Status400BadRequest, "Requisição inválida", ex.Message),
                NaoAutorizadoException  => (StatusCodes.Status401Unauthorized, "Não autorizado", ex.Message),
                NaoEncontradoException  => (StatusCodes.Status404NotFound, "Recurso não encontrado", ex.Message),
                ConflitoException       => (StatusCodes.Status409Conflict, "Conflito", ex.Message),
                RegraDeNegocioException => (StatusCodes.Status422UnprocessableEntity, "Regra de negócio violada", ex.Message),

                // Rede de segurança: dois cadastros simultâneos com o mesmo e-mail passam pelo
                // EmailExisteAsync, mas o índice único do Postgres barra o segundo (código 23505).
                DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } }
                    => (StatusCodes.Status409Conflict, "Conflito", "Registro duplicado."),

                // Dois admins ajustando o mesmo estoque ao mesmo tempo: o segundo perde (Versao mudou)
                DbUpdateConcurrencyException
                    => (StatusCodes.Status409Conflict, "Conflito", "O registro foi alterado por outra operação. Recarregue e tente novamente."),

                // Qualquer outra coisa é bug: mensagem genérica, NUNCA ex.Message (pode vazar detalhes internos)
                _ => (StatusCodes.Status500InternalServerError, "Erro inesperado", "Ocorreu um erro inesperado. Tente novamente mais tarde.")
            };

            // Só o 500 é erro de verdade. 4xx de domínio é comportamento esperado, não vai como Error.
            if (status == StatusCodes.Status500InternalServerError)
                _logger.LogError(ex, "Erro não tratado em {Metodo} {Caminho}", http.Request.Method, http.Request.Path);
            else
                _logger.LogInformation("{Excecao} em {Caminho}: {Mensagem}", ex.GetType().Name, http.Request.Path, ex.Message);

            http.Response.StatusCode = status;

            return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = http,
                Exception = ex,
                ProblemDetails = new ProblemDetails
                {
                    Status = status,
                    Title = titulo,
                    Detail = detalhe,
                    Instance = http.Request.Path
                }
            });
        }
    }
}

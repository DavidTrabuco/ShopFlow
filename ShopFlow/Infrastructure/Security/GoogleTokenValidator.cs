using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using ShopFlow.Domain.Exceptions;
using ShopFlow.Domain.Interfaces;
using ShopFlow.Domain.Options;

namespace ShopFlow.Infrastructure.Security
{
    public class GoogleTokenValidator : IGoogleTokenValidator
    {
        private readonly GoogleOptions _google;

        public GoogleTokenValidator(IOptions<GoogleOptions> google)
        {
            _google = google.Value;
        }

        public async Task<GoogleUsuarioInfo> ValidarAsync(string idToken)
        {
            try
            {
                // Audience = o nosso ClientId: recusa tokens emitidos para outro app
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _google.ClientId }
                });

                return new GoogleUsuarioInfo(
                    payload.Subject,
                    payload.Email,
                    payload.EmailVerified,
                    payload.Name ?? payload.Email);
            }
            catch (InvalidJwtException)
            {
                throw new NaoAutorizadoException("Token do Google inválido.");
            }
        }
    }
}

using ShopFlow.Application.DTO.Request;
using ShopFlow.Application.DTO.Response;

namespace ShopFlow.Application.Service
{
    public interface IAuthService
    {
        Task<AuthResponse> RegistrarAsync(RegistrarRequest request, CancellationToken ct = default);
        Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    }
}

using ShopFlow.Domain.Enums;

namespace ShopFlow.Domain.Entidades
{
    public class Usuario
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        // Null quando a conta foi criada só pelo Google (sem senha)
        public string? SenhaHash { get; set; }
        // "sub" do Google: identificador estável (o e-mail pode mudar)
        public string? GoogleId { get; set; }
        public string? Cpf { get; set; }
        public string? Telefone { get; set; }
        public PapelUsuario Papel { get; set; } = PapelUsuario.Cliente;
        public bool EmailConfirmado { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;



        public ICollection<Endereco> Enderecos { get; set; } = new List<Endereco>();
    }
}

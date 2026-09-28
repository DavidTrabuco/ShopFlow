namespace ShopFlow.Domain.Entidades
{
    public class Endereco
    {
        public Guid Id { get; set; }
        public string Apelido { get; set; } = string.Empty;
        public string Cep { get; set; } = string.Empty;
        public string Logradouro { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string? Complemento { get; set; }
        public string Bairro { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Uf { get; set; } = string.Empty;
        public bool Principal { get; set; }



        public Guid UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
    }
}

namespace ShopFlow.Domain.Entidades
{
    public class Sessao
    {

        public Guid Id { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime ExpiraEm { get; set; }
        public DateTime? EncerradaEm { get; set; }
        public bool Ativa => EncerradaEm is null && DateTime.UtcNow < ExpiraEm;



        public Guid UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
    }
}

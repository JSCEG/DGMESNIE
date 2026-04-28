namespace NSIE.Models
{
    public class UsuarioApp
    {
        public int Id { get; set; }
        public string Usuario { get; set; }
        public string Email { get; set; }
        public string EmailNormalizado { get; set; }
        public string PasswordHash { get; set; }
    }
}

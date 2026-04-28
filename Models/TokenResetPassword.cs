namespace NSIE.Models
{
    public class TokenResetPassword
    {
        public int IdUsuario { get; set; }
        public string Token { get; set; }
        public DateTime Fecha { get; set; }
    }
}

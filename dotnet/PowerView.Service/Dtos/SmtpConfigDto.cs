
namespace PowerView.Service.Dtos
{
    public class SmtpConfigDto
    {
        public string Server { get; set; }
        public ushort Port { get; set; }
        public string User { get; set; }
        public string Auth { get; set; }
        public string Email { get; set; }
    }
}

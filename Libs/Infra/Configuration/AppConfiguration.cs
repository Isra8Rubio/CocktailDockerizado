using Microsoft.Extensions.Configuration;

namespace Infraestructura.Configuration
{
    public class AppConfiguration
    {
        private readonly IConfiguration configuration;
        public JwtConfig Jwt { get; set; } = new JwtConfig();
        public EmailConfig Email { get; set; } = new();

        public AppConfiguration(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void Load()
        {
            configuration.GetSection("Jwt").Bind(Jwt);
            configuration.GetSection("Email").Bind(Email);
        }
    }

        public class JwtConfig
        {
            public string Key { get; set; } = null!;
            public string Issuer { get; set; } = null!;
            public string Audience { get; set; } = null!;
            public int ExpiresInMinutes { get; set; }
        }

        public class EmailConfig
        {
            public string Host { get; set; } = null!;
            public int Port { get; set; } = 587;
            public bool EnableSsl { get; set; } = true;
            public string User { get; set; } = null!;
            public string Password { get; set; } = null!;
            public string From { get; set; } = null!;
            public string FromName { get; set; } = "Cocktail Bar";
        }
}

namespace LittleArkFoundation.Data
{
    
    public class ConnectionService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ConnectionService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ConnectionService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetDefaultConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection");
        }

        public string GetCurrentConnectionString()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("ConnectionString")
                   ?? GetDefaultConnectionString();
        }
    }
}

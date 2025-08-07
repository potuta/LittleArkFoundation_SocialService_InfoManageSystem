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
            return _configuration.GetConnectionString("DefaultConnection3");
        }

        public string GetCurrentConnectionString()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null && session.GetString("ConnectionString") != null)
            {
                return session.GetString("ConnectionString");
            }
            return _configuration.GetConnectionString("DefaultConnection3"); 
        }
    }
}

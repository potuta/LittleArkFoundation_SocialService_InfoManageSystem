namespace LittleArkFoundation.Data
{
    
    public class ConnectionService
    {
        private readonly IConfiguration _configuration;

        public ConnectionService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetConnectionString(string dbType)
        {
            if (dbType == "backup")
            {
                // return "Data Source=DESKTOP-MQAI63D\\SQLEXPRESS;Initial Catalog=STU_DB_2023_2024;Integrated Security=True;TrustServerCertificate=True;Column Encryption Setting=Disabled";
                return _configuration.GetConnectionString("DefaultConnection");
            }
            return _configuration.GetConnectionString("DefaultConnection");
        }
    }
}

using System.Data;
using Microsoft.Data.SqlClient;

namespace hrm.Context
{
    public class HRMContext
    {
        private readonly string? _connectionString;
        private readonly IConfiguration _configuration;

        public HRMContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}

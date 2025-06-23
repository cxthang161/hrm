using Dapper;
using hrm.Context;

namespace hrm.Respository.Roles
{
    public class RoleRespository : IRoleRespository
    {
        private readonly HRMContext _context;
        public RoleRespository(HRMContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Entities.Roles>> GetAll()
        {
            using var connection = _context.CreateConnection();
            string sql = "SELECT * FROM Roles";
            return await connection.QueryAsync<Entities.Roles>(sql);
        }
    }
}

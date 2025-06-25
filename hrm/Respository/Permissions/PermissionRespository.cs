using Dapper;
using hrm.Context;
using hrm.DTOs;

namespace hrm.Respository.Permissions
{
    public class PermissionRespository : IPermissionRespository
    {
        private readonly HRMContext _context;
        public PermissionRespository(HRMContext context)
        {
            _context = context;
        }

        public async Task<List<Entities.Permissions>> GetAllPermissions()
        {
            using var connection = _context.CreateConnection();
            string sql = "SELECT * FROM Permissions";
            return (await connection.QueryAsync<Entities.Permissions>(sql)).ToList();
        }

        public async Task<(string, bool)> CreatePermission(PermissionDto permissionDto)
        {
            using var connection = _context.CreateConnection();
            string existsSql = "SELECT Name, KeyName FROM Permissions WHERE Name = @Name OR KeyName = @KeyName";
            var existingPermission = await connection.QueryFirstOrDefaultAsync<Entities.Permissions>(existsSql, new
            {
                Name = permissionDto.Name,
                KeyName = permissionDto.KeyName
            });
            if (existingPermission != null)
            {
                return ("Permission with the same name or key already exists.", false);
            }
            string sql = "INSERT INTO Permissions (Name, KeyName) VALUES (@Name, @KeyName)";
            var result = await connection.ExecuteAsync(sql, new
            {
                Name = permissionDto.Name,
                KeyName = permissionDto.KeyName
            });
            return (result > 0 ? "Permission created successfully." : "Failed to create permission.", result > 0);
        }

        public async Task<(string, bool)> UpdatePermission(int id, PermissionDto permissionDto)
        {
            using var connection = _context.CreateConnection();
            string existsSql = "SELECT Name, KeyName FROM Permissions WHERE Name = @Name OR KeyName = @KeyName";
            var existingPermission = await connection.QueryFirstOrDefaultAsync<Entities.Permissions>(existsSql, new
            {
                Name = permissionDto.Name,
                KeyName = permissionDto.KeyName
            });
            if (existingPermission != null)
            {
                return ("Permission with the same name or key already exists.", false);
            }
            string sql = "UPDATE Permissions SET Name = @Name, KeyName = @KeyName WHERE Id = @Id";
            var result = await connection.ExecuteAsync(sql, new
            {
                Id = id,
                Name = permissionDto.Name,
                KeyName = permissionDto.KeyName
            });
            return (result > 0 ? "Permission updated successfully." : "Failed to update permission.", result > 0);
        }
    }
}

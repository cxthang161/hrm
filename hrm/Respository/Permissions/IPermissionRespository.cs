using hrm.DTOs;

namespace hrm.Respository.Permissions
{
    public interface IPermissionRespository
    {
        public Task<List<Entities.Permissions>> GetAllPermissions();
        public Task<(string, bool)> CreatePermission(PermissionDto permissionDto);
        public Task<(string, bool)> UpdatePermission(int id, PermissionDto permissionDto);
    }
}

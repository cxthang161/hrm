namespace hrm.Respository.Roles
{
    public interface IRoleRespository
    {
        public Task<IEnumerable<Entities.Roles>> GetAll();
    }
}

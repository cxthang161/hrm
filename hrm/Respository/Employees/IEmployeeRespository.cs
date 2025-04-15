using hrm.Entities;

namespace hrm.Respository.Employees
{
    public interface IEmployeeRespository
    {
        Task<IEnumerable<Employee>> GetAll();
    }
}

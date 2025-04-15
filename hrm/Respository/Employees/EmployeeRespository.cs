using Dapper;
using hrm.Context;
using hrm.Entities;

namespace hrm.Respository.Employees
{
    public class EmployeeRespository : IEmployeeRespository
    {
        private readonly HRMContext _context;

        public EmployeeRespository(HRMContext context) => _context = context;

        public async Task<IEnumerable<Employee>> GetAll()
        {
            var query = @"
                        SELECT 
                            e.Id AS EmployeeId,
                            e.Name,
                            e.Dob,
                            e.Address,
                            e.Phone,
                            e.Email,
                            e.Sex,
                            e.SalaryLevel,
                            e.DepartmentId,
                            d.Id AS DepartmentId,
                            d.Name AS DepartmentName,
                            s.Id AS SalaryId,
                            s.Salary AS SalaryAmount,
                            s.Date AS SalaryDate
                        FROM Employees e
                        JOIN Departments d ON e.DepartmentId = d.Id
                        JOIN Salaries s ON e.SalaryLevel = s.Id";

            using (var conn = _context.CreateConnection())
            {
                var employees = await conn.QueryAsync<Employee, Department, Salaries, Employee>(
                                query,
                                (emp, dept, salary) =>
                                {
                                    emp.Department = dept;
                                    emp.Salary = salary;
                                    return emp;
                                },
                                splitOn: "DepartmentId,SalaryId"
                            );

                return employees.ToList();
            }
        }
    }
}

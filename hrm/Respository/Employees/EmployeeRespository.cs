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
            var sql = @"
                        SELECT 
                            e.Id,
                            e.Name,
                            e.Dob,
                            e.Phone,
                            e.Email,
                            e.Sex,
                            e.SalaryId,
                            e.DepartmentId,
                            d.Id,
                            d.Name,
                            s.Id,
                            s.Salary,
                            s.Date,
                            p.Id,
                            p.Name
                        FROM Employees e
                        JOIN Departments d ON e.DepartmentId = d.Id
                        JOIN Salaries s ON e.SalaryId = s.Id
                        LEFT JOIN EmployeePositions ep ON e.Id = ep.EmployeeId
                        LEFT JOIN Positions p ON ep.PositionId = p.Id";

            var employeeDict = new Dictionary<int, Employee>();

            using (var conn = _context.CreateConnection())
            {
                var result = await conn.QueryAsync<Employee, Department, Salaries, Position, Employee>(
                    sql,
                    (employee, department, salary, position) =>
                    {
                        if (!employeeDict.TryGetValue(employee.Id, out var empEntry))
                        {
                            employee.Department = department;
                            employee.Salary = salary;
                            employee.EmployeePositions = new List<Position>();
                            empEntry = employee;
                            employeeDict.Add(employee.Id, empEntry);
                        }

                        if (position != null && !empEntry.EmployeePositions.Any(ep => ep.Id == position.Id))
                        {
                            empEntry.EmployeePositions.Add(position);
                        }

                        return empEntry;
                    },
                    splitOn: "Id,Id,Id"
                );

                return employeeDict.Values.ToList();
            }
        }
    }
}

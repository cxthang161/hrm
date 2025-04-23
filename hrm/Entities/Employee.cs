namespace hrm.Entities
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Dob { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Sex { get; set; }
        public int SalaryId { get; set; }
        public int DepartmentId { get; set; }

        public Department Department { get; set; }
        public Salaries Salary { get; set; }
        public ICollection<Position> EmployeePositions { get; set; }
    }
}

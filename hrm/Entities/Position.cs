namespace hrm.Entities
{
    public class Position
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<EmployeePosition> EmployeePositions { get; set; }
    }
}

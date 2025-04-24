namespace hrm.Entities
{
    public interface User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int EmployeeId { get; set; }
        public int PositionId { get; set; }
        public Employee Employee { get; set; }
        public Position Position { get; set; }
    }

    public interface LoginRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}

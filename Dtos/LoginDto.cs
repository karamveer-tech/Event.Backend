using static eventManager.Enums.Enums;

namespace eventManager.Dtos
{
    public class LoginDto
    {
        public long Id { get; set; }
        public long RoleId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Token { get; set; }
    }
}

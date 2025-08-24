using System;
using static eventManager.Enums.Enums;

namespace eventManager.Model
{
    public class users
    {
        public Int64 id { get; set; } = 0;
        public Int64 role_id { get; set; } = 0;
        public string name { get; set; } = string.Empty;
        public string email { get; set; }
        public string password_hash { get; set; } = string.Empty;
        public string phone { get; set; } = string.Empty;
        public string token { get; set; } = string.Empty;
        public string role_name { get; set; } = string.Empty;
        public Status Status { get; set; } = Status.Pending;
        public DateTime created_at { get; set; } = DateTime.Now;

        //public roles Role { get; set; } = new roles();
    }
}

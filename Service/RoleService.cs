using eventManager.Model;

namespace eventManager.Service
{
    public class RoleService
    {
        private readonly IConfiguration _configuration;
        public RoleService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<List<roles>> GetAllRoles()
        {
            DataBaseUtil _db = new DataBaseUtil(_configuration);
            List<roles> roles = new List<roles>();
            string query = @"select * from roles";
            roles = await _db.GetMultipleRecordFromQuery<roles>(query);
            return roles;
        }


    }
}

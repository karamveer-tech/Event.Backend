using eventManager.Dtos;
using eventManager.Model;
using MySqlX.XDevAPI.Common;
using static eventManager.Enums.Enums;

namespace eventManager.Service
{
    public class OrganiserService
    {
        private readonly IConfiguration _configuration;
        public OrganiserService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<Response> AddOrgStaff(users input)
        {
            var response = new Response();
            DataBaseUtil _db = new DataBaseUtil(_configuration);
            try
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                Dictionary<string, string> staff = new Dictionary<string, string>();
                values.Add("role_id", input.role_id.ToString());
                values.Add("name", input.name);
                values.Add("email", input.email);
                values.Add("phone", input.phone);
                values.Add("status", Status.Pending.ToString());
                var res = await _db.SaveScalar<users>(values);

                if (res == 1)
                {
                    var Staff_user_id = await _db.getIdByEmail(input.email, "users", "id");
                    staff.Add("organiser_id", "1");
                    staff.Add("staff_user_id", Staff_user_id.ToString());
                    var _res = await _db.SaveScalar<organiser_staff>(staff);
                    if (_res == 1)
                    {
                        response.status = 1;
                        response.message = "Organiser staff added successfully.";
                    }
                    else
                    {
                        response.status = 0;
                        response.message = "Failed to save organiser staff.";
                    }

                }
                else
                {
                    response.status = 0;
                    response.message = "Failed to save organiser staff.";
                }

            }
            catch (Exception ex) { }
            return response;
        }
        public async Task<users> GetStaff(string id)
        {
            DataBaseUtil _db = new DataBaseUtil(_configuration);
            var res = new users();
            try
            {
                Dictionary<string, string> whereParam = new Dictionary<string, string>();
                whereParam.Add("id", id);
                res = await _db.GetSingleRecordFromSingleTable<users>(whereParam, "");
            }
            catch (Exception ex) { }
            return res;
        }
        public async Task<List<users>> GetStaffList(string organiser_id)
        {
            DataBaseUtil _db = new DataBaseUtil(_configuration);
            var res = new List<users>();
            try
            {
                string query = @"SELECT u.*
                                 FROM users u
                                 INNER JOIN organiser_staff os ON u.id = os.staff_user_id
                                 WHERE os.organiser_id = '" + organiser_id + "'";
                res = await _db.GetMultipleRecordFromQuery<users>(query);
            }
            catch (Exception ex) { }
            return res;
        }
        public async Task<List<OrganiserListDto>> GetOrganiserList()
        {
            DataBaseUtil _db = new DataBaseUtil(_configuration);
            var res = new List<OrganiserListDto>();
            try
            {
                string query = @"SELECT u.id,u.name FROM users u Join roles r on r.id = u.role_id where r.name = 'organiser'";
                res = await _db.GetMultipleRecordFromQuery<OrganiserListDto>(query);
            }
            catch (Exception ex) { }
            return res;
        }
    }
}

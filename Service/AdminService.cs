using eventManager.Model;
using static eventManager.Enums.Enums;

namespace eventManager.Service
{
    public class AdminService
    {
        private readonly IConfiguration _configuration;
        public AdminService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<List<organiser_staff>> GetAllPendingOrganizerStaff()
        {
            List<organiser_staff> staff = new List<organiser_staff>();
            DataBaseUtil _db = new DataBaseUtil(_configuration);
            try
            {
                string query = @"select * from users u
                                left join roles r on u.role_id = r.id 
                                where r.name = 'organiser' and u.status = pending";
                staff = await _db.GetMultipleRecordFromQuery<organiser_staff>(query);
            }
            catch (Exception ex) { }
            return staff;
        }
        public async Task<Response> ChangeOrganiserStatus(string status, string organiser_id)
        {
            DataBaseUtil _db = new DataBaseUtil(_configuration);
            var result = new Response();
            try
            {
                Dictionary<string, string> coloumn = new Dictionary<string, string>();
                Dictionary<string, string> where = new Dictionary<string, string>();
                coloumn.Add("status", status);
                where.Add("id", organiser_id);
                var res = await _db.UpdateRecord<users>(coloumn, where, "");
                if (res == 1)
                {
                    result.status = 1;
                    result.message = "Status updated successfully.";
                }
                else
                {
                    result.status = 0;
                    result.message = "Failed to update status.";
                }
            }
            catch (Exception ex) { }
            return result;
        }
        public async Task<Response> AddAdminStaff(users user)
        {
            DataBaseUtil _db = new DataBaseUtil(_configuration);
            var result = new Response();
            try
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                Dictionary<string, string> staff = new Dictionary<string, string>();
                values.Add("role_id", user.role_id.ToString());
                values.Add("name", user.name);
                values.Add("email", user.email);
                values.Add("phone", user.phone);
                values.Add("status", Status.Pending.ToString());
                var res = await _db.SaveScalar<users>(values);

                if (res == 1)
                {
                    var staff_id = await _db.getIdByEmail(user.email, "users", "id");
                    staff.Add("admin_id", "1");
                    staff.Add("staff_user_id", staff_id.ToString());
                    var _res = await _db.SaveScalar<admin_staff>(staff);
                    if (_res == 1)
                    {
                        result.status = 1;
                        result.message = "Admin staff added successfully.";
                    }
                    else
                    {
                        result.status = 0;
                        result.message = "Failed to save Admin staff.";
                    }
                }
                else
                {
                    result.status = 0;
                    result.message = "Failed to save Admin staff.";
                }

            }
            catch (Exception ex) { }
            return result;
        }
    }
}

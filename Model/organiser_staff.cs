namespace eventManager.Model
{
    public class organiser_staff
    {
        public Int64 id { get; set; }
        public Int64 organiser_id { get; set; }
        public Int64 staff_user_id { get; set; }

        public users Organiser { get; set; }
        public users StaffUser { get; set; }
    }
}

namespace eventManager.Model
{
    public class admin_staff
    {
        public Int64 id { get; set; }
        public int admin_id { get; set; }
        public int staff_user_id { get; set; }

        public users Admin { get; set; }
        public users StaffUser { get; set; }
    }
}

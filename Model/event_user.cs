namespace eventManager.Model
{
    public class event_user
    {
        public int id { get; set; }

        public int event_id { get; set; }

        public int user_id { get; set; }

        public int no_of_events { get; set; }

        public DateTime purchase_at { get; set; }

        public int ticket_type_id { get; set; }
    }
}

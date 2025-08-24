namespace eventManager.Model
{
    public class ticket_type
    {
        public int id { get; set; }

        public string ticket_type_name { get; set; }

        public decimal ticket_price { get; set; }

        public int event_id { get; set; }

        public int no_of_tickets { get; set; }
    }
}

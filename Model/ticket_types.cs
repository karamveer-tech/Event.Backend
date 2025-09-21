namespace eventManager.Model
{
    public class ticket_types
    {
        public Int64 id { get; set; }
        public Int64 event_id { get; set; }
        public Int64 ticket_type_id { get; set; }
        public string name { get; set; }
        public decimal price { get; set; }
        public Int64 quantity { get; set; }

        public events Event { get; set; }
    }
}

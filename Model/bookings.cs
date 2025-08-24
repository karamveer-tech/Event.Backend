namespace eventManager.Model
{
    public class bookings
    {
        public Int64 id { get; set; }
        public Int64 user_id { get; set; }
        public Int64 event_id { get; set; }
        public DateTime booking_date { get; set; }
        public string status { get; set; }
        public decimal total_amount { get; set; }

        public users User { get; set; }
        public events Event { get; set; }
        public ICollection<booking_tickets> BookingTickets { get; set; }
    }
}

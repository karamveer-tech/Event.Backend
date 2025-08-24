namespace eventManager.Model
{
    public class booking_tickets
    {
        public Int64 id { get; set; }
        public Int64 booking_id { get; set; }
        public Int64 ticket_type_id { get; set; }
        public Int64 quantity { get; set; }
        public decimal price { get; set; }

        public bookings Booking { get; set; }
        public ticket_types TicketType { get; set; }
    }
}

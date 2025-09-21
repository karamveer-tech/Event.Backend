namespace eventManager.Model
{
    //public class bookings
    //{
    //    public Int64 id { get; set; }
    //    public Int64 user_id { get; set; }
    //    public Int64 event_id { get; set; }
    //    public Int64 quantity { get; set; }
    //    public DateTime booking_date { get; set; }
    //    public string status { get; set; }
    //    public decimal total_amount { get; set; }

    //    public users User { get; set; }
    //    public events Event { get; set; }
    //    public string bookingTicketsJson { get; set; }
    //    public ICollection<booking_tickets> BookingTickets { get; set; }
    ////}
    //public class BookingDto
    //{
    //    public long id { get; set; }
    //    public long user_id { get; set; }
    //    public long event_id { get; set; }
    //    public long quantity { get; set; }
    //    public DateTime booking_date { get; set; }
    //    public string status { get; set; }
    //    public decimal total_amount { get; set; }
    //    public ICollection<booking_tickets> BookingTickets { get; set; }

    //    // Angular will send tickets as JSON string inside FormData
    //    public string bookingTicketsJson { get; set; }
    //}
    //public class ticket_type
    //{
    //    public long? ticket_type_id { get; set; }
    //    public long quantity { get; set; }
    //    public decimal price { get; set; }
    //}

    public class bookings
    {
        public long id { get; set; }
        public long user_id { get; set; }
        public long event_id { get; set; }
        public int quantity { get; set; }
        public DateTime booking_date { get; set; }
        public string status { get; set; }
        public string? emailId { get; set; }
        public decimal total_amount { get; set; }
        public string bookingTicketsJson { get; set; }
        public List<ticket_type>? ticket_Types { get; set; }
    }
    public class totalTicketBookedCount
    {
        public int quantity { get; set; }
    }

    public class myBookings
    {
        public long id { get; set; }
        public long user_id { get; set; }
        public long event_id { get; set; }
        public int quantity { get; set; }
        public DateTime booking_date { get; set; }
        public string status { get; set; }
        public decimal total_amount { get; set; }
        public Int64 organiser_id { get; set; } = 0;
        public string title { get; set; } = string.Empty;
        public string? description { get; set; } = string.Empty;       // lowercase to match FormData
        public string? location { get; set; } = string.Empty;
        public string banner_path { get; set; } = string.Empty;
        public string ImagesPath { get; set; } = string.Empty;
        public string csvFile_path { get; set; } = string.Empty;
        public DateTime start_datetime { get; set; }
        public DateTime end_datetime { get; set; }
        public string ticketType { get; set; } = string.Empty;        // "Free" or "Paid"
        public int freeSeats { get; set; }
        public IFormFile? banner { get; set; }
        public List<IFormFile>? images { get; set; }
        //public IFormFile? csvFile { get; set; }
        public string paidTicketsJson { get; set; } = string.Empty;
        public List<paid_tickets>? paidTickets { get; set; }         // optional, deserialize from JSON if needed
        public DateTime created_at { get; set; } = DateTime.UtcNow;
    }
}

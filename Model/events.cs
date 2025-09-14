using System.Net.Sockets;

namespace eventManager.Model
{
    public class events
    {
        //public Int64 id { get; set; } = 0;
        //public Int64 organiser_id { get; set; } = 0;
        //public string title { get; set; } = string.Empty;
        //public string description { get; set; } = string.Empty;
        //public string location { get; set; } = string.Empty;
        //public DateTime start_datetime { get; set; } = DateTime.UtcNow;
        //public DateTime end_datetime { get; set; } = DateTime.UtcNow;
        //public string status { get; set; } = string.Empty;
        //public DateTime created_at { get; set; } = DateTime.UtcNow;
        //public IFormFile? banner { get; set; }
        //public IFormFile? csvFile { get; set; }
        //public string template_path { get; set; } = string.Empty;

        public Int64 id { get; set; } = 0;
        public Int64 organiser_id { get; set; } = 0;
        public string title { get; set; } = string.Empty;
        public string? description { get; set; } = string.Empty;       // lowercase to match FormData
        public string? location { get; set; } = string.Empty;
        public string banner_path { get; set; } = string.Empty;
        public string ImagesPath { get; set; } = string.Empty;
        public string csvFile_path { get; set; } = string.Empty;
        public DateTime start_datetime { get; set; }
        public DateTime end_datetime { get; set; }
        public string status { get; set; } = "Draft";             // lowercase to match FormData
        public string ticketType { get; set; } = string.Empty;        // "Free" or "Paid"
        public int freeSeats { get; set; }
        public IFormFile? banner { get; set; }
        public List<IFormFile>? images { get; set; }
        //public IFormFile? csvFile { get; set; }
        public string paidTicketsJson { get; set; } = string.Empty;
        public List<paid_tickets>? paidTickets { get; set; }         // optional, deserialize from JSON if needed
        public DateTime created_at { get; set; } = DateTime.UtcNow;
    }


    //public class EventCreateDto
    //{
    //    public Int64 id { get; set; } = 0;
    //    public string title { get; set; } = string.Empty;
    //    public string description { get; set; } = string.Empty;       // lowercase to match FormData
    //    public string location { get; set; } = string.Empty;
    //    public string template_path { get; set; } = string.Empty;
    //    public DateTime start_datetime { get; set; }
    //    public DateTime end_datetime { get; set; }
    //    public string status { get; set; } = "Scheduled";             // lowercase to match FormData
    //    public string ticketType { get; set; } = string.Empty;        // "Free" or "Paid"
    //    public int? freeSeats { get; set; }
    //    public IFormFile? banner { get; set; }
    //    public IFormFile? csvFile { get; set; }
    //    public string paidTicketsJson { get; set; } = string.Empty;
    //    public List<PaidTicketDto>? paidTickets { get; set; }         // optional, deserialize from JSON if needed
    //    public DateTime created_at { get; set; } = DateTime.UtcNow;
    //}

    public class paid_tickets
    {
        public int id { get; set; }
        public Int64 event_id { get; set; }
        public string name { get; set; } = string.Empty;
        public int seats { get; set; }
        public decimal price { get; set; }
    }
}

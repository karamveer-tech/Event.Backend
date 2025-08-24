using System.Net.Sockets;

namespace eventManager.Model
{
    public class events
    {
        public Int64 id { get; set; } = 0;
        public Int64 organiser_id { get; set; } = 0;
        public string title { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string location { get; set; } = string.Empty;
        public DateTime start_datetime { get; set; } = DateTime.UtcNow;
        public DateTime end_datetime { get; set; } = DateTime.UtcNow;
        public string status { get; set; } = string.Empty;
        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public IFormFile? event_Template { get; set; }
        public string template_path { get; set; } = string.Empty;
    }
}

namespace eventManager.Model
{
    public class event_media
    {
        public Int64 id { get; set; }
        public Int64 event_id { get; set; }
        public string media_type { get; set; }
        public string file_url { get; set; }
        public DateTime uploaded_at { get; set; }

        public events Event { get; set; }
    }
}

namespace eventManager.Model
{
    public class Response
    {
        public int status { get; set; }
        public string message { get; set; }
        public string redirect_url { get; set; }
        public object data { get; set; }

    }
}

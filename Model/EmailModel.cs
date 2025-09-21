namespace eventManager.Model
{
    public class EmailModel
    {
        public string SenderEmail { get; set; }
        public string Password { get; set; }
        public string SmtpHost { get; set; }
        public int Port { get; set; }
        public string DisplayName { get; set; }
    }

}

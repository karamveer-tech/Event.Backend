namespace eventManager.Dtos
{
    public class ChangePassword
    {
        public string User_id { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}

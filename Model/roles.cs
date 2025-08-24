namespace eventManager.Model
{
    public class roles
    {
        public Int64 id { get; set; }
        public string name { get; set; }

        public ICollection<users> Users { get; set; }
    }
}

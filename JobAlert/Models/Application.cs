namespace JobAlert.Models
{
    public class Application
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public Job Job { get; set; }
        public bool Applied { get; set; }
    }
}

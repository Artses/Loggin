namespace Api_Loggin.Models
{
    public class Collector
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public List<Log> Logs { get; set; }
    }
}

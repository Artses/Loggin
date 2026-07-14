namespace Api_Loggin.Models
{
    public class Collector
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Url { get; set; } = null!;
        public List<Log> Logs { get; set; } = new();
    }
}

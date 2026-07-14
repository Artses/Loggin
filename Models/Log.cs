using System.Text.Json.Serialization;

namespace Api_Loggin.Models
{
	public class Log
	{
		public Guid Id { get; set; }
		public string Path { get; set; } = null!;
		public Guid CollectorId { get; set; }
		[JsonIgnore]
		public Collector Collector { get; set; } = null!;
	}
}
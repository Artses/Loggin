using Api_Loggin.Models;

namespace Api_Loggin.DTOs
{
    public record RegisterCollectorDto(string Name, string Url, List<Log> Logs);
    public record DeleteCollectorDto(Guid id);
    public record UpdateCollectorDto(Guid Id, string Name, string Url, List<Log> Logs);
}

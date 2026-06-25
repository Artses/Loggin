namespace Api_Loggin.DTOs
{
    public record RegisterCollectorDto(string Name, string Url, List<string> Path);
    public record DeleteCollectorDto(Guid id);
    public record UpdateCollectorDto(Guid Id, string Name, string Url, List<string> Path);
}

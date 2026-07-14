using Microsoft.AspNetCore.Routing.Constraints;

namespace Api_Loggin.DTOs
{
    public record RegisterLogDto(Guid CollectorId, string path);
    public record GetLogDto(Guid CollectorId, Guid LogId);
    public record FetchLogDto(string path, string url);
    public record UpdateLogDto(Guid Id, Guid CollectorId, string Path);
    public record DeleteLogDto(Guid id);
}

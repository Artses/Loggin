using Microsoft.AspNetCore.Routing.Constraints;

namespace Api_Loggin.DTOs
{
    public record RegisterLogDto(Guid CollectorId, string path);
    public record GetLogDto(Guid CollectorId, Guid LogId);
    public record FetchLogDto(Guid Id, Guid CollectorId);
    public record UpdateLogDto(Guid Id, Guid CollectorId, string Path);
    public record DeleteLogDto(Guid Id);
    public record ResponseLogDto(List<Line> Line);
    public record RequestLogDto(string Path);
    public record Line(int Order, string TimeStamp, string Text);
}

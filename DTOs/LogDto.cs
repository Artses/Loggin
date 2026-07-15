namespace Api_Loggin.DTOs
{
    public record RegisterLogDto(Guid CollectorId, string path);
    public record GetLogDto(Guid CollectorId, Guid LogId);
    public record FetchLogDto(Guid Id, Guid CollectorId);
    public record UpdateLogDto(Guid Id, Guid CollectorId, string Path);
    public record DeleteLogDto(Guid Id);
    public record RequestLogDto(string Path);
    public record ResponseLogDto(List<LogsDto> Content);
    public record LogsDto(int Order, DateTime TimeStamp, string Line);
}

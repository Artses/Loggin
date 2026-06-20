using Api_Loggin.DTOs;

namespace Api_Loggin.Services.Interfaces
{
    public interface IWebSocketService
    {
        Task ConnectAsync();
        Task WriteMessageAsync(WriteMessageDto writeMessagedto);
    }
}

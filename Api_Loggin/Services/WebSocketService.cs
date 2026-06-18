using Api_Loggin.DTOs;
using Api_Loggin.Services.Interfaces;
using System.Net.WebSockets;
using System.Text;

namespace Api_Loggin.Services
{
    public class WebSocketService : IWebSocketService
    {
        private readonly ClientWebSocket _socket = new ClientWebSocket();

        public async Task ConnectAsync()
        {
            await _socket.ConnectAsync(
                new Uri("ws://localhost:8000/api/v1/logs"),
                CancellationToken.None
            );
        }

        public async Task MessageAsync(WriteMessageDto dto)
        {

            byte[] buffer = Encoding.UTF8.GetBytes(dto.Message);

            await _socket.SendAsync(
                new ArraySegment<byte>(buffer),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );

            Console.WriteLine("Message sent!");
        }
    }
}

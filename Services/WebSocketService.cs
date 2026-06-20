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
            try
            {
                await _socket.ConnectAsync(
                    new Uri("ws://localhost:8000/api/v1/logs"),
                    CancellationToken.None
                );
            }
            catch(WebSocketException e)
            {
                throw new WebSocketException("Não foi possivel se conectar ao websocket, verifique se: \n- Seu coletor está ligado \n- Se seu servidor tem acesso ao endpoint do coletor");
            }
        }

        public async Task WriteMessageAsync(WriteMessageDto dto)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(dto.Message);

            if (_socket.State != WebSocketState.Open)
            {
                throw new InvalidOperationException("WebSocket is not connected.");
            }

            await _socket.SendAsync(
                 new ArraySegment<byte>(buffer),
                 WebSocketMessageType.Text,
                 true,
                 CancellationToken.None

             );

            while (_socket.State == WebSocketState.Open)
            {
                var result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                Console.WriteLine(message);
            }
            Console.WriteLine("Message sent!");
        }
    }
}

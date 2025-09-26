using System.Net.WebSockets;
using System.Text;

namespace TestRevify.Api.Middlewares;

public class WebSocketMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await HandleWebSocketAsync(webSocket);
        }
        else
        {
            await _next(context);
        }
    }

    private async Task HandleWebSocketAsync(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!result.CloseStatus.HasValue)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            // Aquí se puede manejar el mensaje recibido y enviar una respuesta si es necesario

            var responseMessage = Encoding.UTF8.GetBytes($"Respuesta del servidor: {message}");
            await webSocket.SendAsync(new ArraySegment<byte>(responseMessage), result.MessageType, result.EndOfMessage, CancellationToken.None);

            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }
}

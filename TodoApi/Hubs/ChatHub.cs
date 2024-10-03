using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.AspNetCore.SignalR;

namespace TodoApi.Hubs
{
    public class ChatHub : Hub
    {
        List<UserInfo> userInfos = [];
        public async Task SendMessage(String user, String message)
        {
            userInfos.Add(new UserInfo
            {
                userId = Context.ConnectionId
            });
            Console.WriteLine($"user id -> {Context.ConnectionId}");
            await Clients.All.SendAsync("ReceiverMessage", user, $"{message} - ${Context.ConnectionId}");
        }
    }

    public class UserInfo
    {
        public required string userId { get; set; }
    }

    // Hosted service that runs TcpListener in the background
    public class TcpListenerService : IHostedService
    {

        public TcpListenerService(ILogger<TcpListenerService> logger){
            this._logger = logger;
            _logger.LogInformation("HEHE");
        }

        private readonly ILogger _logger;
        private TcpListener? _tcpListener;
        private Task? _listenerTask;
        private CancellationTokenSource? _cts;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _tcpListener = new TcpListener(IPAddress.Any, 13000);
            _tcpListener.Start();

            _cts = new CancellationTokenSource();
            _listenerTask = Task.Run(() => ListenForClients(_cts.Token), cancellationToken);

            return Task.CompletedTask;
        }

        private async Task ListenForClients(CancellationToken cancellationToken)
        {
            Console.WriteLine("TCP Listener started on port 13000");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_tcpListener != null)
                    {
                        TcpClient client = await _tcpListener.AcceptTcpClientAsync(cancellationToken);

                        // Handle the client in a separate task
                        _ = Task.Run(() => HandleClient(client), cancellationToken);
                    }else {
                        throw new Exception("_tcpListener is null");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            Console.WriteLine("Client connected!");

            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
            {
                string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received: {receivedData}");

                byte[] responseData = Encoding.UTF8.GetBytes("Hello from server");
                await stream.WriteAsync(responseData, 0, responseData.Length);
                Console.WriteLine("Sent: Hello from server");
            }

            client.Close();
            Console.WriteLine("Client disconnected.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cts?.Cancel();
            _tcpListener?.Stop();
            return _listenerTask ?? Task.CompletedTask;
        }
    }
}
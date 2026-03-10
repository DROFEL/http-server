using System.Net;
using System.Net.Sockets;

namespace http_server;

public class TcpConnectionAccepter : ITcpConnectionAccepter
{
    private readonly TcpListener _listener;

    public TcpConnectionAccepter(IPAddress ip, int port)
    {
        _listener = new TcpListener(ip, port);
    }

    public void Start() => _listener.Start();

    public void Stop() => _listener.Stop();

    public async ValueTask<Stream> AcceptStreamAsync(CancellationToken cancellationToken = default)
    {
        var client = await _listener.AcceptTcpClientAsync(cancellationToken);
        return client.GetStream();
    }

    public void Dispose() => _listener.Stop();
}
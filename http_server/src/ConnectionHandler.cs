using System.IO.Pipelines;
using http_server.Handlers;
using http_server.helpers;
using http_server.Parsers;

namespace http_server;

public class ConnectionHandler
{
    private readonly ILog _log;

    public ConnectionHandler()
    {
        _log = new Log();
    }
    public async Task HandleConnectionAsync(Stream wire, CancellationToken token = default)
    {
        _log.WriteLine("Connection opened");
        await using (wire)
        {
            // if (_certificate is not null)
            // {
            //     await using var ssl = new SslStream(net, leaveInnerStreamOpen: false);
            //     await ssl.AuthenticateAsServerAsync(_certificate);
            //     wire = ssl;
            // }

            var reader = PipeReader.Create(wire);
            var writer = PipeWriter.Create(wire);
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            
            try
            {
                var httpRequest = await HttpParser.ParseRequest(reader);

                var context = new ConnectionContext(reader, writer, httpRequest, cts.Token);
                var handler = HttpHandlerFactory.Create(httpRequest.HttpVersion);
                _log.WriteLine($"Request on version {httpRequest.HttpVersion.ToString()} to {httpRequest.Path} with {httpRequest.Method}.");
                await handler.Accept(context);
            }
            catch (Exception e)
            {
                _log.WriteLine("Invalid request response: 400");
                var userErrorHttpResponse = new HttpResponse(400);
                await writer.WriteAsync(userErrorHttpResponse.FormatResponseAsByteArray(), token);
                return;
            }

        }
        
    }
}
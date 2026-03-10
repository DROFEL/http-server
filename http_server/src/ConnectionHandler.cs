using System.IO.Pipelines;
using http_server.Handlers;
using http_server.helpers;
using http_server.Parsers;

namespace http_server;

public class ConnectionHandler
{
    private readonly ILog _log;
    private readonly HttpParser _parser;

    public ConnectionHandler()
    {
        _log = new Log();
    }
    public async Task HandleConnectionAsync(Stream wire, CancellationToken token = default)
    {
        _log.Debug("Connection opened");
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
                var httpRequest = await _parser.ParseRequest(reader);

                var context = new ConnectionContext(reader, writer, httpRequest, cts.Token);
                var handler = HttpHandlerFactory.Create(httpRequest.HttpVersion);
                _log.Debug($"Request on version {httpRequest.HttpVersion.ToString()} to {httpRequest.Path} with {httpRequest.Method}.");
                await handler.Accept(context);
            }
            catch (Exception e)
            {
                _log.Info("Invalid request response: 400");
                var userErrorHttpResponse = new HttpResponse(400);
                await writer.WriteAsync(userErrorHttpResponse.FormatResponseAsByteArray(), token);
                return;
            }

        }
        
    }
}
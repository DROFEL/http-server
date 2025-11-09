namespace http_server.helpers;

public interface ILog
{
    public void Write(string msg);
    public void WriteLine(string msg);
}
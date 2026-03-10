namespace http_server.helpers;

public interface ILog
{
    public void Info(string msg);
    public void Debug(string msg);
    public void Warning(string msg);
    public void Error(string msg);
}

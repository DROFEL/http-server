namespace http_server.helpers;

public class Log : ILog
{
    public void WriteLine(string msg)
    {
        Console.WriteLine(msg);
    }

    public void Write(string msg)
    {
        Console.Write(msg);
    }
}
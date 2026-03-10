namespace http_server.helpers;

public class Log : ILog
{
    public void Info(string msg)
    {
        Console.WriteLine(msg);
    }

    public void Debug(string msg)
    {
        Console.WriteLine(msg);
    }

    public void Error(string msg)
    {
        Console.WriteLine(msg);
    }
    
    public void Warning(string msg)
    {
        Console.WriteLine(msg);
    }
}
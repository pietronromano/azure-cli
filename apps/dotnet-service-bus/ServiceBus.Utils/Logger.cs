// Very simple Logger class that writes log messages to both the console and a log file in a "logs" directory in the current working directory. The log file is named with a timestamp of when the Logger instance was created. The Logger class implements IDisposable to ensure that the StreamWriter is properly flushed and closed when the Logger is disposed.
namespace ServiceBus.Utils;

using System.Text;
using System.IO;

public class Logger
{


    StreamWriter _logWriter;

    public Logger()
    {

        string logFile = "log_" + DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss") + ".txt";
        string logPath = Path.Join(Environment.CurrentDirectory, "logs");
        if (!Path.Exists(logPath))
            System.IO.Directory.CreateDirectory(logPath);

        logPath = Path.Join(logPath, logFile);
        _logWriter = File.AppendText(logPath);
    }

    public void Write(string log)
    {
        string msg = DateTime.Now.ToString("HH:mm:ss") + ": " + log;
        Console.WriteLine(msg);
        _logWriter.WriteLine(msg);
        _logWriter.Flush();
    }
    
    public void Dispose()
    {
        _logWriter.Flush();
        _logWriter.Close();
        _logWriter.Dispose();
    }
    
}


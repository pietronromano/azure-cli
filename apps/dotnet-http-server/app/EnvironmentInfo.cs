using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

public struct EnvironmentInfo
{
    public EnvironmentInfo()
    {

        string[] args = Environment.GetCommandLineArgs();
        CommandLineArgs = string.Join(" | ", args);


        GCMemoryInfo gcInfo = GC.GetGCMemoryInfo();
        TotalAvailableMemoryBytes = gcInfo.TotalAvailableMemoryBytes;

        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        string[] memoryLimitPaths = new string[]
        {
            "/sys/fs/cgroup/memory.max",
            "/sys/fs/cgroup/memory.high",
            "/sys/fs/cgroup/memory.low",
            "/sys/fs/cgroup/memory/memory.limit_in_bytes",
        };

        string[] currentMemoryPaths = new string[]
        {
            "/sys/fs/cgroup/memory.current",
            "/sys/fs/cgroup/memory/memory.usage_in_bytes",
        };

        MemoryLimit = GetBestValue(memoryLimitPaths);
        MemoryUsage = GetBestValue(currentMemoryPaths);
        
    }

    public static void LogInfo(string method, string info)
    {
        string now = DateTime.Now.ToUniversalTime().ToString();
        Console.WriteLine($"{now}: {method}: {info}");
    }

    public static string GetEnvironmentVariables()
    {

        StringBuilder variables = new StringBuilder();
        foreach (DictionaryEntry de in Environment.GetEnvironmentVariables())
            variables.Append($"{de.Key}={de.Value} | ");

        return variables.ToString();
    }

    public string UniversalTime => DateTime.Now.ToUniversalTime().ToString();
    public string MachineName => Environment.MachineName;

    public string? ProcessPath => Environment.ProcessPath;
    public string CommandLineArgs { get; } 


    public string CurrentDirectory => Environment.CurrentDirectory;

    
    public string RuntimeVersion => RuntimeInformation.FrameworkDescription;
    public string OSVersion => RuntimeInformation.OSDescription;
    public string OSArchitecture => RuntimeInformation.OSArchitecture.ToString();
    public string User => Environment.UserName;
    public int ProcessorCount => Environment.ProcessorCount;

    public long TotalAvailableMemoryBytes { get; }
    public long MemoryLimit { get; }
    public long MemoryUsage { get; }

    private static long GetBestValue(string[] paths)
    {
        string value = string.Empty;
        foreach (string path in paths)
        {
            if (Path.Exists(path) &&
                long.TryParse(File.ReadAllText(path), out long result))
            {
                return result;
            }
        }

        return 0;
    }
}

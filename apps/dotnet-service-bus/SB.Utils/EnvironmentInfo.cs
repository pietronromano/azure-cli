namespace SB.Utils;

using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

public struct EnvironmentInfo
{
    
    public string UniversalTime { get; }
    public string MachineName { get; }
    public string HostProcessId { get; }
    public string InfoGuid { get; }
    public string ProcessPath { get; }
    public string CommandLineArgs { get; }
    public string CurrentDirectory { get; }
    
    public string RuntimeVersion { get; }
    public string OSVersion { get; }
    public string OSArchitecture { get; }
    public string User { get; }
    public int ProcessorCount { get; }

    public long TotalAvailableMemoryBytes { get; }
    public long MemoryLimit { get; }
    public long MemoryUsage { get; }

    public EnvironmentInfo()
    {
        // Initialize all instance-specific values once at creation time
        UniversalTime = DateTime.Now.ToUniversalTime().ToString();
        MachineName = Environment.MachineName;
        HostProcessId = Environment.ProcessId.ToString();
        InfoGuid = Guid.NewGuid().ToString();
        ProcessPath = Environment.ProcessPath;
        CurrentDirectory = Environment.CurrentDirectory;
        
        RuntimeVersion = RuntimeInformation.FrameworkDescription;
        OSVersion = RuntimeInformation.OSDescription;
        OSArchitecture = RuntimeInformation.OSArchitecture.ToString();
        User = Environment.UserName;
        ProcessorCount = Environment.ProcessorCount;

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

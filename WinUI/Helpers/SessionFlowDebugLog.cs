using System;
using System.Diagnostics;
using System.IO;

namespace WinUI.Helpers;

/// <summary>Writes session-related failures to LocalAppData for debugging (same folder tree as startup errors).</summary>
internal static class SessionFlowDebugLog
{
    private const string AppFolderName = "PlayPointPOS";

    public static string LogFilePath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppFolderName,
            "Logs",
            "session.log");

    public static void Append(string stage, Exception exception)
    {
        try
        {
            string directory = Path.GetDirectoryName(LogFilePath)!;
            Directory.CreateDirectory(directory);

            string block =
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} | {stage}{Environment.NewLine}{exception}{Environment.NewLine}{Environment.NewLine}";
            File.AppendAllText(LogFilePath, block);
            Debug.WriteLine(block);
        }
        catch
        {
            Debug.WriteLine($"SessionFlowDebugLog.Append failed for stage {stage}: {exception}");
        }
    }
}

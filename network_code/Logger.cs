using Godot;
using System;
using System.IO;

public static class Logger
{
    private static readonly string logFilePath;

    static Logger()
    {
        string dir = Path.Combine(Directory.GetCurrentDirectory(), "logs");
        Directory.CreateDirectory(dir);
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        logFilePath = Path.Combine(dir, $"log_{timestamp}.txt");
    }

    public static void Log(string tag, string message)
    {
        string line = $"[{DateTime.Now:HH:mm:ss.fff}] : [{tag}] {message}";
        GD.Print(line); // console
        File.AppendAllText(logFilePath, line + "\n");
    }
}
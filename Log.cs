using System;
using System.Diagnostics;
using System.IO;

namespace CrowdControl;

internal static class Log
{
    static Log() => File.Delete("CrowdControl.log");

    //[Conditional("DEBUG")]
    public static void Debug(object message) => Write(Console.Out, message);

    public static void Message(object message) => Write(Console.Out, message);

    public static void Error(object message) => Write(Console.Error, message);

    public static void Error(Exception ex, string message) =>
        Write(Console.Error, message + Environment.NewLine + ex);

    public static void DebugFormat(string message, params object[] args) =>
        WriteFormat(Console.Out, message, args);

    public static void MessageFormat(string message, params object[] args) =>
        WriteFormat(Console.Out, message, args);

    public static void ErrorFormat(string message, params object[] args) =>
        WriteFormat(Console.Error, message, args);

    private static void WriteFormat(TextWriter writer, string format, object[] args) =>
        Write(writer, string.Format(format, args));

    private static void Write(TextWriter writer, object message)
    {
        string m = $"[{DateTimeOffset.UtcNow}] " + (message?.ToString() ?? "(null)");
        writer.Write("> ");
        writer.WriteLine(m);
        File.AppendAllText("CrowdControl.log", $"[{DateTime.Now}] {message}{Environment.NewLine}");
        try { OnMessage?.Invoke(m); }
        catch { /**/ }
    }

    public static void InvokeOnMessage(object message, bool omitTimestamp = false)
        => OnMessage?.Invoke((omitTimestamp ? string.Empty : $"[{DateTimeOffset.UtcNow}] ") + (message?.ToString() ?? "(null)"));

    public static event Action<string> OnMessage;
}
using System;
using System.IO;
using UnityEngine;

// A class for logging info to files
public class Logger : MonoBehaviour
{
    private const string LOGS_DIRECTORY = "MyLogs";

    private static string logPath;
    public static string LogPath => logPath;

    private const int LEFT_PAD_LENGTH = 6;

    private void Awake()
    {
        CreateLog();
        Application.logMessageReceived += UnityMessageHandler;
    }

    public static void CreateLog()
    {
        // if the logs directory doesn't exist, create it
        if (!Directory.Exists(LOGS_DIRECTORY)) Directory.CreateDirectory(LOGS_DIRECTORY);

        logPath = $"{LOGS_DIRECTORY}/log_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.txt";

        // create the log file
        File.Create(logPath).Dispose();

        File.AppendAllText(logPath, $"----- Stormworks XML Editor log -----\n");
        File.AppendAllText(logPath, $"Version: {Application.version}\n");
        File.AppendAllText(logPath, $"Unity version: {Application.unityVersion}\n");
        File.AppendAllText(logPath, $"Log created at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
        LogSeparator();
    }

    public static void Log(string message)
    {
        File.AppendAllText(logPath, $"{LeftPad("[LOG]", LEFT_PAD_LENGTH)} {message}\n");
    }

    public static void LogError(string message)
    {
        File.AppendAllText(logPath, $"{LeftPad("[ERR]", LEFT_PAD_LENGTH)} {message}\n");
    }

    public static void LogWarning(string message)
    {
        File.AppendAllText(logPath, $"{LeftPad("[WARN]", LEFT_PAD_LENGTH)} {message}\n");
    }

    public static void LogException(string message)
    {
        File.AppendAllText(logPath, $"{LeftPad("[EXCP]", LEFT_PAD_LENGTH)} {message}\n");
    }

    public static void LogAssertion(string message)
    {
        File.AppendAllText(logPath, $"{LeftPad("[ASRT]", LEFT_PAD_LENGTH)} {message}\n");
    }

    public static void LogStackTrace(string message)
    {
        string[] messageLines = message.Split('\n');

        File.AppendAllText(logPath, $"{LeftPad("~~", LEFT_PAD_LENGTH + 3)} {messageLines[0]}\n");

        string indent = new string(' ', LEFT_PAD_LENGTH + 4);

        for (int i = 1; i < messageLines.Length; i++)
        {
            File.AppendAllText(logPath, $"{indent + messageLines[i]}\n");
        }
    }

    public static void LogSeparator()
    {
        File.AppendAllText(logPath, $"{new string('-', 15)}\n");
    }

    private static string LeftPad(string str, int length)
    {
        return str.PadLeft(length);
    }

    private void UnityMessageHandler(string condition, string stackTrace, LogType type)
    {
        switch (type)
        {
            case LogType.Error:
                LogError($"Unity: {condition}");
                LogStackTrace(stackTrace);
                break;

            case LogType.Assert:
                LogAssertion($"Unity: {condition}");
                break;

            case LogType.Warning:
                LogWarning($"Unity: {condition}");
                break;

            case LogType.Log:
                Log($"Unity: {condition}");
                break;

            case LogType.Exception:
                LogError($"Unity: {condition}");
                LogStackTrace(stackTrace);
                break;

            default:
                Log($"Unity: {condition}");
                break;
        }
    }
}
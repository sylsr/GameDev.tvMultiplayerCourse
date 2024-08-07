using System;
using UnityEngine;

public class TimestampedLogHandler : ILogHandler
{
    private ILogHandler defaultLogHandler = Debug.unityLogger.logHandler;

    public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string message = string.Format(format, args);
        defaultLogHandler.LogFormat(logType, context, "{0} [{1}] {2}", timestamp, logType, message);
    }

    public void LogException(Exception exception, UnityEngine.Object context)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        defaultLogHandler.LogFormat(LogType.Exception, context, "{0} [Exception] {1}", timestamp, exception);
    }
}
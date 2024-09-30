using System;
using System.Diagnostics;
using System.IO;

namespace WinDurango.UI.Utils
{
    public enum LogLevel
    {
        Debug, Info, Warning, Error, Exception, Fatal,
    }

    public class Logger
    {
        public static readonly Logger Instance = new();
        private static readonly string logDir = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WinDurango"), "logs");
        private static readonly string logFile = Path.Combine(logDir, $"WinDurangoUI_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log");
        private static readonly object @lock = new();

        static Logger()
        {
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);
        }

        public static void WriteDebug(string str) => Instance.WriteLog(LogLevel.Debug, str);
        public static void WriteError(string str) => Instance.WriteLog(LogLevel.Error, str);
        public static void WriteWarning(string str) => Instance.WriteLog(LogLevel.Warning, str);
        public static void WriteInformation(string str) => Instance.WriteLog(LogLevel.Info, str);
        public static void WriteDebug(string format, params object[] args) => Instance.WriteLog(LogLevel.Debug, string.Format(format, args));
        public static void WriteError(string format, params object[] args) => Instance.WriteLog(LogLevel.Error, string.Format(format, args));
        public static void WriteWarning(string format, params object[] args) => Instance.WriteLog(LogLevel.Warning, string.Format(format, args));
        public static void WriteInformation(string format, params object[] args) => Instance.WriteLog(LogLevel.Info, string.Format(format, args));
        public static void Write(LogLevel level, string str) => Instance.WriteLog(level, str);


        public static void WriteException(Exception e, bool throwException = false)
        {
            Instance.WriteLog(LogLevel.Exception, e.ToString());
            if (throwException)
                throw e;
        }

        public static void WriteException(string str)
        {
            Instance.WriteLog(LogLevel.Exception, str);
        }

        private void WriteLog(LogLevel level, string message)
        {
            if (level == LogLevel.Debug && !App.Settings.Settings.DebugLoggingEnabled && !Debugger.IsAttached)
                return;

            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level.ToString().ToUpper()}] {message}";
            Debug.WriteLine(logEntry);

            lock (@lock)
            {
                using StreamWriter writer = new(logFile, true);
                writer.WriteLine(logEntry);
                writer.Flush();
            }
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace WinDurango.UI.Utils
{
    public class Logger
    {
        private static readonly string logDir = Path.Combine(App.DataDir, "logs");
        private static readonly string logFile = Path.Combine(logDir, $"WinDurangoUI_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log");
        private static readonly object @lock = new object();
        private static readonly Logger instance = new Logger();

        static Logger()
        {
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);
        }

        public static Logger Instance => instance;

        public void WriteDebug(string str) => WriteLog("DEBUG", str);
        public void WriteError(string str) => WriteLog("ERROR", str);
        public void WriteWarning(string str) => WriteLog("WARNING", str);
        public void WriteInformation(string str) => WriteLog("INFORMATION", str);

        public void WriteException(Exception e, bool throwException = false)
        {
            WriteLog("EXCEPTION", e.ToString());
            if (throwException)
                throw e;
        }

        private void WriteLog(string level, string message)
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}";
            Debug.WriteLine(logEntry);

            lock (@lock)
            {
                using (StreamWriter writer = new(logFile, true))
                {
                    writer.WriteLine(logEntry);
                    writer.Flush();
                }
            }
        }
    }
}

using System;
using System.IO;

namespace CSharpFinder
{
    internal class Logger
    {
        private readonly string _logFilePath;
        private string _callingMethod;

        internal Logger(string filePath)
        {
            _logFilePath = filePath;
            string directory = new FileInfo(_logFilePath).Directory.FullName;

            // Prüfen ob Verzeichnis existiert
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Prüfen ob Datei existiert
            if (!File.Exists(_logFilePath))
            {
                File.Create(_logFilePath).Close();
            }
        }

        internal void Info(string message)
        {
            _callingMethod = GetCallingMethod();
            using (StreamWriter sw = File.AppendText(_logFilePath))
            {
                sw.WriteLine($"INFO | {DateTime.Now:dd-MM-yyyy HH:mm:ss} {_callingMethod} {message}");
            }
        }

        internal void Error(string message)
        {
            _callingMethod = GetCallingMethod();
            using (StreamWriter sw = File.AppendText(_logFilePath))
            {
                sw.WriteLine($"ERROR | {DateTime.Now:dd-MM-yyyy HH:mm:ss} {_callingMethod} {message}");
            }
        }

        internal void Error(string message, Exception exception)
        {
            _callingMethod = GetCallingMethod();
            using (StreamWriter sw = File.AppendText(_logFilePath))
            {
                sw.WriteLine($"ERROR | {DateTime.Now:dd-MM-yyyy HH:mm:ss} {_callingMethod} {exception.GetType()} | {message}");
            }
        }

        private string GetCallingMethod()
        {
            try
            {
                var stackTrace = new System.Diagnostics.StackTrace();
                var callingFrame = stackTrace.GetFrame(2); // 2, um die aufrufende Methode zu erhalten
                var callingMethod = callingFrame.GetMethod();
                return $"| {callingMethod.DeclaringType.Name}.{callingMethod.Name} |";
            }
            catch (Exception ex)
            {
                Error("Logger.GetCallingMethod: " + ex.Message, ex);
                return " | ";
            }
        }
    }
}
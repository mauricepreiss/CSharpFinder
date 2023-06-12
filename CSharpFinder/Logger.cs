using System;
using System.IO;

namespace CSharpFinder
{
    public class Logger
    {
        private readonly string logFilePath;

        public Logger(string filePath)
        {
            logFilePath = filePath;
            string directory = new FileInfo(logFilePath).Directory.FullName;

            // Prüfen ob Verzeichnis existiert
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Prüfen ob Datei existiert
            if (!File.Exists(logFilePath))
            {
                File.Create(logFilePath).Close();
            }
        }

        public void Info(string message)
        {
            using (StreamWriter sw = File.AppendText(logFilePath))
            {
                sw.WriteLine($"INFO | {DateTime.Now:dd-MM-yyyy HH:mm:ss} | {message}");
            }
        }

        public void Error(string message)
        {
            using (StreamWriter sw = File.AppendText(logFilePath))
            {
                sw.WriteLine($"ERROR | {DateTime.Now:dd-MM-yyyy HH:mm:ss} | {message}");
            }
        }
    }
}
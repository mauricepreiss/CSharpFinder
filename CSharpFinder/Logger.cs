using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

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
                sw.WriteLine($"INFO | {DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")} | {message}");
            }
        }

        public void Error(string message)
        {
            using (StreamWriter sw = File.AppendText(logFilePath))
            {
                sw.WriteLine($"ERROR | {DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")} | {message}");
            }
        }
    }
}

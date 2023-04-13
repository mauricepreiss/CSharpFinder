using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CSharpFinder
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // ============================================
            // ============== Deklarationen  ==============
            // ============================================

            List<string> csharpFiles; // Liste aller C#-Dateien
            string fileName, resultFilePath; // Dateiname, Ergebnispfad
            int fileCount = 0; // Anzahl verschobener Dateien
            int errorCount = 0; // Anzahl fehlerhafter / nicht verschobener Dateien
            bool someFilesNotCoppied = false; // Wurden manche Dateien nicht kopiert?

            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", $"log-{DateTime.Now.ToString("yyyy-dd-MM")}");
            Logger logger = new Logger(logPath);


            // ============================================
            // ================= Header ===================
            // ============================================

            // Anzeigeeinstellungen
            Console.Title = "CSharp Finder";
            Console.ForegroundColor = ConsoleColor.DarkRed;

            try
            {
                Console.WindowWidth = Console.WindowWidth + 40;
                Console.WindowHeight = Console.WindowHeight + 10;
            }
            catch { }

            // Programmtitel
            Console.WriteLine();
            Console.WriteLine("█▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀█");
            Console.WriteLine("█       CSHARP FINDER      █");
            Console.WriteLine("█▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄█");
            Console.WriteLine();


            // ============================================
            // =============== Pfadabfrage  ===============
            // ============================================

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("> Bitte geben sie den Pfad des Verzeichnisses ein, dass auf C#-Dateien untersucht werden soll: ");
            Console.ForegroundColor = ConsoleColor.White;
            string dirPath = Console.ReadLine();

            // Prüfen ob Pfad gefüllt
            if (string.IsNullOrEmpty(dirPath))
            {
                WriteErrorLine("Bitte geben sie einen Pfad an!!");
            }

            // Prüfen ob Zielverzeichnis existiert
            if (!Directory.Exists(dirPath))
            {
                WriteErrorLine("Dieses Verzeichnis existiert nicht. Bitte wählen sie en anderes Verzeichnis aus!!!");
            }

            // Dateien auflisten
            Console.ForegroundColor = ConsoleColor.Cyan;

            csharpFiles = new List<string>();

            try
            {
                // ============================================
                // ============= Dateien finden  ==============
                // ============================================

                // Startmeldung
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Dateien werden gesucht...");

                // Zielverzeichnis für kopierte Dateien
                string resultDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "result");
                if (!Directory.Exists(resultDirPath))
                {
                    Directory.CreateDirectory(resultDirPath);
                }

                // Alle Dateien im Verzeichnis mit .exe und .dll bekommen
                List<string> files = new List<string>();

                // Alle Dateien in "dirPath" und seinen Unterordnern suchen
                foreach (string file in Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories))
                {
                    // Auf DLL oder EXE Datei prüfen
                    if (Path.GetExtension(file).Equals(".dll", StringComparison.OrdinalIgnoreCase) ||
                        Path.GetExtension(file).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        // Prüfen, ob die Datei schon in der Liste vorhanden ist
                        fileName = Path.GetFileName(file);
                        if (!files.Any(f => Path.GetFileName(f).Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                        {
                            // Wenn die Datei noch nicht in der Liste ist, hinzufügen
                            files.Add(file);
                            Console.WriteLine($"Datei gefunden: {fileName}");
                        }
                    }
                }


                // ============================================
                // ============= Dateien prüfen  ==============
                // ============================================


                foreach (string file in files)
                {
                    try
                    {
                        Assembly assembly = Assembly.LoadFrom(file);
                        if (IsCSharpAssembly(assembly))
                        {
                            // Kompletten Pfad bekommen, Datei in Liste hinzufügen
                            fileName = Path.GetFileName(file);
                            resultFilePath = Path.Combine(resultDirPath, fileName);
                            File.Copy(file, resultFilePath, true);
                            csharpFiles.Add(fileName);
                            fileCount++;

                            // Konsolenausgabe
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("Datei kopiert: " + fileName);
                        }
                    }
                    catch (BadImageFormatException) { /* ignore non-.net assemblies */ }
                    catch (Exception ex)
                    {
                        errorCount++;
                        Console.ForegroundColor = ConsoleColor.Red;
                        logger.Error($"Fehler beim Laden der Datei: {file} : {ex.Message}");
                    }
                }

                // Prüfen auf keine Dateien gefunden
                if (csharpFiles.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Es wurden keine Dateien gefunden. Das Programm wird geschlossen.");
                    Console.ReadKey();
                    Environment.Exit(0);
                }

                Console.ForegroundColor = ConsoleColor.Magenta;

                // Vorherige Dateien ausgeben
                Console.WriteLine($"Insgesamt gefundene Dateien: {files.Count}");

                // Ergebnis ausgeben
                Console.WriteLine($"Davon verschobene C#-Dateien: {fileCount}");

                // Prüfen auf fehlerhafte Dateien
                if (errorCount > 0)
                {
                    someFilesNotCoppied = true;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("HINWEIS: Einige Dateien wurden nicht verschoben oder sind fehlerhaft. Ein Fehler-Log zu diesen Dateien wurde unter 'logs' abgelegt.");
                    Console.WriteLine($"Fehlerhafte / nicht verschobene Dateien: {errorCount}");
                }

                // ============================================
                // ======== Kopierverzeichnis öffnen  =========
                // ============================================

                bool gotValidAnswer = false;
                ConsoleKeyInfo info = new ConsoleKeyInfo();

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Möchten sie das Kopierverzeichnis öffnen? [Y/N]");

                do
                {
                    info = Console.ReadKey();

                    switch (info.Key)
                    {
                        case (ConsoleKey.Y):
                            Process.Start(resultDirPath);
                            gotValidAnswer = true;
                            break;
                        case (ConsoleKey.N):
                            gotValidAnswer = true;
                            break;
                        default:
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Keine gültige Eingabe!");
                            gotValidAnswer = false;
                            break;
                    }
                } while (!gotValidAnswer);

                // ============================================
                // ============ Log-Datei öffnen  =============
                // ============================================

                if (someFilesNotCoppied)
                {
                    gotValidAnswer = false;
                    info = new ConsoleKeyInfo();
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("Möchten sie die Log-Datei öffnen? [Y/N]");

                    do
                    {
                        info = Console.ReadKey();

                        switch (info.Key)
                        {
                            case (ConsoleKey.Y):
                                Process.Start(logPath);
                                gotValidAnswer = true;
                                break;
                            case (ConsoleKey.N):
                                gotValidAnswer = true;
                                break;
                            default:
                                Console.WriteLine();
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Keine gültige Eingabe!");
                                gotValidAnswer = false;
                                break;
                        }
                    } while (!gotValidAnswer);
                }

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                WriteErrorLine("Fehler beim Aufrufen der Dateien aus dem Verzeichnis: " + ex.Message);
            }
        }

        private static void WriteErrorLine(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ReadKey();
            Main(new string[0]);
        }

        private static bool IsCSharpAssembly(Assembly assembly)
        {
            return assembly.GetCustomAttributes(typeof(CompilerGeneratedAttribute), inherit: false).Any() ||
                    assembly.GetTypes().Any(type => type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), inherit: false).Any() ||
                    type.FullName?.StartsWith("System", StringComparison.Ordinal) == false);
        }
    }
}

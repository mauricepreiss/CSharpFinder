using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CSharpFinder
{
    internal class Program
    {
        private static readonly string _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", $"log-{DateTime.Now:yyyy-MM-dd}.log");
        private static readonly Logger logger = new Logger(_logPath);
        private static bool consoleResized = false;
        private static bool errorOccured = false;
        private static bool gotValidAnswer = false;
        private static ConsoleKeyInfo info;

        static void Main(string[] args)
        {
            // ============================================
            // ============== Deklarationen  ==============
            // ============================================


            List<string> csharpFiles; // Liste aller C#-Dateien
            string fileName, resultFilePath; // Dateiname, Ergebnispfad
            int fileCount = 0; // Anzahl verschobener Dateien
            int errorCount = 0; // Anzahl fehlerhafter / nicht verschobener Dateien


            // ============================================
            // ================= Header ===================
            // ============================================


            // Anzeigeeinstellungen
            Console.Title = "CSharp Finder v" + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.ForegroundColor = ConsoleColor.DarkRed;

            if (Environment.Is64BitProcess)
            {
                Console.Title += " (64-bit Windows)";
            }

            try
            {
                if (!consoleResized)
                {
                    Console.WindowWidth += 40;
                    Console.WindowHeight += 10;
                    consoleResized = true;
                }
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


            // Prüfen ob Pfad gefüllt
            string dirPath = "";
            do
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("> Bitte geben sie den Pfad des Verzeichnisses ein, dass auf C#-Dateien untersucht werden soll: ");
                Console.ForegroundColor = ConsoleColor.White;

                dirPath = Console.ReadLine();
                if (!string.IsNullOrEmpty(dirPath))
                {
                    gotValidAnswer = true;
                }
                else
                {
                    WriteErrorLine("Bitte geben sie einen Pfad an!!", false);
                    gotValidAnswer = false;
                }
            } while (!gotValidAnswer);

            // Prüfen ob Zielverzeichnis existiert
            if (!Directory.Exists(dirPath))
            {
                WriteErrorLine("Dieses Verzeichnis existiert nicht. Bitte wählen sie en anderes Verzeichnis aus!");
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
                Console.WriteLine("\n[INFO]: Dateien werden gesucht...");

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
                // ============ Dateien kopieren  =============
                // ============================================

                gotValidAnswer = false;
                info = new ConsoleKeyInfo();
                bool copyFiles = false;

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("\n> Möchten Sie diese Dateien kopieren? [Y/N]");

                do
                {
                    info = Console.ReadKey();
                    Console.WriteLine("\b");

                    switch (info.Key)
                    {
                        case (ConsoleKey.Y):
                            copyFiles = true;
                            gotValidAnswer = true;
                            break;
                        case (ConsoleKey.N):
                            copyFiles = false;
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

                if (!copyFiles)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nEs wurde keine Aktion ausgeführt. Das Programm wird automatisch geschlossen...");
                    Thread.Sleep(2500);
                    Environment.Exit(0);
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\n[INFO]: Dateien werden kopiert...");

                // Dateien durchlaufen
                foreach (string file in files)
                {
                    try
                    {
                        // Prüfen auf C# / .NET Datei
                        if (IsCSharpAssembly(file))
                        {
                            // Kompletten Pfad bekommen, Datei in Liste hinzufügen
                            fileName = Path.GetFileName(file);
                            resultFilePath = Path.Combine(resultDirPath, fileName);

                            // Datei kopieren, überschreiben falls schon vorhanden
                            File.Copy(file, resultFilePath, true);
                            csharpFiles.Add(fileName);
                            fileCount++;

                            // Konsolenausgabe
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("Datei kopiert: " + fileName);
                        }
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        Console.ForegroundColor = ConsoleColor.Red;
                        logger.Error($"Fehler beim Laden der Datei: {file} : {ex.Message}", ex);
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
                    errorOccured = true;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("HINWEIS: Einige Dateien wurden nicht verschoben oder sind fehlerhaft. Ein Fehler-Log zu diesen Dateien wurde unter 'logs' abgelegt.");
                    Console.WriteLine($"Fehlerhafte / nicht verschobene Dateien: {errorCount}");
                }


                // ============================================
                // ======== Kopierverzeichnis öffnen  =========
                // ============================================


                gotValidAnswer = false;
                info = new ConsoleKeyInfo();

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("\n> Möchten Sie das Kopierverzeichnis öffnen? [Y/N]");

                do
                {
                    info = Console.ReadKey();
                    Console.WriteLine("\b");

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


                // Wenn manche Dateien nicht kopiert wurden ODER es Fehler gab
                if (errorOccured)
                {
                    gotValidAnswer = false;
                    info = new ConsoleKeyInfo();

                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("\n> Möchten Sie die Log-Datei öffnen? [Y/N]");

                    do
                    {
                        info = Console.ReadKey();
                        Console.WriteLine("\b");

                        switch (info.Key)
                        {
                            case (ConsoleKey.Y):
                                Process.Start(_logPath);
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

                // Dateien komprimieren?
                bool compressFiles = false;
                gotValidAnswer = false;
                info = new ConsoleKeyInfo();
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("\n> Möchten Sie versuchen, die kopierten Dateien so klein wie möglich zu komprimieren? (Beta) [Y/N]");

                do
                {
                    info = Console.ReadKey();
                    Console.WriteLine("\b");

                    switch (info.Key)
                    {
                        case (ConsoleKey.Y):
                            CompressFiles(resultDirPath);
                            gotValidAnswer = true;
                            compressFiles = true;
                            break;
                        case (ConsoleKey.N):
                            gotValidAnswer = true;
                            compressFiles = false;
                            break;
                        default:
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Keine gültige Eingabe!");
                            gotValidAnswer = false;
                            break;
                    }
                } while (!gotValidAnswer);

                // Prüfen ob Dateien komprimiert wurden?
                if (compressFiles)
                {
                    // Komprimierte Dateien anzeigen?
                    gotValidAnswer = false;
                    info = new ConsoleKeyInfo();
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("\n> Möchten Sie das Verzeichnis der komprimierten Dateien öffnen? [Y/N]");

                    do
                    {
                        info = Console.ReadKey();
                        Console.WriteLine("\b");

                        switch (info.Key)
                        {
                            case (ConsoleKey.Y):
                                Process.Start(Path.Combine(resultDirPath, "compressed"));
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
                logger.Error(ex.Message, ex);
                WriteErrorLine("Fehler beim Aufrufen der Dateien aus dem Verzeichnis: " + ex.Message);
            }
        }

        private static void CompressFiles(string filePath)
        {
            try { Console.WindowWidth += 40; } catch { }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            string compressedFile, compressedFolderPath;

            foreach (string file in Directory.GetFiles(filePath, "*.*", SearchOption.AllDirectories))
            {
                compressedFile = Path.Combine(Path.GetDirectoryName(file), "compressed", Path.GetFileName(file) + ".gz");
                compressedFolderPath = Path.GetDirectoryName(compressedFile);

                if (!Directory.Exists(compressedFolderPath))
                {
                    Directory.CreateDirectory(compressedFolderPath);
                }

                using (FileStream originalFileStream = File.OpenRead(file))
                {
                    using (FileStream compressedFileStream = File.Create(compressedFile))
                    {
                        using (GZipStream compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                        {
                            originalFileStream.CopyTo(compressionStream);
                            Console.WriteLine("Datei komprimiert/kopiert zu: " + compressedFile.Replace(filePath, "..."));
                        }
                    }
                }
            }

            Console.WriteLine("[INFO]: Dateien kopiert. Nutzen sie 7Zip oder andere Tools, um die Dateien wieder zu dekomprimieren.");
        }

        private static void WriteErrorLine(string message, bool restart = true)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ERROR]: " + message);
            Console.ReadKey();

            // Prüfen auf geforderten Restart
            if (restart)
            {
                Console.Clear();
                Main(new string[0]);
            }
        }

        private static bool IsCSharpAssembly(string assemblyLocation)
        {
            bool loadResult, csharpResult;

            // Versuchen, die Assembly zu laden
            loadResult = TryLoadAssembly(assemblyLocation, out Assembly loadedAssembly);
            if (!loadResult || loadedAssembly == null)
            {
                return false;
            }

            // Prüfen ausführen, ob es eine C# Assembly ist
            csharpResult = IsGeneratedOrSystemAssembly(loadedAssembly);
            if (!csharpResult)
            {
                csharpResult = IsReflectionOnlyAssembly(loadedAssembly);
                if (!csharpResult)
                {
                    csharpResult = IsValidImageRuntimeAssemlby(loadedAssembly);
                    if (!csharpResult)
                    {
                        csharpResult = IsSystemAssembly(loadedAssembly);
                        if (!csharpResult)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        private static bool TryLoadAssembly(string path, out Assembly assembly)
        {
            try
            {
                try
                {
                    assembly = Assembly.LoadFrom(path);
                    return true;
                }
                catch
                {
                    try
                    {
                        AssemblyName assemblyName = AssemblyName.GetAssemblyName(path);
                        assembly = Assembly.Load(assemblyName);
                        return true;
                    }
                    catch
                    {
                        assembly = null;
                        return false;
                    }
                }
            }
            catch
            {
                assembly = null;
                return false;
            }
        }

        private static bool IsGeneratedOrSystemAssembly(Assembly assembly)
        {
            try
            {
                var types = assembly.GetTypes();

                if (types.Any(type => type.FullName?.StartsWith("System", StringComparison.Ordinal) == false))
                {
                    return true;
                }

                if (assembly.GetTypes().Any(type => type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), inherit: false).Any()))
                {
                    return true;
                }

                if (assembly.GetCustomAttributes(typeof(CompilerGeneratedAttribute), inherit: false).Any())
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsReflectionOnlyAssembly(Assembly assembly)
        {
            try
            {
                Assembly.ReflectionOnlyLoadFrom(assembly.Location);
                return true;
            }
            catch (BadImageFormatException) // Datei ist keine gültige Datei
            {
                return false;
            }
            catch (ReflectionTypeLoadException ex) // LoaderExceptions wurden gefunden
            {
                string logMessage = "Es wurden LoaderExceptions festgestellt: ";

                var loaderExceptions = ex.LoaderExceptions;
                for (int i = 0; i < loaderExceptions.Length; i++)
                {
                    logMessage += $"({i}): {loaderExceptions[i].Message}";
                }

                return false;
            }
        }

        private static bool IsValidImageRuntimeAssemlby(Assembly assembly)
        {
            return assembly.ImageRuntimeVersion.StartsWith("v") && assembly.ImageRuntimeVersion.Contains("mscorlib");
        }

        private static bool IsSystemAssembly(Assembly assembly)
        {
            return assembly.GetTypes().Any(type => !type.FullName.StartsWith("System", StringComparison.Ordinal));
        }
    }
}
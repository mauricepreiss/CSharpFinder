# CSharpFinder

Ever wondered if it's possible to find all C# / .NET assemblys / files / dlls in a directory? The answer is yes. Some time ago i needed a tool like this for private usage and thought: it may be that others will also need it sometime.

This repository brings you a little tool to find C# .NET assemblies in a directory. It's a console application where you can insert the path your files are stored in. This path will be searched.


## Usage
This is how you can use the program:

- start the exe file
- copy and paste the path where your c# assemblies / files are stored in
- press enter

Now the program searches for all files in the given directory and checks if its a C# / .NET file. All found C# / .NET files will be printed in the console. Ex.:

```
File found: C:\user\username\source\repos\Program\DynamicLinkLibrary.dll
File found: C:\user\username\source\repos\Program\Executeable.exe
...
```

After that all found files will be copied in the programs' directory in subdirectory "result", so you dont have to search them by yourself. 

After all files are copied there will be a short summary about the scan. Ex.:

```
Found files: 2
Copied C# files: 2
Errored files: 0
```

❗ **NOTE**: If a file cannot be opened because of missing permissions or because of a opening / reading problem, the file will be marked as "errored". Error files will not be scanned / copied. If you want to scan this files anyway, please be sure to check your permissions / restart as administrator. ❗

After the summary the program will ask you if you want to open the "result" directory where the copied c# files / assemblies are copied in. You just have to press '**Y**' to open it or '**N**' to discard.

If there is any errors, the program will ask you if you want to open the log file. The program created log files for errors. They will be stored in the programs directory in subdirectory "logs". The log file will be created new for every day.

## Code
[Here](https://github.com/mauricepreiss/CSharpFinder/blob/master/CSharpFinder/Program.cs#L523)
 is one of the many function how the program identifies a file as a c# / .net assembly:
```C#
private static bool IsCSharpAssembly(Assembly assembly)
{
    return assembly.GetCustomAttributes(typeof(CompilerGeneratedAttribute), inherit: false).Any() || 
    assembly.GetTypes().Any(type => type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), inherit: false).Any() || 
    type.FullName?.StartsWith("System", StringComparison.Ordinal) == false);
}
```
This will be expanded in the future so more checks will be added.

❗❗ **This program is written in german, so the console instructions will be in german in the code. Also the comments are in german. If there is an request, i can send a version with english comments. If you want to use the program in english, please download the "CSsharpFinder-EN-{CurrentVersion}" package.** ❗❗

If there is any problem with the usage of this program, please create an issue.

## License
- free for commertical use

## Author
[@mauricepreiss](https://www.github.com/mauricepreiss)

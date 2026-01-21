using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace mergeSortLogs
{
    internal class Program
    {
        public static List<string> guids = new List<string>();
        public static List<string> lines = new List<string>();

        public static int counter = 0;
        public static string unsortedLogs = @"C:\Users\dspencer\code\sort-merge-log\logs";
        public static string sortedLogs = @"C:\Users\dspencer\code\sort-merge-log\sortMergeLogs.log";


        public static async Task<int> Main(string[] args)

        {


            // Determine log folder
            string logFolder = args.Length > 0 ? args[0] : unsortedLogs;

            Console.WriteLine("Starting watcher...");
            using var watcher = new FileSystemWatcher(@"C:\Users\dspencer\code\sort-merge-log\logs\", "*.log");
            watcher.IncludeSubdirectories = true;
            watcher.InternalBufferSize = 65536; // 64KB
            watcher.NotifyFilter = NotifyFilters.FileName |
                                             NotifyFilters.DirectoryName |
                                             NotifyFilters.LastWrite |
                                             NotifyFilters.Size;
            watcher.Changed += OnChanged;

            watcher.EnableRaisingEvents = true;

            string stop = "";
            while (stop != "stop")
            {
                Console.WriteLine("Type 'stop' to end:");
                stop = Console.ReadLine() ?? "";

                List<string> directories = GetDirectories(unsortedLogs);


                // Ensure GUID Unique in Sorted File
                await WriteLines(directories);
            }

            return 0;

        }

        private static List<string> GetDirectories(string path)
        {
            List<string> files = new List<string>();

            if (!Directory.Exists(path))
            {
                Console.WriteLine($"Error: Path '{path}' not found.");
                Console.WriteLine("Usage: LoggerApplication [parameterFile]");
                Console.WriteLine("parameterFile: Path to file containing logger parameters (default: params.txt)");
                return files;
            }

            Console.WriteLine($"{DateTime.Now}: Target Directory: {path}");

            // Get all files in the current directory
            foreach (var file in Directory.GetFiles(path))
            {
                Console.WriteLine("Found file: " + file);
                files.Add(file);
            }

            // Recursively get files from subdirectories
            foreach (var dir in Directory.GetDirectories(path))
            {
                Console.WriteLine("Entering directory: " + dir);
                files.AddRange(GetDirectories(dir)); // Recursive call
            }

            return files;
        }

        private static async Task WriteLines(List<string> directories)
        {

//             var fileStream = new FileStream(@"C:\Users\dspencer\code\sort-merge-log\sortMergeLogs.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096, useAsync: true);
// fileStream.Seek(0, SeekOrigin.Current);


            foreach (string f in directories)
            {
                Console.WriteLine("Processing file: " + f);
using var writer = new StreamWriter(new FileStream(sortedLogs, FileMode.Append, FileAccess.Write, FileShare.None, 4096, useAsync: true));
                using var reader = new StreamReader(f);
                string? line;

                while ((line = await reader.ReadLineAsync())is not null)
                {
           

                    if(DateTime.TryParse(line.Substring(0,23), out DateTime logTime))
                    {
                        Console.WriteLine("Parsed timestamp: " + logTime.ToString("o"));
                    }
                    else
                    {
                        Console.WriteLine("Failed to parse timestamp from line: " + line);
                    }

                    await writer.WriteLineAsync(line);
                    Console.WriteLine("Wrote line: " + line);
                }

            }
        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {

            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                Console.WriteLine("Ignored: {e.FullPath}");
                return;
            }

            counter++;
            Console.WriteLine("Change count: " + counter);
            Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");

        }

        private static void OnAttributedChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"Attribute changed: {e.FullPath} {e.ChangeType}");
        }

        private static void OnLastWriteChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"Last write changed: {e.FullPath} {e.ChangeType}");
        }

    }
}

using System.ComponentModel.DataAnnotations;

namespace mergeSortLogs
{
    internal class Program
    {
        public static List<String> guids = new List<String>();
        public static List<String> lines = new List<String>();
        public static async Task<int> Main(string[] args)

        {

            string unsortedLogs = @"C:\Users\dspencer\code\mergeSortLogs\logs";
            // Determine log folder
            string logFolder = args.Length > 0 ? args[0] : unsortedLogs;
            Watcher(unsortedLogs);

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
            List<String> files = new List<String>();
            if (Directory.Exists(path))
            {

                Console.WriteLine($"{DateTime.Now}: Target Directory: {path}");

                // Get all log files
                foreach (string d in Directory.GetDirectories(path))
                {
                    Console.WriteLine("Dir: " + d);
                    foreach (string f in Directory.GetFiles(d))
                    {
                        Console.WriteLine("Found file in subdir: ");
                        files.Add(f);
                        Console.WriteLine(files.Count + ": " + f);
                    }
                }
            }
            else if (!Directory.Exists(path))
            {
                Console.WriteLine($"Error: Parameter file '{path}' not found.");
                Console.WriteLine("Usage: LoggerApplication [parameterFile]");
                Console.WriteLine("parameterFile: Path to file containing logger parameters (default: params.txt)");
                return new List<string>();
            }
            return files;
        }

        private static void Watcher(string path)
        {
            using var watcher = new FileSystemWatcher();
            watcher.Path = @"C:\Users\dspencer\code\mergeSortLogs\logs";
            watcher.IncludeSubdirectories = true;
            watcher.Changed += OnChanged;
            watcher.NotifyFilter = NotifyFilters.FileName |
                                             NotifyFilters.DirectoryName |
                                             NotifyFilters.LastWrite |
                                             NotifyFilters.Size;

            // Watch all files
            watcher.Filter = ".log";
            watcher.EnableRaisingEvents = true;
        }
        private static async Task WriteLines(List<string> directories)
        {
            foreach (string f in directories)
            {
                // TODO Stream line by line (reading and writing) instead of reading all lines at once
                List<string> filelines = File.ReadAllLines(f).ToList();
                foreach (string fileline in filelines)
                {
                    Console.WriteLine("File Line: " + fileline);
                    string guid = fileline.Split(' ')[2];
                    if (!guids.Contains(guid))
                    {
                        guids.Add(guid);
                        lines.AddRange(fileline);
                        lines.Sort();
                        Console.WriteLine("Adding GUID: " + guid);
                        using (StreamWriter outputFile = new StreamWriter(Path.Combine(@"C:\Users\dspencer\code\sort-merge-log", "sortMergeLogs.txt")))
                        {
                            await outputFile.WriteAsync(string.Join("\n", lines));
                            lines.Sort();
                        }

                    }
                    else
                    {
                        Console.WriteLine($"Duplicate GUID found: {guid}");
                    }

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
            Console.WriteLine($"Changed: {e.FullPath}");
        }

    }
}

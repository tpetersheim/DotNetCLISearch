using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DotNetCLISearch
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication(false);
            app.HelpOption("-h | --help");
            app.Command("search", (searchCommand) =>
            {
                searchCommand.Description = "Search through files for a term";
                var searchArg = searchCommand.Argument("search_term", "The term to search by.", false);


                var searchDirectory = searchCommand.Option("-d | --directory <directory>",
                    "The directory to search through. Defaults to the current directory.", CommandOptionType.SingleValue);
                var caseInsensitive = searchCommand.Option("-i | --case-insensitive", "Search without case sensitivity.", CommandOptionType.NoValue);
                var searchRecursively = searchCommand.Option("-r | --recursive", "Search recursively into sub-folders.", CommandOptionType.NoValue);
                searchCommand.HelpOption("-h | --help");

                searchCommand.OnExecute(() =>
                {
                    if (searchArg.Values.Count == 0)
                    {
                        Console.WriteLine("Search term missing. Aborting search.");
                        searchCommand.ShowHelp();
                        return -1;
                    }

                    var searchDir = GetSearchDirectory(searchDirectory);
                    if (string.IsNullOrEmpty(searchDir))
                    {
                        Console.WriteLine("Invalid search directory. Aborting search.");
                        return -1;
                    }

                    Console.WriteLine($"Searching for {searchArg.Value}");
                    var sw = Stopwatch.StartNew();
                    var resultCount = TextSearch.Search(searchArg.Value, searchDir, caseInsensitive.HasValue(), searchRecursively.HasValue());
                    Console.WriteLine($"Found {resultCount} results in {sw.ElapsedMilliseconds} ms");

                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();

                    return 0;
                });
            });

            app.Command("gentestfiles", (genTestFilesCommand) =>
            {
                genTestFilesCommand.Description = "Generate test files and folders in current directory. Search terms are \"test\" and \"Test\".";
                genTestFilesCommand.HelpOption("-h | --help");
                genTestFilesCommand.OnExecute(() =>
                {
                    const int numDirectoriesBreadth = 200;
                    const int numDirectoriesDepth = 30;
                    string testDirPath = $"{Environment.CurrentDirectory}\\generated_test_files";
                    Directory.CreateDirectory(testDirPath);
                    for (int i = 0; i < numDirectoriesBreadth; i++)
                    {
                        var path = $"{testDirPath}\\folder{i}";
                        Directory.CreateDirectory(path);
                        var subpath = $"{path}\\0";
                        Directory.CreateDirectory(subpath);
                        for (int j = 1; j < numDirectoriesDepth; j++)
                        {
                            subpath += $"\\{j}";
                            Directory.CreateDirectory(subpath);
                            File.WriteAllText($"{subpath}\\no-test-file.txt", String.Concat(Enumerable.Repeat(new String('b', 100) + "\r\n", 2500)));
                            if (j == numDirectoriesDepth - 1)
                            {
                                string test = i % 2 == 0 ? "test" : "Test";
                                File.WriteAllText($"{subpath}\\test-file.txt", String.Concat(Enumerable.Repeat(new String('a', 100) + "\r\n", 2500)) + test);
                            }
                        }
                    }

                    return 0;
                });
            });

            try
            {
                app.Execute(args);
            }
            catch (Exception e)
            {
                Console.WriteLine("Search error: " + e.ToString());
                app.ShowHelp();
            }
        }

        private static string GetSearchDirectory(CommandOption searchDirectory)
        {
            string searchDir = null;

            if (!searchDirectory.HasValue()) // No value. Default to current directory
            {
                searchDir = Environment.CurrentDirectory;
            }
            else if (Directory.Exists(searchDirectory.Value())) // Validate if directory given
            {
                searchDir = searchDirectory.Value();
            }

            return searchDir;
        }
    }
}

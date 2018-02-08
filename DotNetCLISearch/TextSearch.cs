using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DotNetCLISearch
{
    static class TextSearch
    {
        /// <summary>
        /// Searches a given directory for a given term.  Prints results to console
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="searchDirectory"></param>
        /// <param name="caseInsensitive"></param>
        /// <param name="searchRecursively"></param>
        /// <returns>The number of search results</returns>
        public static int Search(string searchTerm, string searchDirectory, Boolean caseInsensitive, Boolean searchRecursively)
        {
            var resultCount = 0;

            if (caseInsensitive)
            {
                searchTerm = searchTerm.ToLower();
            }

            IEnumerable<string> filePaths = Enumerable.Empty<string>();
            try
            {
                filePaths = EnumerateFiles(searchDirectory, searchRecursively);

                foreach (var filePath in filePaths)
                {
                    try
                    {
                        var sw = Stopwatch.StartNew();
                        var matches = SearchFile(filePath, searchTerm, caseInsensitive);
                        resultCount += matches.Count();
                        foreach (var match in matches)
                        {
                            var filePathLessBase = filePath.Replace(searchDirectory + "\\", "");
                            var line = match.Value.Trim();
                            if (line.Length > 100) {
                                line = line.Substring(0, 100) + "...";
                            }
                            Console.WriteLine($"\"{searchTerm}\" found. Time {sw.ElapsedMilliseconds} ms. {filePathLessBase}:{match.Key}  {line}");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error searching file. {e.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.ToString());
            }
            Console.WriteLine($"Searched {filePaths.Count()} files");

            return resultCount;
        }

        private static Dictionary<int, string> SearchFile(string filePath, string searchTerm, Boolean caseInsensitive)
        {
            var matches = new Dictionary<int, string>();

            using (StreamReader stream = File.OpenText(filePath))
            {
                string line;
                int i = 0;
                while ((line = stream.ReadLine()) != null)
                {
                    i++;
                    var comparison = caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                    if (line.IndexOf(searchTerm, comparison) > -1)
                    {
                        matches.Add(i, line);
                    }
                }
            }

            return matches;
        }

        /**
         * Directory.EnumerateFiles fails when it can't read a file/folder
         */
        private static IEnumerable<string> EnumerateFiles(string folderPath, Boolean enumerateRecursively)
        {
            var files = getFiles(folderPath);

            for (int i = 0; i < files.Count(); i++)
            {
                yield return files.ElementAt(i);
            }

            if (enumerateRecursively)
            {
                var directories = getDirectories(folderPath);

                for (int i = 0; i < directories.Count(); i++)
                {
                    foreach (var file in EnumerateFiles(directories.ElementAt(i), true))
                    {
                        yield return file;
                    }
                }
            }
        }

        private static string[] getFiles(string folderPath)
        {
            var files = new string[0];

            try
            {
                files = Directory.GetFiles(folderPath);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error getting files. {e.Message}");
            }

            return files;
        }

        private static string[] getDirectories(string folderPath)
        {
            var directories = new string[0];

            try
            {
                directories = Directory.GetDirectories(folderPath);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error getting directories. {e.Message}");
            }

            return directories;
        }
    }
}

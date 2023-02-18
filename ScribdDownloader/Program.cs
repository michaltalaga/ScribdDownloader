using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.Json.Nodes;

namespace ScribdDownloader
{
    class Program
    {
        static async Task Main()
        {
            var title = GetInput("Title (leave empty to name files with just numbers)");
            var fileNamePattern = "{0}";
            if (!string.IsNullOrWhiteSpace(title))
            {
                fileNamePattern = fileNamePattern + " - " + GetSafeFileName(title);
            }

            fileNamePattern += ".mp3";

            var playlistJson = GetInput("Playlist JSON");
            var playlistFiles = GetFilesToDownloadFromPlaylistJson(playlistJson);
            await DownloadFiles(fileNamePattern, playlistFiles);
            Console.WriteLine("Hello World!");
        }

        private static async Task DownloadFiles(string fileNamePattern, IEnumerable<PlaylistFile> playlistFiles)
        {
            using var client = new HttpClient();
            foreach (var item in playlistFiles)
            {
                var localFileName = string.Format(fileNamePattern, item.Chapter + item.Part);
                
                Console.Write(localFileName);
                var remoteFile = await client.GetByteArrayAsync(item.Url);
                Console.Write($" ({remoteFile.Length})");
                
                
                File.WriteAllBytes(localFileName, remoteFile);
                Console.WriteLine(" - DONE");
            }
        }

        private static IEnumerable<PlaylistFile> GetFilesToDownloadFromPlaylistJson(string playlistJson)
        {
            var playlist = JsonNode.Parse(playlistJson);
            var playListItems = playlist["playlist"];
            return playListItems.AsArray().Select(x => new PlaylistFile((string)x["url"], (int)x["chapter_number"], (int)x["part_number"]));
        }
        record PlaylistFile (string Url, int Chapter, int Part);
        private static string GetInput(string label, bool mask = false)
        {
            Console.Write(label + ": ");
            if (mask)
            {
                string pass = "";
                do
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    // Backspace Should Not Work
                    if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                    {
                        pass += key.KeyChar;
                        Console.Write("*");
                    }
                    else
                    {
                        if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                        {
                            pass = pass.Substring(0, (pass.Length - 1));
                            Console.Write("\b \b");
                        }
                        else if (key.Key == ConsoleKey.Enter)
                        {
                            break;
                        }
                    }
                } while (true);
                Console.WriteLine();
                return pass;
            }
            return Console.ReadLine();
        }

      
    
        private static string GetSafeFileName(string file)
        {
            var sb = new StringBuilder();
            var validCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890_- ".ToCharArray();
            foreach (var c in file)
            {
                if (validCharacters.Contains(c)) sb.Append(c);
            }
            return sb.ToString();
        }
    
   
    }
}
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace ERTimeCurvesToGOD
{
    class Program
    {
        const string DEFAULT_FILE = "1.txt";

        static void Main(string[] args)
        {
            Console.Title = "Easy Refraction Time Curves To Godograph format";

            string path = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            string text = string.Empty;
            string[] lines;
            string file = string.Empty;
            bool fileFound = false;

            if (args.Length != 0)
            {
                try
                {
                    file = args.FirstOrDefault();
                    if (file != null && File.Exists(file))
                        text = File.ReadAllText(file);

                    fileFound = true;
                }
                catch { Console.WriteLine($"Cant read file from args, will find 1.txt"); }
            }

            while (!fileFound)
            {
                try
                {
                    file = Directory.GetFiles(path).FirstOrDefault(c => Path.GetFileName(c).Equals(DEFAULT_FILE));
                    if (file == null)
                        throw new Exception();

                    text = File.ReadAllText(file);
                    if (string.IsNullOrEmpty(text))
                        throw new Exception();

                    break;
                }
                catch
                {
                    Console.WriteLine("The file 1.txt cannot be found or is empty. Press any key to check again");
                    Console.ReadKey();
                }
            }
            Console.WriteLine($"Reading {file}...");
            if (text.Contains("SOU_X:REC_X"))
            {
                text = text.Replace("SOU_X:REC_X\r\n", "");
				text = text.Substring(1, text.Length - 1);
                text = text.Replace("\r\n\t", "\r\n");
                text = text.Replace(":\t", "\t");
            }

            text = text.Replace('\t', ' ');
            text = text.Replace(',', '.');
            lines = text.Split('\n');

            path = Path.Combine(path, "g");
            try { Directory.CreateDirectory(path); }
            catch { ExitWithMessage("Cannot create output directory."); }

            string souX = string.Empty;
            string prevSoux = string.Empty;
            string line;
            List<string> hod = new List<string>();
            List<string> fileNames = new List<string>();
            int k = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                line = lines[i];
                if (string.IsNullOrEmpty(line))
                    continue;

                prevSoux = line.Split(' ')[0];
                k = i;
                break;
            }

            for (int i = k; i < lines.Length; i++)
            {
                line = lines[i];
                if (string.IsNullOrEmpty(line))
                    continue;

                souX = line.Split(' ')[0];

                if (prevSoux != souX)
                {
                    file = $"{prevSoux}.txt";
                    if (ChangeHodAndWrite(Path.Combine(path, file), prevSoux, hod))
                        fileNames.Add(file);

                    prevSoux = souX;
                    hod.Clear();
                }

                hod.Add(line);
            }

            file = $"{souX}.txt";
            if (ChangeHodAndWrite(Path.Combine(path, file), souX, hod))
                fileNames.Add(file);

            if (WriteInputFile(Path.Combine(path, "i.txt"), fileNames))
                fileNames.Add("i.txt");

            if (CreateReliefFile(Path.Combine(path, "r.txt")))
                fileNames.Add("r.txt");

            if (fileNames.Count == 0)
                ExitWithMessage("No files were created.");

            Console.WriteLine("SUCCESS! Created:");
            Console.WriteLine(string.Join('\n', fileNames));
            ExitWithMessage();
        }

        private static void ExitWithMessage(string msg = "")
        {
            string pressKey = "Press any key to exit...";

            if (string.IsNullOrEmpty(msg))
                Console.WriteLine(pressKey);
            else
                Console.WriteLine($"{msg} {pressKey}");

            Console.ReadKey();
            Environment.Exit(0);
        }


        private static bool ChangeHodAndWrite(string path, string souX, List<string> hod)
        {
            if (hod.Count == 0)
                return false;

            var textToDelete = $"{souX} ";
            int firstOcur = hod[0].IndexOf(souX);
            hod[0] = hod[0].Substring(firstOcur + textToDelete.Length, hod[0].Length - firstOcur - textToDelete.Length);

            var text = string.Join('\n', hod);
            text = text.Replace($"\r\n{souX} ", "\r\n");
            text = $"{souX}\r\n{souX}\r\n{text.Substring(0, text.Length - 1)}";

            try { File.WriteAllText(path, text); }
            catch { Console.WriteLine($"File {path} - Error"); return false; }

            return true;
        }

        private static bool WriteInputFile(string path, List<string> fileNames)
        {
            try { File.WriteAllText(path, string.Join("\r\n", fileNames)); }
            catch { Console.WriteLine($"File {path} - Error"); return false; }

            return true;
        }

        private static bool CreateReliefFile(string path)
        {
            var content = @"2
0 0
1000 0";
            try { File.WriteAllText(path, content); }
            catch { Console.WriteLine($"File {path} - Error"); return false; }

            return true;

        }

    }
}

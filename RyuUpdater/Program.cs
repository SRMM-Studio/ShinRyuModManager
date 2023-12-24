using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using YamlDotNet.Serialization;

namespace RyuUpdater
{
    internal class Program
    {
        class Options
        {
            [Option('v', "srmmversion", Required = true, HelpText = "Shin Ryu Mod Manager version.")]
            public string Version { get; set; }

            [Option('b', "branch", Default = "release", HelpText = "Branch to download.")]
            public string BranchName { get; set; }

            [Option('k', "key", Default = null, HelpText = "Branch key.")]
            public string BranchKey { get; set; }

            [Option('c', "checkonly", Default = false, HelpText = "Only check if an update is available.")]
            public bool CheckOnly { get; set; }
        }


        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
            {
                Console.WriteLine("Fetching branch data...");
                Branch branch = GetBranchData(o.BranchName, o.BranchKey);
                Console.WriteLine("Comparing versions...");
                bool isHigher = Util.CompareVersionIsHigher(branch.Version, o.Version);
                if (isHigher || branch.IgnoreInstalledVersion)
                {
                    if (o.CheckOnly)
                    {
                        Console.WriteLine("Writing flag file...");
                        string exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                        File.WriteAllText(Path.Combine(exePath, "update.txt"), branch.Version);
                        return;
                    }

                    Console.WriteLine("Downloading update...");
                    DownloadUpdate(branch);
                    Console.WriteLine("Extracting update files...");
                    ExtractUpdate();
                }
            });
        }


        private static Branch GetBranchData(string branch, string key)
        {
            WebClient client = new WebClient();
            string yamlString = client.DownloadString($"https://raw.githubusercontent.com/{Settings.RepoOwner}/{Settings.RepoName}/main/{Settings.RepoPath}");

#if DEBUG
            Console.WriteLine(yamlString);
#endif

            var deserializer = new DeserializerBuilder().Build();
            var yamlObject = deserializer.Deserialize<Dictionary<string, Dictionary<string, Branch>>>(yamlString);

            if (yamlObject.TryGetValue("branches", out Dictionary<string, Branch> branchCollection))
            {
                if (branchCollection.ContainsKey(branch)) 
                {
                    Branch br = branchCollection[branch];
                    if (br.Key == key)
                    {
                        return br;
                    }
                    else
                    {
                        throw new Exception("The supplied key does not match the branch key.");
                    }
                }
                else
                {
                    throw new Exception($"The branch '{branch}' does not exist.");
                }
            }
            else
            {
                throw new Exception("The config file does not contain branch information.");
            }

        }


        private static void DownloadUpdate(Branch branch)
        {
            Directory.CreateDirectory(Settings.PathTempFolder);
            using (var client = new WebClient())
            {
                client.DownloadFile(branch.Download, Settings.PathTempUpdateFile);
                Console.WriteLine(Path.Combine(Settings.PathTempFolder, Settings.PathTempUpdateFile));
            }
        }


        private static void ExtractUpdate()
        {
            string exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            using (FileStream zipToOpen = new FileStream(Settings.PathTempUpdateFile, System.IO.FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    archive.ExtractToDirectory(exePath, true);
                }
            }

            if (File.Exists(Settings.PathTempUpdateFile))
            {
                File.Delete(Settings.PathTempUpdateFile);
            }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using YamlDotNet.Serialization;

namespace ShinRyuModManager
{
    public class LibMeta
    {
        public Guid GUID { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }

        public string Author { get; set; }

        public string Description { get; set; }

        public bool CanBeDisabled { get; set; }

        public bool IsDisabled { get; set; }

        /// <summary>
        /// A semicolon (;) separated list of game executable names.
        /// </summary>
        public string TargetGames { get; set; }

        public string Source { get; set; }

        public string Download { get; set; }

        public string MainBinary { get; set; }



        /// <summary>
        /// Reads the <see cref="LibMeta"/> from a YAML string.
        /// </summary>
        /// <returns>A <see cref="LibMeta"/> object.</returns>
        public static LibMeta ReadLibMeta(string yamlString)
        {
            var deserializer = new DeserializerBuilder().Build();
            LibMeta meta = deserializer.Deserialize<LibMeta>(yamlString);
            return meta;
        }


        public static List<LibMeta> Fetch()
        {
            WebClient client = new WebClient();
            //TODO UNCOMMENT THIS WHEN THE REPO IS READY. READING FROM LOCAL FILE IN THE MEANTIME
            string yamlString = client.DownloadString($"https://raw.githubusercontent.com/{Settings.LIBRARIES_INFO_REPO_OWNER}/{Settings.LIBRARIES_INFO_REPO}/main/{Settings.LIBRARIES_INFO_REPO_FILE_PATH}");
            //string yamlString = File.ReadAllText(Program.GetLocalLibraryCopyPath());

            // Save a copy of the manifest for offline use
            string localManifestCopyPath = Program.GetLocalLibraryCopyPath();
            if (!File.Exists(localManifestCopyPath) && !Util.IsFileBlocked(localManifestCopyPath))
            {
                File.WriteAllText(localManifestCopyPath, yamlString);
            }

            return LibMeta.ReadLibMetaManifest(yamlString);
        }

        /// <summary>
        /// Reads the <see cref="LibMeta"/> manifest from a YAML string.
        /// </summary>
        /// <returns>A <see cref="LibMeta"/> list.</returns>
        public static List<LibMeta> ReadLibMetaManifest(string yamlString)
        {
            List<LibMeta> returnList = new List<LibMeta>();

            var deserializer = new DeserializerBuilder().Build();
            var yamlObject = deserializer.Deserialize<Dictionary<string, LibMeta>>(yamlString);
            foreach (string key in yamlObject.Keys)
            {
                LibMeta meta = yamlObject[key];
                meta.GUID = new Guid(key);
                returnList.Add(meta);
            }

            Program.LibraryMetaCache = returnList;

            return returnList;
        }
    }
}

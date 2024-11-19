using System;

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
    }
}

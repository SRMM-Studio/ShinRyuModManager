using System.Collections.Generic;

namespace ShinRyuModManager.Templates
{
    public struct IniSection
    {
        public string Name { get; set; }

        public List<string> Comments { get; set; }

        public List<IniKey> Keys { get; set; }
    }
}

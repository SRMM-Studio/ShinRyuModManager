namespace ShinRyuModManager
{
    class ModMeta
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string Dependencies { get; set; }


        public static ModMeta GetPlaceholderModMeta()
        {
            ModMeta meta = new ModMeta()
            {
                Name = "Mod Name",
                Author = "Author",
                Version = "1.0.0",
                Description = "",
                Dependencies = ""
            };

            return meta;
        }


        public static ModMeta GetPlaceholderModMeta(string modName)
        {
            ModMeta meta = GetPlaceholderModMeta();
            meta.Name = modName;
            return meta;
        }
    }
}

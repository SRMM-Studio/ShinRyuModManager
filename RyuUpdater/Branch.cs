namespace RyuUpdater
{
    public class Branch
    {
        public string Key { get; set; }
        public string Version { get; set; }
        public bool IgnoreInstalledVersion { get; set; }
        public string Download { get; set; }
    }
}

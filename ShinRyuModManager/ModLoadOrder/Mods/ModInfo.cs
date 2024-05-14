using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Utils;

namespace ShinRyuModManager.ModLoadOrder.Mods
{
    public class ModInfo : IEqualityComparer<ModInfo>, INotifyPropertyChanged
    {
        private string name;
        private bool enabled;

        public string Name
        {
            get => this.name;

            set
            {
                this.name = value;
                NotifyPropertyChanged();
            }
        }

        public bool Enabled
        {
            get => this.enabled;

            set
            {
                this.enabled = value;
                NotifyPropertyChanged();
            }
        }

        public ModInfo(string name, bool enabled = true)
        {
            this.Name = name;
            this.Enabled = enabled;
        }

        public void ToggleEnabled()
        {
            this.Enabled = !this.Enabled;
        }

        public bool Equals(ModInfo x, ModInfo y)
        {
            return x?.Name == y?.Name;
        }

        public int GetHashCode(ModInfo obj)
        {
            return obj.GetHashCode();
        }

        public static bool IsValid(ModInfo info)
        {
            return (info != null) && Directory.Exists(Path.Combine(GamePath.MODS, info.Name));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

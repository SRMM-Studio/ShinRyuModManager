using ShinRyuModManager.ModLoadOrder;
using Utils;

namespace ShinRyuModManager
{
    //Purpose: Replace files in vanilla game data folder, while backing them up
    //Original files are restored once the file is no longer used by a mod.
    //This is required to make profile.par moddable in Yakuza 5.
    internal static class ParReplacer
    {
        public static string BackupPath 
        { 
            get 
            {
                string dataPath = GamePath.DataPath;

                if (string.IsNullOrEmpty(dataPath))
                    return null;

                return Path.Combine(dataPath, "SRMM_DATA_BACKUP");
            }
        }


        public static void CreateBackup(string file)
        {
            FileInfo fileInf = new FileInfo(file);

            string relativePath = Path.GetRelativePath(GamePath.DataPath, fileInf.Directory.FullName);
            string dirPath = Path.Combine(BackupPath, relativePath);
            string filePath = Path.Combine(dirPath, fileInf.Name);

            //no need
            if (File.Exists(filePath))
                return;

            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            fileInf.CopyTo(filePath, true);
        }

        public static void RestoreAll()
        {
            string backupFolderPath = BackupPath;

            if (string.IsNullOrEmpty(backupFolderPath))
                return;

            DirectoryInfo backupFolder = new DirectoryInfo(backupFolderPath);

            if (!backupFolder.Exists)
                return;

            Utils.MoveDirectory(backupFolder.FullName, GamePath.DataPath);

            backupFolder.Delete(true);
        }

        //Restore files not in mlo
        public static void RestoreFromMLO(MLO mlo)
        {
            string backupFolderPath = BackupPath;

            if (string.IsNullOrEmpty(backupFolderPath))
                return;

            DirectoryInfo backupFolder = new DirectoryInfo(backupFolderPath);

            if (!backupFolder.Exists)
                return;

            foreach(string file in Directory.GetFiles(backupFolderPath, "*", SearchOption.AllDirectories))
            {
                string relPath = Path.GetRelativePath(backupFolderPath, file);

                bool anyModsContains = mlo.Mods.Any(x => File.Exists(Path.Combine(GamePath.ModsPath, x, relPath)));

                if(!anyModsContains)
                {
                    string targetPath = Path.Combine(GamePath.DataPath, relPath);
                    File.Move(file, targetPath, true);
                }

            }

            if (Directory.GetFiles(backupFolderPath, "*", SearchOption.AllDirectories).Length <= 0)
                new DirectoryInfo(backupFolderPath).Delete(true);
        }
    }
}
